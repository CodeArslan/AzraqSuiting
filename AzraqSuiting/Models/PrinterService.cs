using AzraqSuiting.ViewModels;
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using ESCPOS_NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;
using Color = System.Drawing.Color;
using System.Threading;

namespace AzraqSuiting.Models
{
    public class PrinterService
    {
        public void PrintInvoice(SalesViewModel model)
        {
            string GetProductNameById(int productId)
            {
                using (var dbContext = new ApplicationDbContext())
                {
                    var product = dbContext.Product.FirstOrDefault(p => p.Id == productId);
                    return product?.Name ?? "Unknown Product";
                }
            }

            string logoImagePath = "dist-assets\\images\\azraq-logo.jpg";  
            string absoluteLogoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logoImagePath);

            byte[] logoBytes = ConvertImageToEscPosFormat(absoluteLogoPath);

           
            var width = 48; 
            var lineWidth = 48; 
            var invoiceNumberWidth = 24; 

            var invoiceText =  $"{CenterText("Chandni Chowk, Lalamusa", width)}\n" +
                               $"{CenterText("Phone: 0300 6200774", width)}\n\n" +
                               $"{LeftAlign("Invoice No: " + model.orderNum, invoiceNumberWidth)}" +
                               $"{RightAlign("Date: " + model.saleDate.ToString("MM/dd/yyyy"), lineWidth - invoiceNumberWidth)}\n" +
                               "------------------------------------------------\n"+
                               "Description         U.P    Qty    Disc.    Total\n" +
                               "------------------------------------------------\n";

            decimal subtotal = 0;

            foreach (var detail in model.InvoiceDetails)
            {
                var productName = string.IsNullOrEmpty(detail.ProductName) && detail.productId.HasValue
                    ? GetProductNameById(detail.productId.Value)
                    : detail.ProductName;

                var itemTotal = (detail.Quantity * detail.unitPrice) - detail.discount;
                subtotal += itemTotal;
                var fixedDescription = productName.Length > 20
                    ? productName.Substring(0, 15)  
                    : productName.PadRight(17);

                invoiceText += $"{fixedDescription,-13} {detail.unitPrice,4:0} {detail.Quantity,7} {detail.discount,7:0} {itemTotal,9:0}\n";
            }

            var totalDiscount = model.InvoiceDetails.Sum(d => d.discount);
            var totalAmount = subtotal - model.discount;
            var totalWidth = 48; 
            var amountWidth = 14; 

            var spacingAdjustmentSubtotal = 20; 
            var spacingAdjustmentDiscount = 25; 
            var spacingAdjustmentTotal = 20; 

            var labelSubtotal = "Subtotal";
            var labelDiscount = "Total Discount";
            var labelTotal = "Total";

            var labelWidthSubtotal = totalWidth - amountWidth - spacingAdjustmentSubtotal;
            var labelWidthDiscount = totalWidth - amountWidth - spacingAdjustmentDiscount;
            var labelWidthTotal = totalWidth - amountWidth - spacingAdjustmentTotal;

            invoiceText += "                    ----------------------------\n" +
                           $"                    {labelSubtotal.PadRight(labelWidthSubtotal)}{subtotal.ToString("0").PadLeft(amountWidth)}\n" +
                           $"                    {labelDiscount.PadRight(labelWidthDiscount)}{model.discount.ToString("0").PadLeft(amountWidth)}\n" +
                           $"                    {labelTotal.PadRight(labelWidthTotal)}{totalAmount.ToString("0").PadLeft(amountWidth)}\n" +
                           "                    ----------------------------\n\n" +
                           "No Return!\n" +
                           "No Refund!\n" +
                           "Thankyou for Shopping\n" +
                          "------------------------------------------------\n\n" +
                           "Software By\n" +
                           "Arslan Tariq\n" +
                           "Contact: 0309 5123745\n\n\n\n\n\n\n\n";




            var invoiceBytes = System.Text.Encoding.ASCII.GetBytes(invoiceText);

            RawPrinterHelper.SendBytesToPrinter("XP-80C", logoBytes);
            RawPrinterHelper.SendBytesToPrinter("XP-80C", invoiceBytes);
            byte[] cutPaperCommand = new byte[] { 0x1D, 0x56, 0x00 };
            RawPrinterHelper.SendBytesToPrinter("XP-80C", cutPaperCommand);
        }

        private string CenterText(string text, int width)
        {
            if (text.Length >= width) return text; 
            var spaces = width - text.Length;
            var padLeft = spaces / 2;
            var padRight = spaces - padLeft;
            return new string(' ', padLeft) + text + new string(' ', padRight);
        }

        private string LeftAlign(string text, int width)
        {
            return text.PadRight(width);
        }

        private string RightAlign(string text, int width)
        {
            return text.PadLeft(width);
        }




        private byte[] ConvertImageToEscPosFormat(string imagePath, int maxWidth = 300, int maxHeight = 300)
        {
            using (var originalImage = new Bitmap(imagePath))
            {
                var resizedImage = ResizeImage(originalImage, maxWidth, maxHeight);

                var blackAndWhiteImage = ConvertToBlackAndWhite(resizedImage);

                List<byte> imageBytes = new List<byte>();

                imageBytes.Add(0x1B);  
                imageBytes.Add(0x61);  
                imageBytes.Add(0x01);  
                int imageWidth = blackAndWhiteImage.Width;
                int imageHeight = blackAndWhiteImage.Height;
                for (int y = 0; y < imageHeight; y += 24)
                {
                    imageBytes.Add(0x1B); // ESC
                    imageBytes.Add(0x2A); // *
                    imageBytes.Add(0x21); // 24-dot double density
                    imageBytes.Add((byte)(imageWidth % 256)); // nL
                    imageBytes.Add((byte)(imageWidth / 256)); // nH

                    for (int x = 0; x < imageWidth; x++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            byte slice = 0;

                            for (int b = 0; b < 8; b++)
                            {
                                int pixelY = y + k * 8 + b;
                                if (pixelY >= imageHeight)
                                    continue;

                                Color pixelColor = blackAndWhiteImage.GetPixel(x, pixelY);
                                if (pixelColor.R == 0)  // Black pixel
                                {
                                    slice |= (byte)(1 << (7 - b));
                                }
                            }
                            imageBytes.Add(slice);
                        }
                    }

                    imageBytes.Add(0x0A);  // Line feed
                }
                imageBytes.Add(0x1B);  // ESC
                imageBytes.Add(0x61);  // 'a'
                imageBytes.Add(0x00);  // 0 for left alignment

                return imageBytes.ToArray();
            }
        }

        // Resize the image
        private Bitmap ResizeImage(Bitmap original, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / original.Width;
            var ratioY = (double)maxHeight / original.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(original.Width * ratio);
            var newHeight = (int)(original.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(newImage))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.DrawImage(original, 0, 0, newWidth, newHeight);
            }

            return newImage;
        }

        // Convert image to black-and-white
        private Bitmap ConvertToBlackAndWhite(Bitmap original)
        {
            Bitmap newImage = new Bitmap(original.Width, original.Height);
            for (int y = 0; y < original.Height; y++)
            {
                for (int x = 0; x < original.Width; x++)
                {
                    System.Drawing.Color originalColor = original.GetPixel(x, y);
                    int grayscale = (int)((originalColor.R * 0.3) + (originalColor.G * 0.59) + (originalColor.B * 0.11));
                    System.Drawing.Color newColor = grayscale < 128 ? System.Drawing.Color.Black : System.Drawing.Color.White;
                    newImage.SetPixel(x, y, newColor);
                }
            }
            return newImage;
        }

    }
}