
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayKeysight;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_PowerSupplyKeysight : DeviceFullData
	{
		public DeviceFullData_PowerSupplyKeysight(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "PowerSupplayKeysightConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new PowerSupplayKeysight_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new TcpConncetViewModel(5025, 21353, 21350, "");
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = new PowerSupplayKeysight_ParamData()
			{
				Name = "Identification",
				Command = "*IDN"
			};

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PowerSupplayKeysight");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as PowerSupplayKeysight_Communicator).Init(
				false,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ConnectionViewModel as TcpConncetViewModel).Port,
				(ConnectionViewModel as TcpConncetViewModel).RxPort,
				(ConnectionViewModel as TcpConncetViewModel).TxPort);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as PowerSupplayKeysight_Communicator).Init(
				true,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ConnectionViewModel as TcpConncetViewModel).Port,
				(ConnectionViewModel as TcpConncetViewModel).RxPort,
				(ConnectionViewModel as TcpConncetViewModel).TxPort);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is TcpConncetViewModel scope))
				return true;

			return scope.IsUdpSimulation;
		}
	}
}
