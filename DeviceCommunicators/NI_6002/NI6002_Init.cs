using NationalInstruments.DAQmx;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceCommunicators.NI_6002
{
     public class  NI6002_Init
    {

         private string _ni_a;
         private string _ni_b;

         public string NI_a
        {
            get { return _ni_a; }  
            private set { _ni_a = value; }  
        }

         public string NI_b
        {
            get { return _ni_b; }  
            private set { _ni_b = value; }  
        }

        static private List<string> ConnectedNIDevices;

        const int portToTest = 1;
        const int lineToTest = 2;

        private LogLineListService _logLineList;

		#region Constructor 

		public NI6002_Init(LogLineListService logLineList)
        {
            _logLineList = logLineList;
        }

		#endregion Constructor

		#region Methods
		static public List<string> ReturnCurrentDeviceList()
        {
            return ConnectedNIDevices;
        }

        static private void UpdateDeviceList()
        {
            try
            {
                ConnectedNIDevices = new List<string>(DaqSystem.Local.Devices);
            }
            catch(Exception ex) 
            {
                LoggerService.Error($"typeof(NI6002_Init).Name", "failed to use daq", ex);
            }
        }

        public void BindDevices()
        {
            UpdateDeviceList();
            if (ConnectedNIDevices.Count >= 2)
            {
                NI6002_Communicator nitemp = new NI6002_Communicator(_logLineList);
                nitemp._commmand_to_device = new NI6002_Command(ConnectedNIDevices[0]);

                if (IdentifyDeviceByWiring(nitemp))
                {
                    NI_a = ConnectedNIDevices[0];
                    NI_b = ConnectedNIDevices[1];
                }
                else
                {
                    NI_a = ConnectedNIDevices[1];
                    NI_b = ConnectedNIDevices[0];
                }
            }
            else if (ConnectedNIDevices.Count == 1)
            {
                NI_a = ConnectedNIDevices[0];
            }
        }

        static private bool IdentifyDeviceByWiring(NI6002_Communicator nitemp)
        {
            string hexstring = nitemp._commmand_to_device.DigitalIO_input(portToTest, lineToTest);
            int result;

            if (hexstring.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                hexstring = hexstring.Substring(2);
            }

            bool res = int.TryParse(hexstring, out result);

            if (res)
                return result == 0 ? false : true;
            else
                return false;

        }
        #endregion Methods
    }
}
