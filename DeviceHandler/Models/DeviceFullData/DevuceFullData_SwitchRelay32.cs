
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullData
{
	public class DevuceFullData_TorqueKistler : DeviceFullData
	{
		public DevuceFullData_TorqueKistler(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "SwitchRelay32Connect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new SwitchCommunicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
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
