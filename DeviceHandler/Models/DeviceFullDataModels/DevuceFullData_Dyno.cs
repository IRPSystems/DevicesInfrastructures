
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DevuceFullData_Dyno : DeviceFullData
	{
		public DevuceFullData_Dyno(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "DynoCanConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new Dyno_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
			if ((ConnectionViewModel as CanConnectViewModel).SyncNodeID == 0)
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID = 1;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new CanConnectViewModel(250000, 1, 1, 11223, 11220);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Enable");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Dyno");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as Dyno_Communicator).Init(
				(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
				(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID,
				(ConnectionViewModel as CanConnectViewModel).GetSelectedHWId((ConnectionViewModel as CanConnectViewModel).SelectedHwId));
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as Dyno_Communicator).Init(
				(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
				(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID,
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
