using Bulletfish.MKVCommercialSkip.Helpers;
using System;
using System.IO;

namespace Bulletfish.MKVCommercialSkip
{
    public class Program
    {
        static void Main(string[] argv)
        {
            // - Comskip -> edl file (temp)
            // -Convert EDL->XML Chapters(see test.xml)
            // - Mux XML + MKV->MKV w / Chapters

            // Params:
            // -xml {fileName}

            //TODO: Potentially inject?
            var chapterGenerator = new ComskipChapterFileGenerator();
            var muxer = new MkvToolNixChapterMuxer();
            var output = Console.Out;

            var args = ArgumentExtractionHelper.ExtractArguments(argv);
            FileInfo file = null;
            if (args.ContainsKey(Argument.File))
            {
                file = new FileInfo(args[Argument.File]);
                if (!file.Exists || file.Extension != ".mkv")
                    file = null;
            }

            if (file == null)
            {
                Console.WriteLine("File is required for processing and must be a valid file");
                Console.WriteLine("Press any key to continue.");

                if (!args.ContainsKey(Argument.Log))
                    Console.ReadKey();

                Environment.Exit(-1);
            }

            if (args.ContainsKey(Argument.Log))
                output = File.CreateText($"{file.Directory.FullName}\\{file.Name.Substring(0, file.Name.Length - 3)}log");

            try
            {
                var xmlFile = chapterGenerator.GetChapterFileContents(args, file, output);
                muxer.MuxChapters(args, file, xmlFile, output);

                if (!args.ContainsKey(Argument.Log))
                    Console.ReadKey();

                output.Flush();
                output.Close();
            } 
            catch (Exception ex)
            {
                output.WriteLine(ex.Message);
                output.WriteLine("Press any key to continue.");

                if (!args.ContainsKey(Argument.Log))
                    Console.ReadKey();

                output.Flush();
                output.Close();

                Environment.Exit(-1);
            }

            //Make sure the stream is closed (safeguard)
            try
            {
                output.Flush();
                output.Close();
            }
            catch { }
        }
    }
}
