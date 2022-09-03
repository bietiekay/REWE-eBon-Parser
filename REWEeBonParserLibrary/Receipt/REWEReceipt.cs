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
    
    }
}
