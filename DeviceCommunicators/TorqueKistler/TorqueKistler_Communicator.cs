
using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using Services.Services;
using System.Timers;
using System;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.TorqueKistler
{
	public class TorqueKistler_Communicator: DeviceCommunicator
	{
		#region Fields

		private string _name_comport;
		private int _boud_rate;

		public string Name;

		private bool _isTimeout;
		private System.Timers.Timer _timeoutTimer;

		#endregion Fields


		#region Properties


		private ISerialService SerialService
		{
			get => CommService as ISerialService;
		}


		#endregion Properties

		#region Constructor

		public TorqueKistler_Communicator()
		{
			_timeoutTimer = new System.Timers.Timer(1000);
			_timeoutTimer.Elapsed += TimoutElapsedEventHandler;
		}

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string comName,
			int baudtate,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
		{
			_name_comport = comName;
			_boud_rate = baudtate;

			if (isUdpSimulation)
				CommService = new SerialUdpSimulationService(rxPort, txPort, address);
			else
				CommService = new SerialService(_name_comport, _boud_rate);

			CommService.Init(false);

			InitBase();
		}

		public override void Dispose()
		{
			if (_timeoutTimer != null)
				_timeoutTimer.Stop();

			base.Dispose();
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

		private void SetParamValue_Do(DeviceParameterData param, double value, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				if (!(param is TorqueKistler_ParamData tk_ParamData))
					return;

				string cmd = tk_ParamData.Command + value + "\r\n";
				string buffer = null;
				for (int i = 0; i < 5; i++)
				{
					lock (_lockObj)
					{
						SerialService.Send(cmd);
					}

					buffer = WaitForResponse(tk_ParamData);
					if (!string.IsNullOrEmpty(buffer) && !buffer.Contains("ERR-"))
						break;

				}

				if (string.IsNullOrEmpty(buffer))
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}

				buffer = buffer.Replace("\r", string.Empty);
				if (buffer.ToUpper().Contains("ERR-"))
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Error: " + buffer);
					return;
				}

				callback?.Invoke(param, CommunicatorResultEnum.OK, null);
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
			}
		}


		private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				if (!(param is TorqueKistler_ParamData tk_ParamData))
					return;

				
				string cmd = "MEAS:ALL?\r\n";
				if(param.Name != "Torque" && param.Name != "Speed")
					cmd = tk_ParamData.Command + "\r\n";


				string buffer = null;
				for (int i = 0; i < 20; i++)
				{
					lock (_lockObj)
					{
						SerialService.Send(cmd);
					}

					buffer = WaitForResponse(tk_ParamData);

					if (!string.IsNullOrEmpty(buffer) && !buffer.Contains("ERR-"))
						break;

					
				}

				if (string.IsNullOrEmpty(buffer))
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}

				buffer = buffer.Replace("\r", string.Empty);
				if (buffer.ToUpper().Contains("ERR-"))
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Error: " + buffer);
					return;
				}

				if (param.Name == "Speed" || param.Name == "Torque") 
				{
					string[] paramList = buffer.Split('|');
					if(paramList.Length < 4)
					{
						callback?.Invoke(param, CommunicatorResultEnum.Error, "Invalid value received: " + buffer);
						return;
					}

					double dVal= 0;
					bool res = false;
					if(param.Name == "Torque")
						res = double.TryParse(paramList[1], out dVal);
					else if (param.Name == "Speed")
						res = double.TryParse(paramList[2], out dVal);

					if (!res)
					{
						callback?.Invoke(param, CommunicatorResultEnum.Error, "Invalid value received: " + buffer);
						return;
					}

					param.Value = dVal;

				}


				

				callback?.Invoke(param, CommunicatorResultEnum.OK, null);


			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
			}
		}

		private string WaitForResponse(TorqueKistler_ParamData tk_ParamData)
		{
			_isTimeout = false;
			_timeoutTimer.Start();
			string buffer = null;
			while (string.IsNullOrEmpty(buffer))
			{
				if (_isTimeout)
					break;

				lock (_lockObj)
				{
					SerialService.Read(out buffer);
				}

				if (string.IsNullOrEmpty(buffer) == false)
					break;

				System.Threading.Thread.Sleep(1);
			}

			_timeoutTimer.Stop();

			return buffer;
		}

		private void TimoutElapsedEventHandler(object sender, ElapsedEventArgs e)
		{
			_isTimeout = true;
		}

		#endregion Methods
	}
}
