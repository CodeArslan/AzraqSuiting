using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using ZXing.Common;
using ZXing;

namespace AzraqSuiting.HelperMethods
{
    public class BarcodeHelper
    {
        public static Bitmap GenerateBarcode(string content)
        {
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 70,
                    Width = 300
                }
            };

            return barcodeWriter.Write(content);
        }

        public static byte[] ConvertToByteArray(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}