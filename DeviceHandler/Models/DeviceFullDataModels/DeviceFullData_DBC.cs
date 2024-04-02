
using DeviceCommunicators.ATEBox;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayBK;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_DBC : DeviceFullData
	{
		public DeviceFullData_DBC(
			DeviceData deviceData, DeviceFullData mcuDeviceFullData) :
			base(deviceData)
		{
			if (mcuDeviceFullData == null)
				return;

			DeviceCommunicator = mcuDeviceFullData.DeviceCommunicator;
			ParametersRepository = mcuDeviceFullData.ParametersRepository;
		}

		protected override string GetConnectionFileName()
		{
			return null;
		}

		protected override void ConstructCommunicator()
		{
			
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			
		}

		protected override void ConstructConnectionViewModel()
		{
			
		}

		protected override void ConstructCheckConnection()
		{
			
		}


		protected override void InitRealCommunicator()
		{
			
		}

		protected override void InitSimulationCommunicator()
		{
			
		}

		protected override bool IsSumulation()
		{
			return false;
		}
	}
}
