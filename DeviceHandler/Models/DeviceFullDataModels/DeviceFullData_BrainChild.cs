
using DeviceCommunicators.BrainChild;
using DeviceCommunicators.FieldLogger;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_BrainChild : DeviceFullData
	{
		public DeviceFullData_BrainChild(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "BrainChildSerialConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new BrainChild_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as ModbusRTUConnectViewModel;
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new ModbusRTUConnectViewModel("COM6", 9600, 255, 1, 8, 2);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Channel 1");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"BrainChild");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as BrainChild_Communicator).Init(
				(ConnectionViewModel as ModbusRTUConnectViewModel).IsUdpSimulation,
				(ConnectionViewModel as ModbusRTUConnectViewModel).ComPort,
				(ConnectionViewModel as ModbusRTUConnectViewModel).Baudrate,
				(ConnectionViewModel as ModbusRTUConnectViewModel).ModbusAddress,
				(ConnectionViewModel as ModbusRTUConnectViewModel).StartAddress,
				(ConnectionViewModel as ModbusRTUConnectViewModel).NoOfItems,
				(ConnectionViewModel as ModbusRTUConnectViewModel).SizeOfItems);
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
			if (!(ConnectionViewModel is ModbusRTUConnectViewModel modbusRTUConncet))
				return true;

			return modbusRTUConncet.IsUdpSimulation;
		}
	}
}
