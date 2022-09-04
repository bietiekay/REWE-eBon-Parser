using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REWEeBonParserLibrary
{
    /// <summary>
    /// This represents an item on the receipt. It's meant to be filled during parsing like this:
    /// Step 1: Set the initial count to at least 1 and the total price.
    /// Step 2: While parsing there will be additional Items and the Count will be upped while the total price should stay the same
    /// </summary>
    public class ReceiptItem
    {
        public String name;
        public Single count;
        public Single singlePrice { get { return (totalPrice+deposit)/count; } }
        public Single totalPrice;
        public Single deposit;
       
        public ReceiptItem(string Name, int Count, float TotalPrice)
        {
            name = Name;

            if (Count == 0)
                throw new ArgumentOutOfRangeException("Count cannot be smaller than 1");

            count = Count;

            totalPrice = TotalPrice;
        }

        public override String ToString()
        {
            return name;
        }
    }
}
