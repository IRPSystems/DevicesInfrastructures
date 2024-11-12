
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_SwitchRelay32 : DeviceFullData
	{
		public DeviceFullData_SwitchRelay32(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "SwitchRelay32Connect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new SwitchCommunicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new TcpConncetViewModel(4196, 16323, 16320);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "All relay status");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"SwitchRelay");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as SwitchCommunicator).Init(
				(ConnectionViewModel as TcpConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ConnectionViewModel as TcpConncetViewModel).Port);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as SwitchCommunicator).Init(
				(ConnectionViewModel as TcpConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ConnectionViewModel as TcpConncetViewModel).Port);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is TcpConncetViewModel tcpConncet))
				return true;

			return tcpConncet.IsUdpSimulation;
		}
	}
}
