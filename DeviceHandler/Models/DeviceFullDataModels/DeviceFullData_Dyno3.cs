
using DeviceCommunicators.Dyno;
using DeviceCommunicators.Dyno3;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_Dyno3 : DeviceFullData
	{
		public DeviceFullData_Dyno3(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "Dyno3CanConnect.json";
		}
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			DeviceCommunicator = new Dyno3_Comunicator(logLineList);
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
			ConnectionViewModel = new TcpConncetViewModel(200, 26323, 26320);
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Speed");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"Dyno3");
		}


		protected override void InitRealCommunicator()
		{
			(DeviceCommunicator as Dyno3_Comunicator).Init(
				(ConnectionViewModel as TcpConncetViewModel).Address);
		}

		protected override void InitSimulationCommunicator()
		{
			(DeviceCommunicator as Dyno3_Comunicator).Init(
				(ConnectionViewModel as TcpConncetViewModel).Address);
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is TcpConncetViewModel tcpCon))
				return true;

			return tcpCon.IsUdpSimulation;
		}
	}
}
