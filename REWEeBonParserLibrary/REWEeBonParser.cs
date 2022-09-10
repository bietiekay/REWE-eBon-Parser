using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using UglyToad.PdfPig;
using System.Text.RegularExpressions;

namespace REWEeBonParserLibrary
{
    public static class REWEeBonParser
    {
        public static REWEReceipt parsePDF(String FullPathPDFFile)
        {
            String? Fulltext = ParsePDF(FullPathPDFFile); // parse the PDF file, assuming it is one

            if (Fulltext != null) // if something got parsed successfully
            {
                // first ensure that we got something from REWE here
                if (Fulltext.Contains("REWE", StringComparison.OrdinalIgnoreCase))
                {
                    // prepare for special case of single-line-multi-item receipt items
                    Fulltext = Fulltext.Replace(" B      ", " B\n");
                    // split up the Fulltext by lines
                    String[] FullTextByLines_run1 = Fulltext.Replace("\r", "").Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    REWEReceipt newReceipt = new REWEReceipt();

                    // parse the marketinformation
                    newReceipt.supermarketHeader = GetMarketInformation(FullTextByLines_run1);
                    newReceipt.cashRegisterNo = GetCashRegisterNo(Fulltext);
                    newReceipt.employeeID = GetEmployeeNo(Fulltext);
                    newReceipt.shopId = GetShopId(Fulltext);
                    newReceipt.receiptDateTime = GetReceiptDateTime(Fulltext);
                    newReceipt.PaybackPointsEarnedWithThisReceipt = GetPaybackPointsEarned(Fulltext);
                    newReceipt.PaybackPointsTotal = GetPaybackPointsTotal(Fulltext) + newReceipt.PaybackPointsEarnedWithThisReceipt;
                    newReceipt.receiptId = GetReceiptId(Fulltext);

                    // now for the actual receipt items
                    newReceipt.ParseItemsFromText(FullTextByLines_run1);

                    return newReceipt;
                }
                else
                    throw new InvalidDataException("This seems not a REWE receipt.");
            }
            else
            {
                throw new InvalidDataException("Could not parse " + FullPathPDFFile);
            }
        }

        #region Parse Helpers
        #region ReceiptDateTime
        private static DateTime GetReceiptDateTime(String Fulltext)
        {
            string datepattern = @"Datum:[^.]*(\d\d).(\d\d).(\d\d\d\d)";
            string timepattern = @"Uhrzeit: [^.]*(..)[^.](..)[^\d](..).Uhr";
            Int32 Hour, Minute, Second, Day, Month, Year;

            RegexOptions options = RegexOptions.Multiline;
            try
            {
                foreach (Match m in Regex.Matches(Fulltext, datepattern, options))
                {
                    if (m.Success)
                    {
                        if (m.Groups.Count == 4)
                        {
                            Day = Convert.ToInt32(m.Groups[1].Value);
                            Month = Convert.ToInt32(m.Groups[2].Value);
                            Year = Convert.ToInt32(m.Groups[3].Value);

                            // since we found the Date lets go for the time now
                            foreach (Match m2 in Regex.Matches(Fulltext, timepattern, options))
                            {
                                if (m2.Success)
                                {
                                    if (m2.Groups.Count == 4)
                                    {
                                        Hour = Convert.ToInt32(m2.Groups[1].Value);
                                        Minute = Convert.ToInt32(m2.Groups[2].Value);
                                        Second = Convert.ToInt32(m2.Groups[3].Value);

                                        return new DateTime(Year,Month,Day,Hour,Minute,Second);
                                    }                                        
                                }
                            }

                        }
                    }
                }
                return DateTime.MinValue;
            } catch { return DateTime.MinValue; }
        }
        #endregion
        #region PaybackPointsTotal
        private static Int32 GetPaybackPointsTotal(String Fulltext)
        {
            string pattern = @"Punktestand.vor.Einkauf:.(\d*)";

            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(Fulltext.Replace(".",""), pattern, options))
            {
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        return Convert.ToInt32(m.Groups[1].Value);
                }
            }
            return 0;
        }
        #endregion
        #region PaybackPointsEarned
        private static Int32 GetPaybackPointsEarned(String Fulltext)
        {
            string pattern = @"Sie.erhalten.(\d*).PAYBACK";

            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(Fulltext.Replace(".", ""), pattern, options))
            {
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        return Convert.ToInt32(m.Groups[1].Value);
                }
            }
            return 0;
        }
        #endregion
        #region ReceiptId
        private static String GetReceiptId(String Fulltext)
        {
            string pattern1 = @"Beleg-Nr.\D*(\d*)";
            string pattern2 = @"Trace-Nr.\D*(\d*)";

            String result1 = "";
            String result2 = "";

            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(Fulltext, pattern1, options))
            {
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        result1 = m.Groups[1].Value;
                }
            }
            foreach (Match m in Regex.Matches(Fulltext, pattern2, options))
            {
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        result2 = m.Groups[1].Value;
                }
            }

            return result1 + result2;
        }
        #endregion
        #region ShopId
        private static Int32 GetShopId(String Fulltext)
        {
            string pattern = @"Markt:([^\s]+)";

            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(Fulltext, pattern, options))
            {
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        return Convert.ToInt32(m.Groups[1].Value);
                }
            }
            return 0;
        }
        #endregion
        #region EmployeeNumber
        private static Int32 GetEmployeeNo(String Fulltext)
        {
            string pattern = @"Bed.:([^\s]+)";

            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(Fulltext, pattern, options))
            {
                if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        return Convert.ToInt32(m.Groups[1].Value);
                }
            }
            return 0;
        }
        #endregion
        #region CashRegisterNo
        private static Int32 GetCashRegisterNo(String Fulltext)
        {
            string pattern = @"Kasse:([^\s]+)";
            
            RegexOptions options = RegexOptions.Multiline;

            foreach (Match m in Regex.Matches(Fulltext, pattern, options))
            {
             if (m.Success)
                {
                    if (m.Groups.Count > 1)
                        return Convert.ToInt32(m.Groups[1].Value);
                }
            }
            return 0;
        }
        #endregion
        #region Market Information
        private static String GetMarketInformation(String[] Fulltext)
        {
            String MarketInformation = "";

            foreach(String FulltextLine in Fulltext)
            {
                if (FulltextLine.StartsWith("EUR"))
                    break;

                if (MarketInformation.Length > 0) MarketInformation += Environment.NewLine;
                MarketInformation += FulltextLine;
            }

            return MarketInformation;
        }
        #endregion
        #endregion

        #region PDF
        private static String? ParsePDF(String FullPathPDFFile)
        {
            try
            {
                using var pdf = PdfDocument.Open(FullPathPDFFile);
                String eReceiptRawText = "";

                foreach (var page in pdf.GetPages())
                {
                    // Either extract based on order in the underlying document with newlines and spaces.
                    eReceiptRawText += "\n"+ContentOrderTextExtractor.GetText(page);
                }
                return eReceiptRawText;

            } catch { return null; }
        }
        #endregion
    }
}
