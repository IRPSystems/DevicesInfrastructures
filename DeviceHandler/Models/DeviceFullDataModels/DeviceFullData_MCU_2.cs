
using Communication.Services;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_MCU_2 : DeviceFullData_MCU
	{
		public const uint MCU_2_DeviceID = 0xABAB;

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
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
			if (!(ConnectionViewModel is CanConnectViewModel))
				ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 15523, 15520, MCU_2_DeviceID);
			if ((ConnectionViewModel as CanConnectViewModel).SyncNodeID == 0)
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID = 0xAB;
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 15523, 15220, MCU_2_DeviceID);
		}
	}
}
