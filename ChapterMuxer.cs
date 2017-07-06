using Bulletfish.MKVCommercialSkip.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Bulletfish.MKVCommercialSkip
{
    public interface IChapterMuxer
    {
        void MuxChapters(Dictionary<Argument, string> args, FileInfo file, string xmlFile, TextWriter output);
    }

    public class MkvToolNixChapterMuxer : IChapterMuxer
    {
        private const string MKVTOOLKIT_PARAMS = "-o \"{0}\" --chapters \"{1}\" \"{2}\"";
        private const string MKVTOOLKIT_EXE = "\\Dependencies\\MKVToolNix\\mkvmerge.exe";

        public void MuxChapters(Dictionary<Argument, string> arguments, FileInfo file, string xmlFile, TextWriter output)
        {
            // Save the XML as a file in a temporary folder
            var tempGuid = Guid.NewGuid();
            var tempPath = Path.GetTempPath() + tempGuid.ToString();
            var tempFileName = tempPath + "\\" + file.Name.Substring(0, file.Name.Length - 3) + "mux.mkv";
            var chapterFileName = tempPath + "\\" + file.Name.Substring(0, file.Name.Length - 3) + "xml";

            Directory.CreateDirectory(tempPath);

            SaveXmlFile(chapterFileName, xmlFile, output);
            RunMkvMerge(tempFileName, chapterFileName, file, output);
            OverwriteMkv(tempFileName, file, output);
            
            output.WriteLine();
            output.Write("Cleaning up temporary directory used for Mux");
            Directory.Delete(tempPath, true);

        }

        private void SaveXmlFile(string chapterFileName, string xmlFile, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("Writing XML to temporary file before muxing");

            var tempFile = new FileInfo(chapterFileName);
            var tempFileStream = tempFile.CreateText();
            tempFileStream.Write(xmlFile);
            tempFileStream.Flush();
            tempFileStream.Close();
        }

        private void RunMkvMerge(string tempFileName, string chapterFileName, FileInfo file, TextWriter output)
        {
            var appLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            appLocation += MKVTOOLKIT_EXE;

            var args = string.Format(MKVTOOLKIT_PARAMS, tempFileName, chapterFileName, file.FullName);
            var startInfo = new ProcessStartInfo(appLocation, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var proc = new Process
            {
                StartInfo = startInfo
            };

            output.WriteLine();
            output.WriteLine($"Passing XML into mux, command: \"{appLocation}\" {args}");

            proc.OutputDataReceived += (sender, a) => UpdateOutput(output, a.Data);
            proc.ErrorDataReceived += (sender, a) => UpdateOutput(output, a.Data);

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            proc.CancelOutputRead();
            proc.CancelErrorRead();
        }

        private void OverwriteMkv(string tempFileName, FileInfo file, TextWriter output)
        {
            output.WriteLine();
            output.WriteLine("Overwriting source MKV");

            var newFile = new FileInfo(tempFileName);
            newFile.CopyTo(file.FullName, true);
        }

        private void UpdateOutput(TextWriter output, string data)
        {
            //Add line breaks (output sometimes updates lines instead of new line, so add new line)
            if (data == null)
                return;

            var formattedData = data.Trim();
            if (formattedData != null && (!formattedData.StartsWith(Environment.NewLine) || !formattedData.EndsWith(Environment.NewLine)))
                output.WriteLine(data);
            else
                output.Write(data);
        }
    }
}
