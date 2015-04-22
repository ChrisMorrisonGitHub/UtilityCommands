using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UniversalBinary.CoreApplicationSupport;
using NAudio;
using NAudio.Wave;


namespace SoundFileConverter
{
    class Program
    {
        private static long filesSkipped = 0;
        private static long filesExamined = 0;
        private static long filesConverted = 0;
        private static bool searchSystem = false;
        private static bool _printSummary = false;
        private static bool verbose = false;
        private static DirectorySearcher ds;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: {0} <search folder> [/V] [/H] [/I] [/S]", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Environment.Exit(-1);
                return;
            }
            if ((args[0] == "/?") || (args[0] == "/h") || (args[0] == "/H"))
            {
                Console.WriteLine("Searches the given folder for images and converts them to high-quality TIFF files.\n");
                Console.WriteLine("{0} <search folder> [/S] [/H] [/I] [/S]\n", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Console.WriteLine("  <search folder>");
                Console.WriteLine("\tSpecifies the folder to search for images to convert.\n");
                Console.WriteLine("  /V\tPrint additional information.\n");
                Console.WriteLine("  /H\tInclude system and hidden files and folders.\n");
                Console.WriteLine("  /S\tPrint a summary when finished.\n");
                Environment.Exit(0);
            }
            string destPath = args[0];
            if (Directory.Exists(destPath) == false)
            {
                Console.Error.WriteLine("Error: {0} is not a valid search location.", destPath);
                Environment.Exit(-1);
                return;
            }
            foreach (string s in args)
            {
                if (s.ToLower() == "/h") searchSystem = true;
                if (s.ToLower() == "/s") _printSummary = true;
                if (s.ToLower() == "/v") verbose = true;
            }


            ds = new DirectorySearcher(args[0], SearchOption.AllDirectories, null);
            ds.FileFound += ds_FileFound;
            ds.DirectoryFound += ds_DirectoryFound;
            ds.OperationEnded += ds_SearchEnded;
            ds.OperationError += ds_SearchError;
            ds.EventMask = DirectorySearchEventMask.Files;
            ds.Start();
        }

        private static void PrintSummary()
        {
            Console.WriteLine("{0}------------------------------------------------", Console.Out.NewLine);
            Console.WriteLine("Directories searched   " + String.Format("{0:n0}", ds.DirectoriesSearched).PadLeft(25));
            Console.WriteLine("Files examined         " + String.Format("{0:n0}", ds.FilesFound).PadLeft(25));
            Console.WriteLine("Files skipped          " + String.Format("{0:n0}", filesSkipped).PadLeft(25));
            Console.WriteLine("Files converted        " + String.Format("{0:n0}", filesConverted).PadLeft(25));
            Console.WriteLine("------------------------------------------------");
        }

        static void ds_SearchEnded(object sender, OperationEndedEventArgs e)
        {
            switch (e.Reason)
            {
                case DirectroryOperationEndReason.Cancelled:
                    Console.WriteLine("Operation cancelled");
                    Environment.Exit(0);
                    break;
                case DirectroryOperationEndReason.FatalError:
                    Environment.Exit(-1);
                    break;
                case DirectroryOperationEndReason.Finished:
                    if (_printSummary == true) PrintSummary();
                    Environment.Exit(0);
                    break;
            }
        }

        static void ds_DirectoryFound(object sender, DirectoryFoundEventArgs e)
        {

        }

        static void ds_FileFound(object sender, FileFoundEventArgs e)
        {
            string file = e.File.FullName;
            if ((searchSystem == false) && (FileUtilities.FileIsInHiddenOrSystemDirectory(file) == true))
            {
                if (verbose == true) Console.WriteLine("Skipping '{0}' as it is in a system or hidden folder.", file);
                filesSkipped++;
                return;
            }
            if (ConvertSoundFile(file) == true)
            {
                filesConverted++;
                filesExamined++;
                Console.WriteLine("Successfully converted '{0}'", file);
            }
            else
            {
                if (verbose == true) Console.WriteLine("The file '{0}' was not converted.", file);
            }
            filesExamined++;
        }

        private static bool ConvertSoundFile(string inFile)
        {
            string outFileStub = Path.GetFileNameWithoutExtension(inFile);
            string outFile = outFileStub + ".wav";
            Random rnd = new Random();
            AudioFileReader audioReader = null;

            try
            {
                audioReader = new AudioFileReader(inFile);
                if (File.Exists(outFile) == true)
                {
                    if (FileUtilities.FileAndStreamAreIdentical(outFile, audioReader) == true)
                    {
                        audioReader.Close();
                        return false;
                    }
                    else
                    {
                        outFile = outFileStub + rnd.Next(0, 10000).ToString("D5") + ".wav";
                    }
                }
                WaveFormat outFormat = new WaveFormat(48000, 32, 2);
                MediaFoundationResampler resampler = new MediaFoundationResampler(audioReader, outFormat);
                resampler.ResamplerQuality = 60;
                WaveFileWriter.CreateWaveFile(outFile, resampler);
                audioReader.Close();
            }
            catch
            {
                if (audioReader != null) audioReader.Close();
                return false;
            }

            File.Delete(inFile);
            return true;
        }

        static void ds_SearchError(object sender, OperationErrorEventArgs e)
        {
            Console.Error.WriteLine("ERROR: Could not search '{0}' {1}", e.Directory, e.Error);
        }
    }
}
