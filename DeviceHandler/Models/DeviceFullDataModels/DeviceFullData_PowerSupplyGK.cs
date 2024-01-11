
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayGK;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_PowerSupplyGK : DeviceFullData
	{
		public DeviceFullData_PowerSupplyGK(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "PSGKSerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new PowerSupplayGK_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as ModbusTCPConnectViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new ModbusTCPConnectViewModel("192.168.2.250", 502);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Voltage");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PSGK");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as PowerSupplayGK_Communicator).Init(
				(ConnectionViewModel as ModbusTCPConnectViewModel).IsUdpSimulation,
				(ConnectionViewModel as ModbusTCPConnectViewModel).IPAddress,
				(ConnectionViewModel as ModbusTCPConnectViewModel).Port);
		}

		protected override void InitSimulationCommunicator()
		{
			
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is ModbusTCPConnectViewModel modbusConncet))
				return true;

			return modbusConncet.IsUdpSimulation;
		}
	}
}
