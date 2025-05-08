
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceCommunicators.TorqueKistler;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_TorqueKistler : DeviceFullData
	{
		public DeviceFullData_TorqueKistler(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "TorqueKistlerConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new TorqueKistler_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
			(ConnectionViewModel as SerialConncetViewModel).ComIdentifier = "";
			(ConnectionViewModel as SerialConncetViewModel).DeviceIdentifier = "Kistler_4503B";
			(ConnectionViewModel as SerialConncetViewModel).IdCommand = "*IDN?\r";
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new SerialConncetViewModel(
				921600, 
				string.Empty, 
				17323, 
				17320,
				"",
				"Kistler_4503B",
				"*IDN?\r");
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Torque");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"TorqueKistler");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as TorqueKistler_Communicator).Init(
				(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
				(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as TorqueKistler_Communicator).Init(
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
