
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayGK;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_PowerSupplyGK : DeviceFullData
	{
		public DeviceFullData_PowerSupplyGK(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "PSGKSerialConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new PowerSupplayGK_Communicator(logLineList);
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
			if (ConnectionViewModel == null)
				ConstructConnectionViewModel(logLineList);
		}

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new TcpConncetViewModel(502, 255, 255, "192.168.2.250");
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find(
				(p) => (p as DeviceParameterData).Name == "Measured Voltage");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PSGK");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as PowerSupplayGK_Communicator).Init(
				(ConnectionViewModel as TcpConncetViewModel).IsUdpSimulation,
				(ConnectionViewModel as TcpConncetViewModel).Address,
				(ushort)(ConnectionViewModel as TcpConncetViewModel).Port);
		}

		protected override void InitSimulationCommunicator()
		{
			
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is TcpConncetViewModel modbusConncet))
				return true;

			return modbusConncet.IsUdpSimulation;
		}
	}
}
