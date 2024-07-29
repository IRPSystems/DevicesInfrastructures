
using DeviceCommunicators.Models;
using DeviceCommunicators.ZimmerPowerMeter;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_ZimmerPowerMeter : DeviceFullData
	{
		public DeviceFullData_ZimmerPowerMeter(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "ZimmerPowerMeterConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new ZimmerPowerMeter_Communicator();
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
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Voltage DC");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"ZimmerPowerMeter");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as ZimmerPowerMeter_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as ZimmerPowerMeter_Communicator).Init(
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
