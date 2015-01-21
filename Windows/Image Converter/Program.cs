using System;
using System.Collections.Generic;
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
        private static bool printSummary = false;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Usage: {0} <search folder> [/S]", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Environment.Exit(-1);
                return;
            }
            if ((args[0] == "/?") || (args[0] == "/h") || (args[0] == "/H"))
            {
                Console.WriteLine("Searches the given folder for images and converts them to high-quality TIFF files.\n");
                Console.WriteLine("{0} <search folder> [/S]\n", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Console.WriteLine("  <search folder>");
                Console.WriteLine("\tSpecifies the folder to search for images to convert.\n");
                Console.WriteLine("  /H\tInclude system and hidden files and folders.\n");
                Console.WriteLine("  /I\tConvert Windows icon (.ico) files.\n");
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
                if (s.ToLower() == "/s") printSummary = true;
            }

            if (OLD_CODE)
            {
                try
                {
                    foreach (string file in FileUtilities.GetDirectoryEntries(args[0], "*.*", SearchOption.AllDirectories, EntryFetchOptions.FilesOnly))
                    {
                        if ((searchSystem == false) && (FileUtilities.FileIsInHiddenOrSystemDirectory(file) == true))
                        {
                            filesSkipped++;
                            continue;
                        }
                        if (ImageUtilities.ConvertFileToTIFF(file, convertICOFiles) == true)
                        {
                            filesConverted++;
                            filesExamined++;
                            Console.WriteLine("Successfully converted {0}", file);
                        }
                        filesExamined++;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Error: {0}", ex.Message);
                    Environment.Exit(-1);
                    return;
                }


                if (printSummary == true) PrintSummary();
                
            }
            else
            {
                DirectorySearcher ds = new DirectorySearcher(args[0], SearchOption.AllDirectories, null);
                ds.FileFound += ds_FileFound;
                ds.DirectoryFound += ds_DirectoryFound;
                ds.SearchEnded += ds_SearchEnded;
                ds.SearchError += ds_SearchError;
                ds.StartSearch();
            }
        }

        private static void PrintSummary()
        {
            Console.WriteLine("{0}------------------------------------------------", Console.Out.NewLine); // 9,223,372,036,854,775,807
            Console.WriteLine("Files examined         " + String.Format("{0:n0}", filesExamined).PadLeft(25));
            Console.WriteLine("Files skipped          " + String.Format("{0:n0}", filesSkipped).PadLeft(25));
            Console.WriteLine("Files converted        " + String.Format("{0:n0}", filesConverted).PadLeft(25));
            Console.WriteLine("------------------------------------------------");
        }

        static void ds_SearchEnded(object sender, SearchEndedEventArgs e)
        {
            switch (e.Reason)
            {
                case DirectrorySearchEndReason.Cancelled:
                    Console.WriteLine("Operation cancelled");
                    Environment.Exit(0);
                    break;
                case DirectrorySearchEndReason.FatalError:
                    Environment.Exit(-1);
                    break;
                case DirectrorySearchEndReason.Finished:
                    if (printSummary == true) PrintSummary();
                    Environment.Exit(0);
                    break;
            }
        }

        static void ds_DirectoryFound(object sender, DirectoryFoundEventArgs e)
        {
            Console.WriteLine("We have directory {0}", e.Directory.FullName);
        }

        static void ds_FileFound(object sender, FileFoundEventArgs e)
        {
            Console.WriteLine("We have file {0}", e.File.FullName);
            return;
            string file = e.File.FullName;
            if ((searchSystem == false) && (FileUtilities.FileIsInHiddenOrSystemDirectory(file) == true))
            {
                filesSkipped++;
                return;
            }
            if (ImageUtilities.ConvertFileToTIFF(file, convertICOFiles) == true)
            {
                filesConverted++;
                filesExamined++;
                Console.WriteLine("Successfully converted {0}", file);
            }
            filesExamined++;
        }

        static void ds_SearchError(object sender, SearchErrorEventArgs e)
        {
            Console.Error.WriteLine("Error: {0}", e.Error);
        }
    }
}
