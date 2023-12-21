
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DevuceFullData_MCU: DeviceFullData
	{
		public DevuceFullData_MCU(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "MCUCanConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new MCU_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
			if (!(ConnectionViewModel is CanConnectViewModel))
				ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 12223, 12220);
			if ((ConnectionViewModel as CanConnectViewModel).SyncNodeID == 0)
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID = 0xAB;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 12223, 12220);
		}

		protected override void ConstructCheckConnection()
		{

			CheckCommunication = new CheckCommunicationService(
				this,
				new MCU_ParamData()
				{
					Cmd = "",
					Name = "Check MCU Comm",
				},
				"MCU");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as MCU_Communicator).Init(
				(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
				(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID,
				(ConnectionViewModel as CanConnectViewModel).AsyncNodeID,
				(ConnectionViewModel as CanConnectViewModel).GetSelectedHWId((ConnectionViewModel as CanConnectViewModel).SelectedHwId));
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as MCU_Communicator).Init(
				(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
				(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID,
				(ConnectionViewModel as CanConnectViewModel).AsyncNodeID,
				0,
				(ConnectionViewModel as CanConnectViewModel).RxPort,
				(ConnectionViewModel as CanConnectViewModel).TxPort,
				(ConnectionViewModel as CanConnectViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is CanConnectViewModel canConnect))
				return true;

			if (canConnect.SelectedAdapter == "UDP Simulator")
				return true;

			return false;
		}
	}
}
