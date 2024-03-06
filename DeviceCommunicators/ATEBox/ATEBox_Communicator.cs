
using Communication.Interfaces;
using Communication.Services;
using DeviceCommunicators.Enums;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using Services.Services;
using System;
using Entities.Enums;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Entities.Models;
using Newtonsoft.Json;
using DeviceCommunicators.MCU;

namespace DeviceCommunicators.ATEBox
{
	public class ATEBox_Communicator: DeviceCommunicator
	{
		#region Properties

		private ISerialService _serial_port
		{
			get => CommService as ISerialService;
		}


		#endregion Properties

		#region Constructor

		public ATEBox_Communicator()
		{
			#region Create device
			//DeviceData deviceData = new DeviceData()
			//{
			//	Name = "ATEBox",
			//	DeviceType = DeviceTypesEnum.ATEBox,
			//	ParemetersList = new ObservableCollection<DeviceParameterData>()
			//};

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "Digital I/O",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.IO,
			//	DropDown = new List<DropDownParamData>()
			//	{ 
			//		new DropDownParamData() { Name = "DIG1", Value = "1"},
			//		new DropDownParamData() { Name = "DIG2", Value = "2"},
			//		new DropDownParamData() { Name = "DIG3", Value = "3"},
			//		new DropDownParamData() { Name = "DIG4", Value = "4"},
			//		new DropDownParamData() { Name = "DIG5", Value = "5"},
			//		new DropDownParamData() { Name = "DIG6", Value = "6"},
			//		new DropDownParamData() { Name = "DIG7", Value = "7"},
			//		new DropDownParamData() { Name = "DIG8", Value = "8"},
			//		new DropDownParamData() { Name = "DIG9", Value = "9"},
			//		new DropDownParamData() { Name = "DIG10", Value = "10"},
			//		new DropDownParamData() { Name = "DIG11", Value = "11"},
			//		new DropDownParamData() { Name = "DIG12", Value = "12"},
			//		new DropDownParamData() { Name = "DIG13", Value = "13"},
			//		new DropDownParamData() { Name = "DIG14", Value = "14"},
			//	},
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "Set Analog I/O",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.IO,
			//	DropDown = new List<DropDownParamData>()
			//	{
			//		new DropDownParamData() { Name = "ANLOG1", Value = "1"},
			//		new DropDownParamData() { Name = "ANLOG2", Value = "2"},
			//		new DropDownParamData() { Name = "ANLOG3", Value = "3"},
			//	},
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "Get Analog I/O",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.IO,
			//	DropDown = new List<DropDownParamData>()
			//	{
			//		new DropDownParamData() { Name = "ANLOG1", Value = "1"},
			//		new DropDownParamData() { Name = "ANLOG2", Value = "2"},
			//		new DropDownParamData() { Name = "ANLOG3", Value = "3"},
			//		new DropDownParamData() { Name = "ANLOG4", Value = "4"},
			//		new DropDownParamData() { Name = "ANLOG5", Value = "5"},
			//		new DropDownParamData() { Name = "ANLOG6", Value = "6"},
			//		new DropDownParamData() { Name = "ANLOG7", Value = "7"},
			//		new DropDownParamData() { Name = "ANLOG8", Value = "8"},
			//		new DropDownParamData() { Name = "ANLOG9", Value = "9"},
			//		new DropDownParamData() { Name = "ANLOG10", Value = "10"},
			//	},
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "Leds",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.IO,
			//	DropDown = new List<DropDownParamData>()
			//	{
			//		new DropDownParamData() { Name = "LED0", Value = "0"},
			//		new DropDownParamData() { Name = "LED1", Value = "1"},
			//		new DropDownParamData() { Name = "LED2", Value = "2"},
			//		new DropDownParamData() { Name = "LED3", Value = "3"},
			//	},
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "RS232 Start",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "ATE_RS232_Start",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "RS422 Start",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "ATE_RS422_Start",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "RS485 Start",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "ATE_RS485_Start",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "CAN Start",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "ATE_CAN_Start",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "LOAD 1A",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "EN_LOAD_1A",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "LOAD 2A",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "EN_LOAD_2A",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "HOT SWAP EN",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "HOT_SWAP_EN",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "V DIGITAL EN",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "V_DIGITAL_EN",
			//});

			//deviceData.ParemetersList.Add(new ATEBox_ParamData()
			//{
			//	Name = "Fan Control",
			//	InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
			//	Command = "Fan_Control",
			//});

			//JsonSerializerSettings settings = new JsonSerializerSettings();
			//settings.Formatting = Formatting.Indented;
			//settings.TypeNameHandling = TypeNameHandling.All;
			//var sz = JsonConvert.SerializeObject(deviceData, settings);
			//System.IO.File.WriteAllText(@"C:\Users\smadar\Documents\Stam\Json files\ATABox.json", sz);
			#endregion Create device
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
			if (isUdpSimulation)
				CommService = new SerialUdpSimulationService(rxPort, txPort, address);
			else
				CommService = new SerialService(comName, baudtate);

			_serial_port.Init(false);

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

		private void SetParamValue_Do(
			DeviceParameterData param,
			double value,
			Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				if(!(param is ATEBox_ParamData ateBoxParam))
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "The parameter is of the wrong type");
					return;
				}

				string message = string.Empty;
				switch(ateBoxParam.InterfaceType)
				{
					case ATEBox_InterfaceTyleEnum.CAN:
						message = GetCANInterfaceMessage(ateBoxParam);
						break;
					case ATEBox_InterfaceTyleEnum.IO:
						message = "Set_";
						if (ateBoxParam.Name == "Digital I/O")
							message = "DIO ";
						else if (ateBoxParam.Name == "Set Analog I/O")
							message = "Analog ";
						else if (ateBoxParam.Name == "Leds")
							message = "LEDs ";
						message += ateBoxParam.Channel + "_" + ((int)ateBoxParam.Value).ToString();
						break;
					case ATEBox_InterfaceTyleEnum.Commands:
						message = ateBoxParam.Command + " " + ((int)ateBoxParam.Value).ToString();
						break;
					default:
						callback?.Invoke(param, CommunicatorResultEnum.Error, "Unknown interface \"" + ateBoxParam.InterfaceType + "\"");
						return;
				}

				if(string.IsNullOrEmpty(message))
				{
					callback?.Invoke(param, CommunicatorResultEnum.Error, "Failed to parse the message");
					return;
				}

				_serial_port.Send(message);
			}
			catch (Exception ex)
			{
				string error = "Failed to set value for parameter: " + param.Name;
				LoggerService.Error(this, error, ex);
				callback?.Invoke(param, CommunicatorResultEnum.Error, error);

			}
		}

		private string GetCANInterfaceMessage(ATEBox_ParamData ateBoxParam)
		{
			string message = string.Empty;

			if (ateBoxParam.MCUParameter == null)
				return message;

			if(!(ateBoxParam.MCUParameter is MCU_ParamData mcuParam))
				return message;

			byte[] id = new byte[3];
			byte[] buffer = new byte[8];
			MCU_Communicator.ConvertToData(mcuParam, (double)ateBoxParam.Value, ref id, ref buffer, true);
			ulong canMsg = BitConverter.ToUInt64(buffer, 0);

			message = "CAN 0x" + ateBoxParam.CANID.ToString("X") + "_" + canMsg.ToString("X");

			return message;
		}

		private void GetParamValue_Do(DeviceParameterData param, Action<DeviceParameterData, CommunicatorResultEnum, string> callback)
		{
			try
			{
				
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
			}
		}

		#endregion Methods
	}
}
