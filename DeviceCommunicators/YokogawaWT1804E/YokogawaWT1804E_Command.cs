using DeviceCommunicators.Interfaces;
using Services.Services;
using System;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using TmctlAPINet;

namespace DeviceCommunicators.YokogawaWT1804E
{
    public class YokogawaWT1804E_Command : IWT1804E
    {
		#region Properties and Fields

		private TMCTL _yokogawa = new TMCTL();
        public int device_index { get; set; } = 0;
        private int _device_id = 0;

        public bool IsInitialized { get; set; }

		#endregion Properties and Fields

		#region Constructor

		public YokogawaWT1804E_Command()
        {
			IsInitialized = true;			
        }

		#endregion Constructor

		#region Methods

		public void Init(string ip)
        {
            try
            {
				_device_id = -1;
				int ret = _yokogawa.Initialize(TMCTL.TM_CTL_VXI11, ip, ref _device_id);
                if(ret == 0)
                    IsInitialized = true;

                Send("COMMunicate:REMote ON");
                Send(":*IDN?");
                Send(":NUMERIC:NORMAL:NUMBER 1");
                Send(":NUMERIC:FORMAT ASCII");

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to init the WT1804E", ex);
            }


        }

        public void Dispose()
        {
            _yokogawa.Finish(_device_id);
			IsInitialized = false;

		}


        public bool Send(string data)
        {
            int ret = _yokogawa.Send(device_index, data);
            //if (ret != 0)
            //    return false;

            return true;
        }




        public string Read_data()
        {
            int rln = 1000;
            StringBuilder temp = new StringBuilder(41000);

            int ret = _yokogawa.Receive(device_index, temp, 1000, ref rln);
			//if (ret != 0)
			//	return null;

			return temp.ToString();
		}

		public int Receive(out StringBuilder temp)
		{
			int rln = 1000;
			temp = new StringBuilder(41000);
			int ret = _yokogawa.Receive(0, temp, 1000, ref rln);

            return ret;
		}







		#endregion Methods

	}


}
