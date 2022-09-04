using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace REWEeBonParserLibrary
{
    public class REWEReceipt
    {
        public DateTime receiptDateTime;
        public Int32 cashRegisterNo;
        public Int32 shopId;
        public Int32 employeeID;
        public String receiptId;
        public String supermarketHeader;
        public Int32 PaybackPointsTotal;
        public Int32 PaybackPointsEarnedWithThisReceipt;

        public List<ReceiptItem> receiptItems;

        public REWEReceipt()
        {
            supermarketHeader = "";
            receiptItems = new List<ReceiptItem>();
        }

        public float TotalAmount { 
            get { 
                return receiptItems.Sum(ReceiptItem => ReceiptItem.totalPrice+ReceiptItem.deposit);
            } 
        }

        /// <summary>
        /// parses from a fulltext line-by-line array representation all the receipt items and prices
        /// </summary>
        /// <param name="FulltextLines"></param>
        public void ParseItemsFromText(String[] FulltextLines)
        {
            ReceiptItem previousReceiptItem = null;
            bool startFound = false;
            // go line by line
            foreach(String line in FulltextLines)
            {
                // we have already found the start of the receipt item block
                if (startFound)
                {
                    Console.WriteLine(line);

                    // a line item if it ends with " B\n"
                    if (line.EndsWith(" B") || line.EndsWith(" A"))
                    {
                        // Step 1 - remove " B"
                        String workItem = line.Remove(line.Length - 2, 2);
                        // Step 2 - split at first instance of " " from the right side
                        String price = workItem.Remove(0, workItem.LastIndexOf(" ")+1).Replace(".", "").Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                        String name = workItem.Remove(workItem.LastIndexOf(" "), workItem.Length-workItem.LastIndexOf(" ")).Trim(' ');

                        // handle "nachtr. Preiskorrekt"
                        if (name == "Nachtr. Preiskorrekt")
                        {
                            if (previousReceiptItem != null)
                            {
                                // correct the price
                                previousReceiptItem.totalPrice = previousReceiptItem.totalPrice + Convert.ToSingle(price);

                                continue;
                            }
                        }

                        previousReceiptItem = new ReceiptItem(name, 1, Convert.ToSingle(price));
                        receiptItems.Add(previousReceiptItem);
                        // next one!
                        continue;
                    }

                    // a deposit line
                    if (line.EndsWith(" A *"))
                    {
                        // Step 1 - remove " A *"
                        String workItem = line.Remove(line.Length - 4, 4);
                        // Step 2 - split at first instance of " " from the right side
                        String price = workItem.Remove(0, workItem.LastIndexOf(" ") + 1).Replace(".", "").Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                        
                        // add the deposit up the total price
                        if (previousReceiptItem != null)
                        {
                            previousReceiptItem.deposit += Convert.ToSingle(price);
                        }
                    }

                    // a credit line
                    if (line.EndsWith(" B *"))
                    {
                        // Step 1 - remove " B *"
                        String workItem = line.Remove(line.Length - 4, 4);
                        // Step 2 - split at first instance of " " from the right side
                        String price = workItem.Remove(0, workItem.LastIndexOf(" ") + 1).Replace(".", "").Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);
                        String name = workItem.Remove(workItem.LastIndexOf(" "), workItem.Length - workItem.LastIndexOf(" ")).Trim(' ');

                        previousReceiptItem = new ReceiptItem(name, 1, 0.0f);
                        previousReceiptItem.deposit = Convert.ToSingle(price);
                        receiptItems.Add(previousReceiptItem);
                    }

                    // a simple multi-item line if it contains " Stk x "
                    if (line.Contains(" Stk x "))
                    {
                        String[] splitted = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                        if (splitted.Length > 1)
                        {
                            if (previousReceiptItem != null)
                            {
                                // set the last receipt item count
                                previousReceiptItem.count = Convert.ToSingle(splitted[0]);
                            }
                        }

                        continue;
                    }
                    // a weight line if it contains " kg x "
                    if (line.Contains(" kg x "))
                    {
                        String[] splitted = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);

                        if (splitted.Length > 1)
                        {
                            if (previousReceiptItem != null)
                            {
                                // set the last receipt item count
                                previousReceiptItem.count = Convert.ToSingle(splitted[0].Replace(".", "").Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator));
                            }
                        }

                        continue;
                    }

                    // if the receipt items are finished
                    if (line.StartsWith("-----"))
                    {
                        break;
                    }
                }

                // is this the beginning of the receipt item block?
                if (line.StartsWith("EUR"))
                    startFound = true;
            }
        }
    
    }
}
