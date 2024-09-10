using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace AzraqSuiting.Models
{
    public static class RawPrinterHelper
    {
        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr pDefault);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, int Level, [In] ref DOC_INFO_1 pDocInfo);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool WritePrinter(IntPtr hPrinter, [In] byte[] pBytes, int dwCount, out int dwWritten);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct DOC_INFO_1
        {
            public string pDocName;
            public string pOutputFile;
            public string pDataType;
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            IntPtr pHandle = IntPtr.Zero;
            DOC_INFO_1 di = new DOC_INFO_1();
            di.pDocName = "MyDocument";
            di.pOutputFile = null;
            di.pDataType = "RAW";

            if (OpenPrinter(printerName, out pHandle, IntPtr.Zero))
            {
                try
                {
                    if (StartDocPrinter(pHandle, 1, ref di))
                    {
                        try
                        {
                            if (StartPagePrinter(pHandle))
                            {
                                try
                                {
                                    int dwWritten;
                                    bool success = WritePrinter(pHandle, bytes, bytes.Length, out dwWritten);
                                    EndPagePrinter(pHandle);
                                    EndDocPrinter(pHandle);
                                    return success;
                                }
                                finally
                                {
                                    EndPagePrinter(pHandle);
                                }
                            }
                        }
                        finally
                        {
                            EndDocPrinter(pHandle);
                        }
                    }
                }
                finally
                {
                    ClosePrinter(pHandle);
                }
            }
            else
            {
                // Log or handle error if OpenPrinter fails
                int errorCode = Marshal.GetLastWin32Error();
                Console.WriteLine($"OpenPrinter failed with error code {errorCode}");
            }
            return false;
        }
    }
}