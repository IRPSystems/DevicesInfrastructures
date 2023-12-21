
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayBK;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullData
{
	public class DevuceFullData_PowerSupplyBK : DeviceFullData
	{
		public DevuceFullData_PowerSupplyBK(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "PSBKSerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new PowerSupplayBK_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 13323, 13320);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "MEASure voltage in supply");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PSBK");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as PowerSupplayBK_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as PowerSupplayBK_Communicator).Init(
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
