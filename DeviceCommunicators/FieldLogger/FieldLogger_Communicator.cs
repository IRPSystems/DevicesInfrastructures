using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DeviceCommunicators.FieldLogger
{
    public class FieldLogger_Communicator : DeviceCommunicator, IDataLoggerCommunicator
	{
		#region Fields

		private string _ipAddress;
		private ushort _port;
		private byte _modbusAddress;
		private ushort _startAddress;
		private ushort _noOfItems;
		private ushort _sizeOfItems;

		private ObservableCollection<short> _channelsValue;

		#endregion Fields


		#region Properties

		public int NumberOfChannels
		{
			get => 8;
		}

		private ModbusTCPSevice ModbusTCPSevice
		{
			get => CommService as ModbusTCPSevice;
		}


		#endregion Properties

		#region Constructor

		public FieldLogger_Communicator()
        {
			_channelsValue = new ObservableCollection<short>();
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

			GetValues();

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

				if(_channelsValue.Count == 0)
				{
					callback?.Invoke(param, CommunicatorResultEnum.NoResponse, "");
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

		private void GetValues()
		{
			Task.Run(() =>
			{
				while(!_cancellationToken.IsCancellationRequested) 
				{
					byte[] buffer;
					ModbusTCPSevice.Read(out buffer);
					if (buffer == null)
						continue;

					_channelsValue.Clear();

					for (int i = 0; i < buffer.Length; i++)
					{
						short val = (short)(buffer[i] << 8);
						i++;
						val += buffer[i];
						_channelsValue.Add(val);
					}

					System.Threading.Thread.Sleep(1);
				}

			}, _cancellationToken);
		}

		#endregion Methods

	}
}
