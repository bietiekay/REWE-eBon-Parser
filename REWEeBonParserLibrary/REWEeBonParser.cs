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
    public class REWEeBonParser
    {
        public REWEeBonParser(String FullPathPDFFile)
        {
            String? Fulltext = ParsePDF(FullPathPDFFile); // parse the PDF file, assuming it is one
            
            if (Fulltext != null) // if something got parsed successfully
            {
                // first ensure that we got something from REWE here
                if (Fulltext.Contains("REWE", StringComparison.OrdinalIgnoreCase))
                {
                    // split up the Fulltext by lines
                    String[] FullTextByLines = Fulltext.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    REWEReceipt newReceipt = new REWEReceipt();
                    
                    // parse the marketinformation
                    newReceipt.supermarketHeader = GetMarketInformation(FullTextByLines);
                    newReceipt.cashRegisterNo = GetCashRegisterNo(Fulltext);
                    newReceipt.employeeID = GetEmployeeNo(Fulltext);
                    newReceipt.shopId = GetShopId(Fulltext);


                    Console.WriteLine(newReceipt.shopId);
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

        #region ShopId
        private Int32 GetShopId(String Fulltext)
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
        private Int32 GetEmployeeNo(String Fulltext)
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
        private Int32 GetCashRegisterNo(String Fulltext)
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
        private String GetMarketInformation(String[] Fulltext)
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
                    eReceiptRawText += ContentOrderTextExtractor.GetText(page);
                }
                return eReceiptRawText;

            } catch { return null; }
        }
        #endregion
    }
}
