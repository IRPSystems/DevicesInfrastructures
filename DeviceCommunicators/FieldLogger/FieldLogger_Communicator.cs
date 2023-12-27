using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Timers;

namespace DeviceCommunicators.FieldLogger
{
    public class FieldLogger_Communicator : DeviceCommunicator
	{
		#region Fields

		private string _ipAddress;
		private ushort _port;
		private byte _modbusAddress;
		private ushort _startAddress;
		private ushort _noOfItems;
		private ushort _sizeOfItems;


		private System.Timers.Timer _timer;
		public List<short> _channelsValue { get; set; }

		#endregion Fields


		#region Properties


		private ModbusTCPSevice ModbusTCPSevice
		{
			get => CommService as ModbusTCPSevice;
		}


		#endregion Properties

		#region Constructor

		public FieldLogger_Communicator()
        {
			_timer = new System.Timers.Timer(1000);
			_timer.Elapsed += _timer_Elapsed;

			_channelsValue = new List<short>();
		}

		#endregion Constructor

		#region Methods

		public void Init(
			bool isUdpSimulation,
			string ipAddress,
			ushort port,
			byte modbusAddress,
			ushort startAddress,
			ushort noOfItems,
			ushort sizeOfItems)
		{
			_ipAddress = ipAddress;
			_port = port;
			_modbusAddress = modbusAddress;
			_startAddress = startAddress;
			_noOfItems = noOfItems;
			_sizeOfItems = sizeOfItems;


			if (isUdpSimulation)
			{
			}
			else
				CommService = new ModbusTCPSevice(
					_ipAddress,
					_port,
					_modbusAddress,
					_startAddress,
					_noOfItems,
					_sizeOfItems);

			CommService.Init(false);

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
			callback?.Invoke(param, CommunicatorResultEnum.OK, "");
		}


        private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
            try
            {
				if(!(param is FieldLogger_ParamData fieldLogger_ParamData))
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "");
					return;
				}

				param.Value = _channelsValue[fieldLogger_ParamData.Channel - 1];
				callback?.Invoke(param, CommunicatorResultEnum.OK, "");

			}
            catch(Exception ex) 
            { 
                LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
            }
		}

		private void _timer_Elapsed(object sender, ElapsedEventArgs e)
		{
			ReadChannels();
		}

		private void ReadChannels()
		{
			byte[] buffer;
			ModbusTCPSevice.Read(out buffer);

			_channelsValue.Clear();

			for (int i = 0; i < buffer.Length; i++)
			{
				short val = (short)(buffer[i] << 8);
				i++;
				val += buffer[i];
				_channelsValue.Add(val);
			}
		}

		#endregion Methods

	}
}
