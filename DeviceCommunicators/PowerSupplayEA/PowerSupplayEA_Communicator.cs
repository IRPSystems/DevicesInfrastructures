using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace DeviceCommunicators.PowerSupplayEA
{
    public class PowerSupplayEA_Communicator: DeviceCommunicator
	{
		#region Fields

		private string _name_comport;
		private int _boud_rate;

        public string Name;


		private List<string> _onOffCommands;

		private bool _isUseRampForOnOff;

		#endregion Fields


		#region Properties


		private ISerialService _serial_port
		{
			get => CommService as ISerialService;
		}


		#endregion Properties

		#region Constructor

		public PowerSupplayEA_Communicator(LogLineListService logLineList) :
			base(logLineList)
		{

			_onOffCommands = new List<string>() { "SYST:LOCK", "OUTP" };
			_isUseRampForOnOff = true;
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

            if(isUdpSimulation)
				CommService = new SerialUdpSimulationService(rxPort, txPort, address);
            else
			    CommService = new SerialService(_name_comport, _boud_rate);

			CommService.Init(false);

			InitBase();
		}

		public override void Dispose()
		{

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
				if (!(param is PowerSupplayEA_ParamData ea_ParamData))
					return;

				

                string cmd = ea_ParamData.Cmd + " " + value;

				LoggerService.Inforamtion(this, "Setting \"" + ea_ParamData.Name + "\" - cmd: " + cmd);

				if (_onOffCommands.IndexOf(ea_ParamData.Cmd) >= 0)
				{
					if (value == 1)
					{
						LoggerService.Inforamtion(this, "Setting \"" + ea_ParamData.Name + "\" ON");
						cmd = ea_ParamData.Cmd + " ON";

						Task task = Task.Run(() =>
						{
							TurnOnProcess(cmd);
						}, _cancellationToken);

						task.Wait();
					}
					else if (value == 0)
					{
						LoggerService.Inforamtion(this, "Setting \"" + ea_ParamData.Name + "\" OFF");
						cmd = ea_ParamData.Cmd + " OFF";

						Task task = Task.Run(() =>
						{
							TurnOffProcess(cmd);
						}, _cancellationToken);

						task.Wait();
					}

					System.Threading.Thread.Sleep(2000);
				}
				else
				{
					_serial_port.Send(cmd);
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
				if (!(param is PowerSupplayEA_ParamData ea_ParamData))
					return;

				

				string cmd = ea_ParamData.Cmd;
				cmd = ea_ParamData.Cmd + "?";

				//LoggerService.Inforamtion(this, "Getting \"" + ea_ParamData.Name + "\" - cmd: " + cmd);

				_serial_port.Send(cmd);

				string buffer = Read();

				if (string.IsNullOrEmpty(buffer))
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, null);
					return;
				}

				

				if (_onOffCommands.IndexOf(ea_ParamData.Cmd) >= 0)
				{
					if (buffer.ToLower() == "ON")
						param.Value = 1;
					else if (buffer == "OFF")
						param.Value = 0;
					else
					{
						callback?.Invoke(param, CommunicatorResultEnum.Error, "Invalid value");
						return;
					}

					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
				}
				else
				{
					double dVal;
					bool res = GetValueFromMessage(buffer, out dVal);

					if (res)
						param.Value = dVal;
					else
						param.Value = buffer;
					
					callback?.Invoke(param, CommunicatorResultEnum.OK, null);
				}
			}
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
		}

		private string Read()
		{
			string buffer = null;
			DateTime startTime = DateTime.Now;
			TimeSpan timeout50ms = new TimeSpan(0, 0, 0, 0, 50);

			while (string.IsNullOrEmpty(buffer) &&
				(DateTime.Now - startTime) < timeout50ms)
			{
				_serial_port.Read(out buffer);
				if (string.IsNullOrEmpty(buffer) == false)
					break;

				System.Threading.Thread.Sleep(1);
			}

			return buffer;
		}

		private bool GetValueFromMessage(string buffer, out double dVal)
		{
			buffer = Regex.Replace(buffer, "[A-Za-z ]", "");
			bool res = double.TryParse(buffer, out dVal);

			return res;
		}


		private void TurnOffProcess(string offCmd)
		{
			if (!_isUseRampForOnOff)
			{
				_serial_port.Send(offCmd);
				return;
			}

			double startVoltage;
			bool res = GetVoltage(out startVoltage);
			if (res == false)
				return;

			for (double i = startVoltage; i >= 0 && !_cancellationToken.IsCancellationRequested; i--)
			{
				_serial_port.Send("SOUR:VOLTAGE " + i.ToString());
				System.Threading.Thread.Sleep(100);
			}

			_serial_port.Send(offCmd);

			_serial_port.Send("SOUR:VOLTAGE " + startVoltage.ToString());
		}

		private void TurnOnProcess(string onCmd) 
		{
			if (!_isUseRampForOnOff)
			{
				_serial_port.Send(onCmd);
				return;
			}

			double startVoltage;
			bool res = GetVoltage(out startVoltage);
			if (res == false)
				return;

			double startCurrent;
			res = GetCurrent(out startCurrent);
			if (res == false)
				return;

			_serial_port.Send("SOUR:VOLTAGE 0");
			_serial_port.Send("SOUR:CURRENT 2");
			_serial_port.Send(onCmd);

			for (double i = 0; i < startVoltage && !_cancellationToken.IsCancellationRequested; i++)
			{
				_serial_port.Send("SOUR:VOLTAGE " + i.ToString());
				System.Threading.Thread.Sleep(100);
			}

			_serial_port.Send("SOUR:VOLTAGE " + startVoltage.ToString());
			_serial_port.Send("SOUR:CURRENT " + startCurrent.ToString());

		}

		private bool GetVoltage(out double dVal)
		{
			dVal = 0;
			for (int i = 0; i < 3; i++)
			{
				_serial_port.Send("SOUR:VOLTAGE?");
				string buffer = Read();
				if (string.IsNullOrEmpty(buffer) == false)
				{
					bool res = GetValueFromMessage(buffer, out dVal);
					if (res == false)
						continue;

					return true;
				}
			}

			return false;
		}

		private bool GetCurrent(out double dVal)
		{
			dVal = 0;
			for (int i = 0; i < 3; i++)
			{
				_serial_port.Send("SOUR:CURRENT?");
				string buffer = Read();
				if (string.IsNullOrEmpty(buffer) == false)
				{
					bool res = GetValueFromMessage(buffer, out dVal);
					if (res == false)
						continue;

					return true;
				}
			}

			return false;
		}

		public void SetIsUseRampForOnOff(bool isUseRamp)
		{
			_isUseRampForOnOff = isUseRamp;
		}

		#endregion Methods

	}
}
