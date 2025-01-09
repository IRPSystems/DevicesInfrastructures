using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using LibUsbDotNet.Main;
using Services.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Markup;
using System.Xml.Linq;

namespace DeviceCommunicators.PowerSupplayGK
{
    public class PowerSupplayGK_Communicator: DeviceCommunicator
	{
		#region Fields


        public string Name;

		private string _ipAdderss;
		private ushort _port;

		private AutoResetEvent _waitForResponse;
		private byte[] _data; 
		private string _error;

		private ConcurrentDictionary<string, string> _codeToDescriptionDict;

		private object _getLockObj;

		private const int _timeout = 2000;


		#endregion Fields


		#region Properties


		private ModbusTCPSevice ModbusTCP
		{
			get => CommService as ModbusTCPSevice;
		}


		#endregion Properties

		#region Constructor

		public PowerSupplayGK_Communicator(LogLineListService logLineList) :
			base(logLineList)
		{
			_waitForResponse = new AutoResetEvent(false);
			_getLockObj = new object();
		}

		#endregion Constructor

		#region Methods

		protected override void InitErrorsDictionary()
		{
			_codeToDescriptionDict = new ConcurrentDictionary<string, string>();
			_codeToDescriptionDict["1"] = "Illegal function.";
			_codeToDescriptionDict["2"] = "Illegal data address.";
			_codeToDescriptionDict["3"] = "Illegal data value.";
			_codeToDescriptionDict["4"] = "Slave device failure.";
			_codeToDescriptionDict["6"] = "Slave device busy.";
		}

		public void Init(
			bool isUdpSimulation,
			string ipAdderss,
			ushort port)
        {
			_ipAdderss = ipAdderss;
			_port = port;


			if (isUdpSimulation)
			{

			}
            else
			    CommService = new ModbusTCPSevice(_ipAdderss, _port, 255);


			ModbusTCP.MessageReceivedEvent += _modbusTCPSevice_MessageReceivedEvent;
			ModbusTCP.ErrorEvent += _modbusTCPSevice_ErrorEvent;
			CommService.Init(true);

			InitBase();
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
			lock (_getLockObj)
			{
				try
				{
					if (!(param is PowerSupplayGK_ParamData gk_ParamData))
						return;

					double val = Convert.ToUInt16(value);
					val /= gk_ParamData.Scale;
					byte[] buffer = BitConverter.GetBytes((ushort)val);


					_data = null;
					ModbusTCP.WriteSingleRegister(7, 255, gk_ParamData.WriteAddress, buffer);
					ModbusTCP.WriteSingleRegister(7, 255, gk_ParamData.WriteTriggerAddress, buffer); // trigger

					System.Threading.Thread.Sleep(100);
					_waitForResponse.WaitOne(_timeout);

					if (_data == null)
					{
						if (callback != null)
						{
							string errorDescription = GetErrorDescription();

							if (_error != null)
								callback(param, CommunicatorResultEnum.Error, errorDescription);
							else
								callback(param, CommunicatorResultEnum.NoResponse, "No response");
						}
					}
					else
					{

						double recVal = BitConverter.ToUInt16(_data, 0);
						_data = null;
						param.Value = recVal * gk_ParamData.Scale;



						if (recVal == val)
						{
							callback(param, CommunicatorResultEnum.OK, null);
						}
						else
							callback(param, CommunicatorResultEnum.Error, "Set value failed");
					}


				}
				catch (Exception ex)
				{
					LoggerService.Error(this, "Failed to set value for parameter: " + param.Name, ex);
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Exception when sending");
				}
			}
		}

		private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			lock (_getLockObj)
			{
				try
				{
					if (!(param is PowerSupplayGK_ParamData gk_ParamData))
						return;

					_data = null;
					if (gk_ParamData.WriteAddress != 0)
						ModbusTCP.ReadHoldingRegister(3, 255, gk_ParamData.WriteAddress, 1);
					else					
						ModbusTCP.ReadInputRegister(4, 255, gk_ParamData.ReadAddress, 1);



					_waitForResponse.WaitOne(_timeout);

					if (_data == null)
					{

						if (callback != null)
						{
							string errorDescription = GetErrorDescription();

							if (_error != null)
								callback(param, CommunicatorResultEnum.Error, errorDescription);
							else
								callback(param, CommunicatorResultEnum.NoResponse, "No response");
						}
					}
					else
					{
						Array.Reverse(_data);
						double val = BitConverter.ToUInt16(_data, 0);
						_data = null;
						param.Value = val * gk_ParamData.Scale;

						if (callback != null)
							callback(param, CommunicatorResultEnum.OK, null);
					}
				}
				catch (Exception ex)
				{
					LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Exception when sending");
				}
			}
			
		}

		private string GetErrorDescription()
		{
			string errorDescription = null;
			if (string.IsNullOrEmpty(_error))
				return errorDescription;


			string errorCode = _error;
			if (_codeToDescriptionDict.ContainsKey(_error))
				errorCode = _codeToDescriptionDict[_error];
			errorDescription =
				"Get value failed.\r\n" +
				errorCode;

			return errorDescription;
		}

		private void _modbusTCPSevice_MessageReceivedEvent(byte[] data)
		{
			_data = data;
			_waitForResponse.Set();

		}


		private void _modbusTCPSevice_ErrorEvent(string error)
		{
			_waitForResponse.Set();
			_error = error;
		}

		#endregion Methods

	}
}
