﻿
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.NI_6002;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceCommunicators.TSCPrinter;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_Printer_TSC : DeviceFullData
	{
		public DeviceFullData_Printer_TSC(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			//TODO Do i need json?
			return "Printer_TSCConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new PrinterTSC_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as PrinterTSCConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new PrinterTSCConncetViewModel();
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => ((PrinterTSC_ParamData)p).Name == "Comm Chcek");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Printer_TSC");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as PrinterTSC_Communicator).Init(true,
						(ConnectionViewModel as PrinterTSCConncetViewModel).DeviceName);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as PrinterTSC_Communicator).Init(true,
                        (ConnectionViewModel as PrinterTSCConncetViewModel).DeviceName);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is PrinterTSCConncetViewModel tsc))
				return true;

			return tsc.IsUdpSimulation;
		}
	}
}
