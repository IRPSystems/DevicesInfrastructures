
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
			DeviceData deviceData = new DeviceData()
			{
				Name = "ATEBox",
				DeviceType = DeviceTypesEnum.ATEBox,
				ParemetersList = new ObservableCollection<DeviceParameterData>()
			};

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "Digital I/O",
				InterfaceType = ATEBox_InterfaceTyleEnum.IO,
				DropDown = new List<DropDownParamData>()
				{ 
					new DropDownParamData() { Name = "DIG1", Value = "1"},
					new DropDownParamData() { Name = "DIG2", Value = "2"},
					new DropDownParamData() { Name = "DIG3", Value = "3"},
					new DropDownParamData() { Name = "DIG4", Value = "4"},
					new DropDownParamData() { Name = "DIG5", Value = "5"},
					new DropDownParamData() { Name = "DIG6", Value = "6"},
					new DropDownParamData() { Name = "DIG7", Value = "7"},
					new DropDownParamData() { Name = "DIG8", Value = "8"},
					new DropDownParamData() { Name = "DIG9", Value = "9"},
					new DropDownParamData() { Name = "DIG10", Value = "10"},
					new DropDownParamData() { Name = "DIG11", Value = "11"},
					new DropDownParamData() { Name = "DIG12", Value = "12"},
					new DropDownParamData() { Name = "DIG13", Value = "13"},
					new DropDownParamData() { Name = "DIG14", Value = "14"},
				},
			});

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "Analog I/O",
				InterfaceType = ATEBox_InterfaceTyleEnum.IO,
				DropDown = new List<DropDownParamData>()
				{
					new DropDownParamData() { Name = "ANLOG1", Value = "1"},
					new DropDownParamData() { Name = "ANLOG2", Value = "2"},
					new DropDownParamData() { Name = "ANLOG3", Value = "3"},
				},
			});

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "RS232 Start",
				InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
				Command = "ATE_RS232_Start",
			});

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "RS422 Start",
				InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
				Command = "ATE_RS422_Start",
			});

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "RS422 Start",
				InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
				Command = "ATE_RS422_Start",
			});

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "CAN Start",
				InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
				Command = "ATE_CAN_Start",
			});

			deviceData.ParemetersList.Add(new ATEBox_ParamData()
			{
				Name = "CAN",
				InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
				Command = "ATE_RS422_Start",
			});

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(deviceData, settings);
			System.IO.File.WriteAllText(@"C:\Users\smadar\Documents\Stam\Json files\ATABox", sz);

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
				
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to receive value for parameter: " + param.Name, ex);
			}
		}

		#endregion Methods
	}
}
