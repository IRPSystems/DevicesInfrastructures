﻿
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_MCU_2 : DeviceFullData_MCU
	{
		public DeviceFullData_MCU_2(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "MCU-2CanConnect.json";
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
			if (!(ConnectionViewModel is CanConnectViewModel))
				ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 15523, 15520);
			if ((ConnectionViewModel as CanConnectViewModel).SyncNodeID == 0)
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID = 0xAB;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 15523, 15220);
		}
	}
}
