using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;

namespace CherukarasThejas.Areas.BillSoftware.Data
{
    public static class BarcodeHelper
    {
        public static Bitmap GenerateBarcode(string text)
        {
            var barcodeWriter = new ZXing.BarcodeWriter
            {
                Format = ZXing.BarcodeFormat.CODE_128,
                Options = new ZXing.Common.EncodingOptions
                {
                    Width = 300,
                    Height = 100,
                    Margin = 2
                }
            };

            return barcodeWriter.Write(text);
        }
    }
}