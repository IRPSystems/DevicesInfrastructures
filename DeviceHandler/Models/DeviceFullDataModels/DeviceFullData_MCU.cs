
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_MCU: DeviceFullData
	{
		private bool _isAsync;

		public DeviceFullData_MCU(
			DeviceData deviceData,
			bool isAsync = true) :
			base(deviceData)
		{
			_isAsync = isAsync;
		}

		protected override string GetConnectionFileName()
		{
			return "MCUCanConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new MCU_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
			if (!(ConnectionViewModel is CanConnectViewModel))
				ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 12223, 12220);
			if ((ConnectionViewModel as CanConnectViewModel).SyncNodeID == 0)
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID = 0xAB;

		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new CanConnectViewModel(500000, 0xAB, 0xAA, 12223, 12220);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as MCU_ParamData).Cmd == "flthi");
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as MCU_ParamData).Cmd == "flthi");
			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"MCU");
		}


		protected override void InitRealCommunicator()
		{
			if (string.IsNullOrEmpty((ConnectionViewModel as CanConnectViewModel).SelectedHwId))
				return;

			(DeviceCommunicator as MCU_Communicator).Init(
				(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
				(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID,
				(ConnectionViewModel as CanConnectViewModel).AsyncNodeID,
				_isAsync,
				(ConnectionViewModel as CanConnectViewModel).GetSelectedHWId((ConnectionViewModel as CanConnectViewModel).SelectedHwId));
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as MCU_Communicator).Init(
				(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
				(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
				(ConnectionViewModel as CanConnectViewModel).SyncNodeID,
				(ConnectionViewModel as CanConnectViewModel).AsyncNodeID,
				_isAsync,
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

		protected override void GetRepository()
		{
			ParametersRepository = new MCU_ParametersRepositoryService(DeviceCommunicator);
			ParametersRepository.Name = Device.DeviceType.ToString();
		}
	}
}
