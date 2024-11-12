
using DeviceCommunicators.Models;
using DeviceCommunicators.ZimmerPowerMeter;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
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
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new ZimmerPowerMeter_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 14323, 14320);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = new ZimmerPowerMeter_ParamData()
			{
				Command = "*IDN",
				Name = "Identification"
			};

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
