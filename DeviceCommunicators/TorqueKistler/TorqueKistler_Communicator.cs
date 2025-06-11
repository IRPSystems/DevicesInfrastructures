
using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using Services.Services;
using System.Timers;
using System;
using DeviceCommunicators.Models;
using System.Collections.Concurrent;
using NationalInstruments.DataInfrastructure;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Media.Animation;

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

		public ConcurrentDictionary<string, string> _ErrorToDescription;

		private string _idenText;

		#endregion Fields


		#region Properties


		private ISerialService _serial_port
		{
			get => CommService as ISerialService;
		}


		#endregion Properties

		#region Constructor

		public TorqueKistler_Communicator(LogLineListService logLineList) :
			base(logLineList)
		{
			_timeoutTimer = new System.Timers.Timer(1000);
			_timeoutTimer.Elapsed += TimoutElapsedEventHandler;
		}

		#endregion Constructor

		#region Methods

		protected override void InitErrorsDictionary()
		{
			_ErrorToDescription = new ConcurrentDictionary<string, string>();
			_ErrorToDescription["ERR-100"] = "Command not understood.";
			_ErrorToDescription["ERR-101"] = "\" ? \" was not added for a query.";
			_ErrorToDescription["ERR-104"] = "Calculation steps caused an overflow.";
			_ErrorToDescription["ERR-105"] = "Error accessing non-volatile buffer memory.";
			_ErrorToDescription["ERR-106"] = "Access to protected buffer memory.";
			_ErrorToDescription["ERR-108"] = "Transmitted string too long.";
			_ErrorToDescription["ERR-109"] = "Transmitted numerical value is invalid.";
			_ErrorToDescription["ERR-121"] = "Invalid output format";
		}

		public void Init(
			bool isUdpSimulation,
			string comName,
			int baudtate,
			string idenText,
			int rxPort = 0,
			int txPort = 0,
			string address = "")
		{
			_name_comport = comName;
			_boud_rate = baudtate;
			_idenText = idenText;

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

				string cmd = tk_ParamData.Command + value + "\r";
				string buffer = null;
				for (int i = 0; i < 5; i++)
				{
					_serial_port.Send(cmd);

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
					string description =
						$"Error {buffer}: {_ErrorToDescription[buffer]}";
					callback?.Invoke(param, CommunicatorResultEnum.Error, description);
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

				
				string cmd = "MEAS:ALL?\r";
				if(param.Name != "Torque" && param.Name != "Speed")
					cmd = tk_ParamData.Command + "\r";


				string buffer = null;
				for (int i = 0; i < 20; i++)
				{
					_serial_port.Send(cmd);

					buffer = WaitForResponse(tk_ParamData);

					if (!string.IsNullOrEmpty(buffer) && !buffer.Contains("ERR-"))
						break;

					
				}

				if (string.IsNullOrEmpty(buffer))
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}

				if (tk_ParamData.Command == "*IDN" && buffer.Contains(_idenText) == false)
				{
					callback?.Invoke(
						param,
						CommunicatorResultEnum.InvalidValue,
						"The device is not Torque Kistler\r\n" + buffer);
					return;
				}

				buffer = buffer.Replace("\r", string.Empty);
				if (buffer.ToUpper().Contains("ERR-"))
				{
					string description =
						$"Error {buffer}: {_ErrorToDescription[buffer]}";
					callback?.Invoke(param, CommunicatorResultEnum.Error, description);
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

				_serial_port.Read(out buffer);
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
