
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DevuceFullData_MCU_B2B : DevuceFullData_MCU
	{
		public DevuceFullData_MCU_B2B(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "MCU-B2BCanConnect.json";
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
			if (!(ConnectionViewModel is CanConnectViewModel))
				ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 19223, 19220);
			if ((ConnectionViewModel as CanConnectViewModel).SyncNodeID == 0)
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID = 0xAB;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 19223, 19220);
		}
	}
}
