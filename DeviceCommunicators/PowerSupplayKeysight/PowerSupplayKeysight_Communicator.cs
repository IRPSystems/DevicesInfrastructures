using Communication.Services;
using DeviceCommunicators.General;
using Services.Services;
using System;
using DeviceCommunicators.Enums;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.PowerSupplayKeysight
{
    public  class PowerSupplayKeysight_Communicator  : DeviceCommunicator, IDisposable
    {

		 #region Fields

		public int channel = 1; // channel of scope




       
        private string _iPaddres;
        private int _port;


        #endregion Fields

        #region Properties

        private TcpStaticService TCPCommService
        {
            get => CommService as TcpStaticService;
        }

        #endregion Properties

        #region Constructor

        public PowerSupplayKeysight_Communicator ()
        { 
            
        }

		#endregion Constructor

		#region Methods

		public void Init(
            bool isUSBSimulator,
            string iPaddres, 
            int port)
		{

			try
			{
				_iPaddres = iPaddres;
				_port = port;

				CommService = new TcpStaticService(_iPaddres, _port);
				CommService.Init(true);



				InitBase();
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to init the ScopeKeySight", ex);
			}


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
        
 
        public void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is PowerSupplayKeysight_ParamData psKeySight))
                    return;

                
				string cmd = psKeySight.Command;
                if (cmd == "SYSTem:COMMunicate:RLSTate")
                {
                    if (value == 0)
                        cmd += " LOCal";
                    else if (value == 1)
                        cmd += " REMote";
                }
                else if (cmd == "OUTP:COUP:CHAN")
				{
					//if (value == 1)
					//	cmd += " CH1";
					//else if (value == 2)
					//	cmd += " CH2";
				}
				else
                    cmd += " " + value.ToString();

				TCPCommService.Send(cmd + "\n");

				callback?.Invoke(param, CommunicatorResultEnum.OK, null);
            }

            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to set Command for Switch relay" + param.Name, ex);
            }
        }
        

        public void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
        {
            try
            {
                if (!(param is PowerSupplayKeysight_ParamData psKeySight))
                    return;

                string cmd = $"{psKeySight.Command}?";

				string response = null;
                DateTime startTime = DateTime.Now; 
				for (int i = 0; i < 5; i++)
                {
                    TCPCommService.Send(cmd + "\n");

                    while ((DateTime.Now - startTime) < TimeSpan.FromSeconds(50))
                    {
                        TCPCommService.Read(out response);
                        if (!string.IsNullOrEmpty(response))
                            break;

                        System.Threading.Thread.Sleep(1);
                    }

                    if (!string.IsNullOrEmpty(response))
                        break;

					System.Threading.Thread.Sleep(1);
				}

				if (string.IsNullOrEmpty(response))
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}

				response = response.Trim(new char[] { '\0', '\n' });


				if (psKeySight.Command == "VOLTage" ||
					psKeySight.Command == "CURRent")
                {
                    double d;
                    bool res = double.TryParse(response, out d);
                    if (res == false)
					{
						callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
						return;
					}

                    param.Value = d;
				}
                else
					param.Value = response;

                
				callback?.Invoke(param, CommunicatorResultEnum.OK, null);

            }
            catch (Exception ex)
            {
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
        }


        #endregion Methods
    }
}
