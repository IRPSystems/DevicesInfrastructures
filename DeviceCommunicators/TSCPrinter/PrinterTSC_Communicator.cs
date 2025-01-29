using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static DeviceCommunicators.TSCPrinter.PrinterTSC_Communicator;

namespace DeviceCommunicators.TSCPrinter
{
    public class PrinterTSC_Communicator : DeviceCommunicator
    {
        #region DLL Import
        // Structure and API declarions:
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)]
            public string pDataType;
        }
        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        [DllImport("TSCLIB.dll", EntryPoint = "about")]
        public static extern int about();

        [DllImport("TSCLIB.dll", EntryPoint = "openport")]
        public static extern int openport(string printername);

        [DllImport("TSCLIB.dll", EntryPoint = "barcode")]
        public static extern int barcode(string x, string y, string type,
                    string height, string readable, string rotation,
                    string narrow, string wide, string code);

        [DllImport("TSCLIB.dll", EntryPoint = "clearbuffer")]
        public static extern int clearbuffer();

        [DllImport("TSCLIB.dll", EntryPoint = "closeport")]
        public static extern int closeport();

        [DllImport("TSCLIB.dll", EntryPoint = "downloadpcx")]
        public static extern int downloadpcx(string filename, string image_name);

        [DllImport("TSCLIB.dll", EntryPoint = "formfeed")]
        public static extern int formfeed();

        [DllImport("TSCLIB.dll", EntryPoint = "nobackfeed")]
        public static extern int nobackfeed();

        [DllImport("TSCLIB.dll", EntryPoint = "printerfont")]
        public static extern int printerfont(string x, string y, string fonttype,
                        string rotation, string xmul, string ymul,
                        string text);

        [DllImport("TSCLIB.dll", EntryPoint = "printlabel")]
        public static extern int printlabel(string set, string copy);

        [DllImport("TSCLIB.dll", EntryPoint = "sendcommand")]
        public static extern int sendcommand(string printercommand);

        [DllImport("TSCLIB.dll", EntryPoint = "setup")]
        public static extern int setup(string width, string height,
                  string speed, string density,
                  string sensor, string vertical,
                  string offset);

        [DllImport("TSCLIB.dll", EntryPoint = "windowsfont")]
        public static extern int windowsfont(int x, int y, int fontheight,
                        int rotation, int fontstyle, int fontunderline,
                        string szFaceName, string content);
        [DllImport("TSCLIB.dll", EntryPoint = "windowsfontUnicode")]
        public static extern int windowsfontUnicode(int x, int y, int fontheight,
                         int rotation, int fontstyle, int fontunderline,
                         string szFaceName, byte[] content);

        [DllImport("TSCLIB.dll", EntryPoint = "sendBinaryData")]
        public static extern int sendBinaryData(byte[] content, int length);

        [DllImport("TSCLIB.dll", EntryPoint = "usbportqueryprinter")]
        public static extern byte usbportqueryprinter();

        #endregion

        #region Fields

        private bool _isInitialized;
        public override bool IsInitialized => _isInitialized;


       // private bool isTestLabelPrinted = false;

        //private string preBuildPrintCmd = null;

        //private string commonSnVarIndicator = "{SN}";

        private string checkStatusString = "<ESC>!?";

        private string encodingName = String.Empty;

        private string _comName = String.Empty;

        //This command forces / restarts the printer after a fault (Triggering the button)
        //private string feedButtonCmd = "\u001b!F";

        IntPtr hPrinter;

        //string WT1 = "TSC Printers";
        //string B1 = "20080101";
        byte[] result_unicode = System.Text.Encoding.GetEncoding("utf-16").GetBytes("unicode test");
        byte[] result_utf8 = System.Text.Encoding.UTF8.GetBytes("TEXT 40,620,\"ARIAL.TTF\",0,12,12,\"utf8 test Wörter auf Deutsch\"");

        #endregion

        #region Constructor

        public PrinterTSC_Communicator(LogLineListService logLineList) :
			base(logLineList)
		{
            hPrinter = new IntPtr(0);
            _isInitialized = false;
        }

        #endregion Constructor

        #region Methods

        #region Enums
        public enum PrinterStatus
        {
            OK = 0x00,
            HeadOpen = 0x01,
            PaperJam = 0x02,
            PaperJamHeadOpened = 0x03,
            OutOfPaper = 0x04,
            OutOfPaperHeadOpened = 0x05,
            OutOfRibbon = 0x08,
            OutOfRibbonHeadOpened = 0x09,
            OutOfRibbonPaperJam = 0x0A,
            OutOfRibbonPaperJamHeadOpened = 0x0B,
            OutOfRibbonOutOfPaper = 0x0C,
            OutOfRibbonOutOfPaperHeadOpened = 0x0D,
            Pause = 0x10,
            Printing = 0x20,
            OtherError = 0x80,
            NoComm = 0xFF
        }

        #endregion Enums

        public void Init(
			bool isUdpSimulation,
			string comName)
        {

            if (comName == null)
            {
                return;
            }
            _comName = comName;
            hPrinter = new IntPtr(0);
            var printer = OpenPrinter(_comName, out hPrinter, IntPtr.Zero);
            if(printer)
            {
                _isInitialized = true;
                //PrintTest();
            }
            InitBase();

		}

        public override void Dispose()
		{
			base.Dispose();
            _isInitialized = false;
		}

		protected override CommunicatorResultEnum HandleRequests(CommunicatorIOData data)
		{

			if (data.IsSet)
			{
				SetParamValue_Do(
					data.Parameter,
					data.Value,
					data.Callback);
			}
			else
			{
				GetParamValue_Do(
					data.Parameter,
					data.Callback);
			}

			return CommunicatorResultEnum.OK;

		}


        private void SetParamValue_Do(
			DeviceParameterData param,
			double value,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
            try
            {
                if (!(param is PrinterTSC_ParamData tscPrinter_Param))
                    return;

                SendStringToPrinter(tscPrinter_Param.DataContent);
                tscPrinter_Param.UpdateSendResLog(tscPrinter_Param.DataContent, DeviceParameterData.SendOrRecieve.Send);
                PrinterStatus printerStatus;
                if (CheckPrinterStatusPostCommand(out printerStatus))
                {
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                    return;
                }
                else
                {
                    callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                    tscPrinter_Param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, printerStatus.ToString());
                    return;
                }

            }
            catch (Exception ex)
            {
                param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Send, "Failed to set value for parameter: " + ex);
                LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
            }
        }

        private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is PrinterTSC_ParamData tscPrinter_Param))
                    return;

                //Connection Logic

                if (tscPrinter_Param.DataContent != checkStatusString)
                {
                    SendStringToPrinter(tscPrinter_Param.DataContent);
                    tscPrinter_Param.UpdateSendResLog(tscPrinter_Param.DataContent, DeviceParameterData.SendOrRecieve.Send);
                }
                PrinterStatus status = PrinterStatus.NoComm;
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(2)))
                {
                    try
                    {
                        Task.Run(() =>
                        {
                            Thread.Sleep(300);
                            status = (PrinterStatus)usbportqueryprinter();
                        }, cts.Token).Wait(cts.Token); // Wait for the task to complete or be canceled
                    }
                    catch (Exception e)
                    {
                        tscPrinter_Param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, "Printer operation timed out and was canceled:" + e.Message);
                        MessageBox.Show("Printer operation timed out and was canceled:" + e.Message);
                    }
                }
                
                
                if (status != PrinterStatus.NoComm)
                {
                    callback?.Invoke(param, CommunicatorResultEnum.OK, null);
                    return;
                }
                else
                {
                    callback?.Invoke(param, CommunicatorResultEnum.Error, null);
                    tscPrinter_Param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, status.ToString());
                    return;
                }
            }
            catch (Exception ex)
            {
                param.UpdateSendResLog("", DeviceParameterData.SendOrRecieve.Recieve, "Failed to receive value for parameter: " + ex);
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }

        #region Printer Functions

        public bool SendStringToPrinter(string printerCmd)
        {
            bool success;
            IntPtr pBytes;
            Int32 dwCount;

            // How many characters are in the string?
            dwCount = printerCmd.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(printerCmd);
            // Send the converted ANSI string to the printer.
            success = SendBytesToPrinter(_comName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);

            return success;
        }

        // SendBytesToPrinter()
        // When the function is given a printer name and an unmanaged array
        // of bytes, the function sends those bytes to the print queue.
        // Returns true on success, false on failure.
        public bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
        {
            Int32 dwError = 0, dwWritten = 0;
            DOCINFOA di = new DOCINFOA();
            bool bSuccess = false; // Assume failure unless you specifically succeed.

            di.pDocName = "VCODE RAW SENDER";
            di.pDataType = "RAW";

            // Open the printer.
            try
            {
                if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    // Start a document.
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        // Start a page.
                        if (StartPagePrinter(hPrinter))
                        {
                            // Write your bytes.
                            bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                            EndPagePrinter(hPrinter);
                        }
                        EndDocPrinter(hPrinter);
                    }
                    ClosePrinter(hPrinter);
                }
                // If you did not succeed, GetLastError may give more information
                // about why not.
                if (bSuccess == false)
                {
                    dwError = Marshal.GetLastWin32Error();
                }
            }
            catch (Exception)
            {
                bSuccess = false;
            }
            return bSuccess;
        }

        private bool CheckPrinterStatusPostCommand(out PrinterStatus printerStatus)
        {
            bool? result = true;

            Thread.Sleep(50);
            sendcommand(checkStatusString);
            printerStatus = (PrinterStatus)usbportqueryprinter();

            while (printerStatus != PrinterStatus.OK && result == true)
            {
                LoggerService.Error(this, "Printer Error: " + printerStatus.ToString());

                // WPF MessageBox equivalent
                MessageBoxResult messageBoxResult = MessageBox.Show(
                    "Label has failed due to printer status: " + printerStatus.ToString() + "\r\n" +
                    "Please perform relevant maintenance and press 'OK' when finished",
                    "Message Box",
                    MessageBoxButton.OKCancel);

                if (messageBoxResult == MessageBoxResult.OK)
                {
                    DisableError();
                    Thread.Sleep(2000);
                    sendcommand(checkStatusString);
                    printerStatus = (PrinterStatus)usbportqueryprinter();
                    LoggerService.Error(this, printerStatus.ToString());
                }
                else
                {
                    result = false; // Exit the loop if "Cancel" is clicked
                }
            }

            if (printerStatus == PrinterStatus.OK)
            {
                LoggerService.Error(this, "Label Printed");
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DisableError()
        {
            openport("usb");
            sendcommand("\u001b!F");
            PrinterStatus status = (PrinterStatus)usbportqueryprinter();
            closeport();
        }

        public bool PrintTest()
        {
            bool success;
            IntPtr pBytes;
            Int32 dwCount;

            string aTmpRawString = $@"SIZE 52.10 mm, 25 mm
                GAP 3 mm, 0 mm
                SPEED 2
                DENSITY 13
                SET RIBBON ON
                DIRECTION 0,0
                REFERENCE 0,0
                OFFSET 0 mm
                SET PEEL OFF
                SET CUTTER OFF
                SET PARTIAL_CUTTER OFF
                SET TEAR ON
                CLS
                CODEPAGE 1252
                TEXT 313,147,""0"",180,39,36,""OK""
                PRINT 1,1
                ";

            // How many characters are in the string?
            dwCount = aTmpRawString.Length;
            // Assume that the printer is expecting ANSI text, and then convert
            // the string to ANSI text.
            pBytes = Marshal.StringToCoTaskMemAnsi(aTmpRawString);
            // Send the converted ANSI string to the printer.
            success = SendBytesToPrinter(_comName, pBytes, dwCount);
            Marshal.FreeCoTaskMem(pBytes);

            return success;
        }

        #endregion

        #endregion Methods

    }
}
