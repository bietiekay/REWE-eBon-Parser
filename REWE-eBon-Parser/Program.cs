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
                Console.WriteLine("Usage: REWEeBonParser.exe <directory to search for PDF eBon files> <mqtt-broker-ip>");
                return;
            }
            try
            {
                #region iterate over all already existing files
                List<REWEReceipt> unsortedReceipts = new List<REWEReceipt>();

                foreach (var file in Directory.EnumerateFiles(args[0], "*.pdf"))
                {
                    Console.Write("Reading file: "+file);
                    try
                    {
                        var receipt = REWEeBonParser.parsePDF(file);

                        unsortedReceipts.Add(receipt);
                        Console.WriteLine(" - "+receipt.receiptDateTime.ToString());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(" - Error");
                    }
                }
                #endregion

                List<REWEReceipt> sortedReceipts = unsortedReceipts.OrderBy(o => o.receiptDateTime).ToList();

                // output all receipt information
                var ReceiptPrefix = "external/receipts/rewe/";
                foreach (REWEReceipt receipt in sortedReceipts)
                {
                    Console.WriteLine(ReceiptPrefix + "receipt/id" + " -> " + receipt.shopId+"-"+receipt.receiptId);
                    //Console.WriteLine(ReceiptPrefix + "receipt/id/"+receipt.shopId+"-"+receipt.receiptId+"/datetime" + " -> " + receipt.receiptDateTime.ToFileTimeUtc());
                    Console.WriteLine(ReceiptPrefix + "payback/pointstotal"+ " -> " + receipt.PaybackPointsTotal);
                    Console.WriteLine(ReceiptPrefix + "payback/earnedwithreceipt" + " -> " + receipt.PaybackPointsEarnedWithThisReceipt);

                    foreach(var item in receipt.receiptItems)
                    {
                        var itemname = new String(item.name.Where(Char.IsLetterOrDigit).ToArray()).Replace(" ","_");

                        Console.WriteLine(ReceiptPrefix + "item/" + itemname+ "/amount"+ " --> "+item.count);
                        Console.WriteLine(ReceiptPrefix + "item/" + itemname + "/singleprice" + " --> " + item.singlePrice);
                        Console.WriteLine(ReceiptPrefix + "item/" + itemname + "/totalprice" + " --> " + item.totalPrice);
                        if (item.deposit > 0)
                            Console.WriteLine(ReceiptPrefix + "item/" + itemname + "/deposit" + " --> " + item.deposit);

                    }
                }

                /*
                #region watch for new files
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
                #endregion
                */

                Console.WriteLine("done");
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
                var receipt = REWEeBonParser.parsePDF(e.FullPath);
            } catch (Exception ex)
            { 
                Console.WriteLine("Exception: "+ex.Message); 
            }

        }
    }
}