
using DeviceCommunicators.FieldLogger;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_FieldLogger : DeviceFullData
	{
		public DeviceFullData_FieldLogger(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "FieldLoggerSerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			DeviceCommunicator = new FieldLogger_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as ModbusTCPConnectViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new ModbusTCPConnectViewModel("192.168.20.1", 502, 255, 3, 8, 2);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Channel 1");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"FieldLogger");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as FieldLogger_Communicator).Init(
				(ConnectionViewModel as ModbusTCPConnectViewModel).IsUdpSimulation,
				(ConnectionViewModel as ModbusTCPConnectViewModel).IPAddress,
				(ConnectionViewModel as ModbusTCPConnectViewModel).Port,
				(ConnectionViewModel as ModbusTCPConnectViewModel).ModbusAddress,
				(ConnectionViewModel as ModbusTCPConnectViewModel).StartAddress,
				(ConnectionViewModel as ModbusTCPConnectViewModel).NoOfItems,
				(ConnectionViewModel as ModbusTCPConnectViewModel).SizeOfItems);
		}

		protected override void InitSimulationCommunicator()
		{
			//(DeviceCommunicator as FieldLogger_Communicator).Init(
			//	(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
			//	(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
			//	(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
			//	(ConnectionViewModel as SerialConncetViewModel).RxPort,
			//	(ConnectionViewModel as SerialConncetViewModel).TxPort,
			//	(ConnectionViewModel as SerialConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is ModbusTCPConnectViewModel modbusTcpConncet))
				return true;

			return modbusTcpConncet.IsUdpSimulation;
		}
	}
}
