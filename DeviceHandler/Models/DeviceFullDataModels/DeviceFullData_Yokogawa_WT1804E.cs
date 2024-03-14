﻿
using DeviceCommunicators.Models;
using DeviceCommunicators.YokogawaWT1804E;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_Yokogawa_WT1804E : DeviceFullData
	{
		public DeviceFullData_Yokogawa_WT1804E(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "Yokogawa_WT1804EConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new YokogawaWT1804E_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as IPAddressOnlyConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new IPAddressOnlyConncetViewModel();
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Controller Efficiency");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Yokogawa_WT1804E");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as YokogawaWT1804E_Communicator).Init(
				false,
				(ConnectionViewModel as IPAddressOnlyConncetViewModel).Address);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as YokogawaWT1804E_Communicator).Init(
				true,
				(ConnectionViewModel as IPAddressOnlyConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is IPAddressOnlyConncetViewModel yoko))
				return true;

			return yoko.IsUdpSimulation;
		}
	}
}
