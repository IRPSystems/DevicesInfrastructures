
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceCommunicators.TorqueKistler;
using DeviceCommunicators.YokogawaWT1804E;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_Yokogawa_WT1804E : DeviceFullData
	{
		public DeviceFullData_Yokogawa_WT1804E(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "Yokogawa_WT1804EConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new YokogawaWT1804E_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as YokogawaWT1804EConncetViewModel;
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new YokogawaWT1804EConncetViewModel();
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Controller Efficiency");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Yokogawa_WT1804E");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as YokogawaWT1804E_Communicator).Init(
				false,
				(ConnectionViewModel as YokogawaWT1804EConncetViewModel).Address);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as YokogawaWT1804E_Communicator).Init(
				true,
				(ConnectionViewModel as YokogawaWT1804EConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is YokogawaWT1804EConncetViewModel yoko))
				return true;

			return yoko.IsUdpSimulation;
		}
	}
}
