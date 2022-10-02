using MQTTnet.Client;
using MQTTnet;
using REWEeBonParserLibrary;
using System;
using System.Runtime.CompilerServices;

namespace REWEeBonParserFileWatcher
{
    internal class REWEeBonParserFileWatcher
    {
        private static String FilePath;
        private static String Prefix;
        private static String Broker; 

        static void Main(string[] args)
        { 
            Console.WriteLine("REWE-eBon-Parser " + typeof(REWEeBonParserFileWatcher).Assembly.GetName().Version);
            Console.WriteLine("(C) Daniel Kirstenpfad, https://www.schrankmonster.de");
            Console.WriteLine("PDFpig PDF Library: https://github.com/UglyToad/PdfPig");
            Console.WriteLine();
            // without parameters show error
            if (args.Length < 1)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: REWEeBonParser.exe <directory to search for PDF eBon files> <mqtt-broker-ip> <mqtt-topic-prefix with / at the end>");
                return;
            }
            try
            {
                FilePath = args[0];
                #region iterate over all already existing files
                List<REWEReceipt> unsortedReceipts = new List<REWEReceipt>();

                foreach (var file in Directory.EnumerateFiles(FilePath, "*.pdf"))
                {
                    Console.Write("Reading file: "+file);
                    try
                    {
                        var receipt = REWEeBonParser.parsePDF(file);

                        unsortedReceipts.Add(receipt);
                        //Console.WriteLine(" - "+receipt.receiptDateTime.ToString());
                    }
                    catch (Exception)
                    {
                        Console.WriteLine(" - Error");
                    }
                }
                #endregion

                List<REWEReceipt> sortedReceipts = unsortedReceipts.OrderBy(o => o.receiptDateTime).ToList();

                if (args.Length == 3)
                {
                    Broker = args[1];
                    Prefix = args[2];

                    // output all receipt information to mqtt - but only if it's a receipt we have not yet seen
                    foreach (REWEReceipt receipt in sortedReceipts)
                    {
                        publishReceiptToMQTT(FilePath, Broker, Prefix, receipt);
                    }

                    #region watch for new files
                    // Create a new FileSystemWatcher and set its properties.
                    FileSystemWatcher watcher = new()
                    {
                        Path = args[0],
                        // Watch both files and subdirectories.
                        IncludeSubdirectories = true,
                        // Watch for all changes specified in the NotifyFilters
                        //enumeration.
                        NotifyFilter = NotifyFilters.LastWrite,
                                       NotifyFilters.FileName,
                                       NotifyFilters.Attributes,

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

                }



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
                publishReceiptToMQTT(FilePath,Broker, Prefix, receipt);

            } catch (Exception ex)
            { 
                Console.WriteLine("Exception: "+ex.Message); 
            }

        }

        private static void publishReceiptToMQTT(String SettingsPath, String MQTTBroker, String ReceiptPrefix, REWEReceipt receipt)
        {
            var mqttFactory = new MqttFactory();

            DateTime LastKnownReceipt = DateTime.MinValue;

            // get from disk if exists...
            if (File.Exists(SettingsPath+ Path.DirectorySeparatorChar+"lastknown.cfg"))
                LastKnownReceipt = DateTime.FromBinary(Convert.ToInt64(File.ReadAllLines(SettingsPath + Path.DirectorySeparatorChar + "lastknown.cfg")[0]));

            if (LastKnownReceipt < receipt.receiptDateTime)
            {
                File.WriteAllText(SettingsPath + Path.DirectorySeparatorChar + "lastknown.cfg", receipt.receiptDateTime.ToBinary().ToString());
                Console.WriteLine("Sending to MQTT: " + receipt.receiptDateTime.ToBinary().ToString());
                using (var mqttClient = mqttFactory.CreateMqttClient())
                {
                    var mqttClientOptions = new MqttClientOptionsBuilder()
                        .WithTcpServer(MQTTBroker)
                        .Build();

                    mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                    while (!mqttClient.IsConnected)
                    {
                        Thread.Sleep(10);
                    }

                    var applicationMessage = new MqttApplicationMessageBuilder()
                                                            .WithTopic(ReceiptPrefix + "receipt/id")
                                                            .WithPayload(receipt.shopId + "-" + receipt.receiptId)
                                                            .Build();
                    mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    applicationMessage = new MqttApplicationMessageBuilder()
                                                .WithTopic(ReceiptPrefix + "payback/pointstotal")
                                                .WithPayload(receipt.PaybackPointsTotal.ToString())
                                                .Build();
                    mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                    applicationMessage = new MqttApplicationMessageBuilder()
                                                .WithTopic(ReceiptPrefix + "payback/earnedwithreceipt")
                                                .WithPayload(receipt.PaybackPointsEarnedWithThisReceipt.ToString())
                                                .Build();
                    mqttClient.PublishAsync(applicationMessage, CancellationToken.None);


                    foreach (var item in receipt.receiptItems)
                    {
                        var itemname = new String(item.name.Where(Char.IsLetterOrDigit).ToArray()).Replace(" ", "_");

                        applicationMessage = new MqttApplicationMessageBuilder()
                                                .WithTopic(ReceiptPrefix + "item/" + itemname + "/amount")
                                                .WithPayload(item.count.ToString().Replace(",", "."))
                                                .Build();
                        mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                        applicationMessage = new MqttApplicationMessageBuilder()
                                                .WithTopic(ReceiptPrefix + "item/" + itemname + "/singleprice")
                                                .WithPayload(item.singlePrice.ToString().Replace(",", "."))
                                                .Build();
                        mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                        applicationMessage = new MqttApplicationMessageBuilder()
                                                .WithTopic(ReceiptPrefix + "item/" + itemname + "/totalprice")
                                                .WithPayload(item.totalPrice.ToString().Replace(",", "."))
                                                .Build();
                        mqttClient.PublishAsync(applicationMessage, CancellationToken.None);

                        if (item.deposit > 0)
                        {
                            applicationMessage = new MqttApplicationMessageBuilder()
                                                    .WithTopic(ReceiptPrefix + "item/" + itemname + "/deposit")
                                                    .WithPayload(item.deposit.ToString().Replace(",", "."))
                                                    .Build();
                            mqttClient.PublishAsync(applicationMessage, CancellationToken.None);
                        }
                    }
                }
            }

        }
    }
}