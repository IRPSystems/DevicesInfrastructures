
using DeviceCommunicators.Models;
using DeviceCommunicators.Scope_KeySight;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_Scope_KeySight : DeviceFullData
	{
		public DeviceFullData_Scope_KeySight(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "Scope_KeySightConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new Scope_KeySight_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new TcpConncetViewModel(5025, "192.168.10.148", 21323, 21320);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Identification");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Scope_KeySight");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as Scope_KeySight_Communicator).Init(
				false,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ConnectionViewModel as TcpConncetViewModel).Port);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as Scope_KeySight_Communicator).Init(
				true,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ConnectionViewModel as TcpConncetViewModel).Port);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is TcpConncetViewModel scope))
				return true;

			return scope.IsUdpSimulation;
		}
	}
}
