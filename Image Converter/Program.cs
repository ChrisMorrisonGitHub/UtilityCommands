using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBinary.CoreApplicationSupport;

namespace ImageConverter
{
    class Program
    {
        const bool OLD_CODE = false;
        private static long filesSkipped = 0;
        private static long filesExamined = 0;
        private static long filesConverted = 0;
        private static bool searchSystem = false;
        private static bool convertICOFiles = false;
        private static bool _printSummary = false;
        private static bool verbose = false;
        private static bool _compress = false;
        private static DirectorySearcher ds;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: {0} <search folder> [/V] [/H] [/I] [/C] [/S]", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Environment.Exit(-1);
                return;
            }
            if ((args[0] == "/?") || (args[0] == "/h") || (args[0] == "/H"))
            {
                Console.WriteLine("Searches the given folder for images and converts them to high-quality TIFF files.\n");
                Console.WriteLine("{0} <search folder> [/V] [/H] [/I] [/C] [/S]\n", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Console.WriteLine("  <search folder>");
                Console.WriteLine("\tSpecifies the folder to search for images to convert.\n");
                Console.WriteLine("  /V\tPrint additional information.\n");
                Console.WriteLine("  /H\tInclude system and hidden files and folders.\n");
                Console.WriteLine("  /I\tConvert Windows icon (.ico) files.\n");
                Console.WriteLine("  /C\tApply lossless compression to the converted Image\n");
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
                if (s.ToLower() == "/i") convertICOFiles = true;
                if (s.ToLower() == "/c") _compress = true;
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
            if (ImageUtilities.ConvertFileToTIFF(file, convertICOFiles, _compress) == true)
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

        static void ds_SearchError(object sender, OperationErrorEventArgs e)
        {
            Console.Error.WriteLine("ERROR: Could not search '{0}' {1}", e.Directory, e.Error);
        }
    }
}
