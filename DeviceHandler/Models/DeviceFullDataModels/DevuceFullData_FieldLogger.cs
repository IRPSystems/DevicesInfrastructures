﻿
using DeviceCommunicators.FieldLogger;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DevuceFullData_FieldLogger : DeviceFullData
	{
		public DevuceFullData_FieldLogger(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "FieldLoggerSerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new FieldLogger_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new SerialConncetViewModel(9600, "COM1", 15323, 15320);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Channel 1");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"FieldLogger");
		}


		protected override void InitRealCommunicator()
		{
			//(DeviceCommunicator as FieldLogger_Communicator).Init(
			//	(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
			//	(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
			//	(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			//(DeviceCommunicator as FieldLogger_Communicator).Init(
			//	(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
			//	(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
			//	(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
			//	(ConnectionViewModel as SerialConncetViewModel).RxPort,
			//	(ConnectionViewModel as SerialConncetViewModel).TxPort,
			//	(ConnectionViewModel as SerialConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is SerialConncetViewModel serialConncet))
				return true;

			return serialConncet.IsUdpSimulation;
		}
	}
}
