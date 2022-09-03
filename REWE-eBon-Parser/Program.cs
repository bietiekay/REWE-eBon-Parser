using REWEeBonParserLibrary;
using System;
using System.Runtime.CompilerServices;

namespace REWEeBonParserFileWatcher
{
    internal class REWEeBonParserFileWatcher
    {        
        static void Main(string[] args)
        {
            Console.WriteLine("REWE-eBon-Parser " + typeof(REWEeBonParserFileWatcher).Assembly.GetName().Version);
            Console.WriteLine("(C) Daniel Kirstenpfad, https://www.schrankmonster.de");
            Console.WriteLine("PDFpig PDF Library: https://github.com/UglyToad/PdfPig");
            Console.WriteLine();
            // without parameters show error
            if (args.Length != 1)
            {
                // Display the proper way to call the program.

                Console.WriteLine("Usage: REWEeBonParser.exe <directory to watch for new PDF eBon files>");
                return;
            }
            try
            {
                // Create a new FileSystemWatcher and set its properties.
                FileSystemWatcher watcher = new()
                {
                    Path = args[0],
                    // Watch both files and subdirectories.
                    IncludeSubdirectories = true,
                    // Watch for all changes specified in the NotifyFilters
                    //enumeration.
                    NotifyFilter = NotifyFilters.Attributes |
                NotifyFilters.CreationTime |
                NotifyFilters.DirectoryName |
                NotifyFilters.FileName |
                NotifyFilters.LastAccess |
                NotifyFilters.LastWrite |
                NotifyFilters.Security |
                NotifyFilters.Size,
                    // Watch all files.
                    Filter = "*.pdf"
                };
                // Add event handlers.
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                //Start monitoring.
                watcher.EnableRaisingEvents = true;
               
                // Wait for user to quit program.
                Console.WriteLine("Watching the directory - \'q + enter\' or ctrl-c to quit.");
                Console.WriteLine();
                //Make an infinite loop till 'q' is pressed.
                while (Console.Read() != 'q') ;
            }
            catch (IOException e)
            {
                Console.WriteLine("A IO-Exception Occurred :" + e);
            }
            catch (Exception oe)
            {
                Console.WriteLine("An Exception Occurred :" + oe);
            }
        }
        // Define the event handlers.
        public static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed.
            Console.WriteLine("Found file: {0}", e.Name);
            try
            {
                var REWEParser = new REWEeBonParser(e.FullPath);
            } catch (Exception ex)
            { 
                Console.WriteLine("Exception: "+ex.Message); 
            }

        }
    }
}