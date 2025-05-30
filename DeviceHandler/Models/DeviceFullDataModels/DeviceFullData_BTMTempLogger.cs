﻿
using DeviceCommunicators.BTMTempLogger;
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_BTMTempLogger : DeviceFullData
	{
		public DeviceFullData_BTMTempLogger(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "BTMTempLoggerSerialConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new BTMTempLogger_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new SerialConncetViewModel(
				9600, 
				string.Empty, 
				15323, 
				15320, 
				"",
				"",
				"");
		}

		protected override void ConstructCheckConnection()
		{
			BTMTempLogger_ParamData data = new BTMTempLogger_ParamData() { Name = "Check Communication", Channel = 1 };

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"BTMTempLogger");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as BTMTempLogger_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as BTMTempLogger_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
				(ConnectionViewModel as SerialConncetViewModel).RxPort,
				(ConnectionViewModel as SerialConncetViewModel).TxPort,
				(ConnectionViewModel as SerialConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is SerialConncetViewModel serialConncet))
				return true;

			return serialConncet.IsUdpSimulation;
		}
	}
}
