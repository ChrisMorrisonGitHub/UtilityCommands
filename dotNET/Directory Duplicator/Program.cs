using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Directory_Duplicator
{
    class Program
    {
        static void Main(string[] args)
        {
            if ((args.Length == 1) && ((args[0] == "/?") || (args[0] == "/h") || (args[0] == "/H")))
            {
                Console.WriteLine("Duplicates a folder as the given path.\n");
                Console.WriteLine("{0} <search folder> <destination folder> [/V] [/H] [/R] [/S]\n", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Console.WriteLine("  <source folder>");
                Console.WriteLine("\tSpecifies the folder to duplicate.\n");
                Console.WriteLine("  <destination folder>");
                Console.WriteLine("\tSpecifies the name and location of duplicate.\n");
                Console.WriteLine("  /V\tPrint additional information.\n");
                Console.WriteLine("  /H\tInclude system and hidden files and folders.\n");
                Console.WriteLine("  /R\tRecurse subdirectores.\n");
                Console.WriteLine("  /S\tPrint a summary when finished.\n");
                Environment.Exit(0);
            }
            if (args.Length < 2)
            {
                Console.Error.WriteLine("Usage: {0} <source folder> <destination folder> [/V] [/H] [/R] [/S]", Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName));
                Environment.Exit(-1);
                return;
            }
            string sourcePath = args[0];
            if (Directory.Exists(sourcePath) == false)
            {
                Console.Error.WriteLine("Error: {0} is not a valid search location.", sourcePath);
                Environment.Exit(-1);
                return;
            }
        }
    }
}
