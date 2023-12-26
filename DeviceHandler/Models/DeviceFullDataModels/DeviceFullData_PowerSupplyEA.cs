
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_PowerSupplyEA : DeviceFullData
	{
		public DeviceFullData_PowerSupplyEA(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "PSEASerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new PowerSupplayEA_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 14323, 14320);
		}

		protected override void ConstructCheckConnection()
		{
			PowerSupplayEA_ParamData data = new PowerSupplayEA_ParamData() { Name = "Identity", Cmd = "*IDN?" };

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PSEA");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as PowerSupplayEA_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as PowerSupplayEA_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
				(ConnectionViewModel as SerialConncetViewModel).RxPort,
				(ConnectionViewModel as SerialConncetViewModel).TxPort,
				(ConnectionViewModel as SerialConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is SerialConncetViewModel serialConncet))
				return true;

			return serialConncet.IsUdpSimulation;
		}
	}
}
