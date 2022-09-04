using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REWEeBonParserLibrary
{
    public class REWEReceipt
    {
        public DateTime receiptDateTime;
        public Int32 cashRegisterNo;
        public Int32 shopId;
        public Int32 employeeID;
        public Int32 receiptId;
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
                return receiptItems.Sum(ReceiptItem => ReceiptItem.totalPrice); 
            } 
        }

        /// <summary>
        /// parses from a fulltext line-by-line array representation all the receipt items and prices
        /// </summary>
        /// <param name="FulltextLines"></param>
        public void ParseItemsFromText(String[] FulltextLines)
        {
            ReceiptItem previousReceiptItem;
            bool startFound = false;

            // go line by line
            foreach(String line in FulltextLines)
            {
                // we have already found the start of the receipt item block
                if (startFound)
                {
                    ^^

                }

                // is this the beginning of the receipt item block?
                if (line == "EUR\n")
                    startFound = true;
            }
        }
    
    }
}
