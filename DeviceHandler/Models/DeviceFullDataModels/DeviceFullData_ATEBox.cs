
using DeviceCommunicators.ATEBox;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayBK;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_ATEBox : DeviceFullData
	{
		public DeviceFullData_ATEBox(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "ATEBoxSerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new ATEBox_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 20323, 20320);
		}

		protected override void ConstructCheckConnection()
		{
			CheckCommunication = new CheckCommunicationService(
				this,
				new ATEBox_ParamData()
				{
					InterfaceType = ATEBox_InterfaceTyleEnum.Commands,
					Command = "Check_Comm 1"
				},
				"ATEBox");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as ATEBox_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as ATEBox_Communicator).Init(
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
