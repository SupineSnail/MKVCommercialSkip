using System;
using System.Collections.Generic;
using System.IO;
using Bulletfish.MKVCommercialSkip.Helpers;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using Bulletfish.MKVCommercialSkip.XmlStructure;

namespace Bulletfish.MKVCommercialSkip
{
    public class ComskipChapterFileGenerator : IGenerateChapterFiles
    {
        private const string COMSKIP_PARAMS = "\"{0}\" --output=\"{1}\"";
        private const string COMSKIP_EXE = "\\Dependencies\\Comskip\\comskip.exe";

        public string GetChapterFileContents(Dictionary<Argument, string> arguments, FileInfo file, TextWriter output)
        {
            //If XML file, nothing to do but load
            if (arguments.ContainsKey(Argument.XmlFile))
            {
                output.WriteLine("Found passsed in XML File, loading...");
                return GetXmlFromFile(arguments[Argument.XmlFile], output);
            }

            return GetXmlFromComskip(file, output);
        }

        private string GetXmlFromFile(string filePath, TextWriter output)
        {
            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                output.WriteLine($"EXCEPTION: Could not find file {filePath}");
                throw new InvalidOperationException($"Could not find file {filePath}");
            }

            return file.OpenText().ReadToEnd();
        }

        private string GetXmlFromComskip(FileInfo file, TextWriter output)
        {
            var comskipLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            comskipLocation += COMSKIP_EXE;

            var tempGuid = Guid.NewGuid();
            var tempPath = Path.GetTempPath() + tempGuid.ToString();

            Directory.CreateDirectory(tempPath);

            var args = string.Format(COMSKIP_PARAMS, file.FullName, tempPath);
            var startInfo = new ProcessStartInfo(comskipLocation, args)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            var proc = new Process
            {
                StartInfo = startInfo
            };

            output.WriteLine($"Calling Comskip: \"{comskipLocation}\" {args}");

            proc.OutputDataReceived += (sender, a) => UpdateOutput(output, a.Data);
            proc.ErrorDataReceived += (sender, a) => UpdateOutput(output, a.Data); 

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            proc.CancelOutputRead();
            proc.CancelErrorRead();

            //Load the file created
            var edlFile = new FileInfo(tempPath + "\\" + file.Name.Substring(0, file.Name.Length - 3) + "edl");
            if (!edlFile.Exists)
                throw new InvalidDataException("Cannot find generated EDL file. Please check comskip.ini to make sure EDL files are being generated");

            //Transform to XML structure
            var xmlStructure = TransformEdl(edlFile, output);

            //Create our own namespaces for the output (no namespaces)
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var serializer = new XmlSerializer(typeof(MatroskaChapterFile));
            var writer = new StringWriter();

            serializer.Serialize(writer, xmlStructure, ns);

            var xmlText = writer.ToString();
            //Manually remove encoding from xml as doing it the 'correct' way is a pain
            xmlText = xmlText.Replace("encoding=\"utf-16\"", "");

            output.WriteLine();
            output.Write($"Generated XML: {Environment.NewLine}{xmlText}");

            //Clean up temp directory
            output.WriteLine();
            output.Write("Cleaning up temporary directory used for Comskip");
            Directory.Delete(tempPath, true);

            //Return XML
            return xmlText;
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

        private MatroskaChapterFile TransformEdl(FileInfo edlFile, TextWriter output)
        {
            var randomGen = new Random(DateTime.Now.Millisecond);

            var edition = new EditionEntry
            {
                Chapters = new List<ChapterAtom>(),
                EditionFlagDefault = "0",
                EditionFlagHidden = "0",
                EditionUID = Math.Floor((randomGen.NextDouble() * (9999999999999999999d - 1000000000000000000d) + 1000000000000000000d)).ToString("###################")
            };

            var fileReader = edlFile.OpenText();
            var chapterNo = 1;
            while (!fileReader.EndOfStream)
            {
                var line = fileReader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var elements = line.Split('\t');
                if (elements.Length < 2)
                    continue;

                edition.Chapters.Add(new ChapterAtom
                {
                    ChapterFlagEnabled = "1",
                    ChapterFlagHidden = "0",
                    ChapterUID = Math.Floor((randomGen.NextDouble() * (9999999999999999999d - 1000000000000000000d) + 1000000000000000000d)).ToString("###################"),
                    Start = GetTimespan(elements[1]),
                    Display = new ChapterDisplay
                    {
                        Language = "eng",
                        Title = $"Chapter {chapterNo}"
                    }
                });

                chapterNo++;
            }

            fileReader.Close();
            return new MatroskaChapterFile { edition };
        }

        private TimeSpan GetTimespan(string timeString)
        {
            var split = timeString.Split('.');
            var seconds = int.Parse(split[0]);
            var milliseconds = int.Parse(split[1] + "0");

            return new TimeSpan(0, 0, 0, seconds, milliseconds);
        }
    }
}
