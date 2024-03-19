
using DeviceCommunicators.Dyno;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_PowerSupplyEA : DeviceFullData
	{
		public DeviceFullData_PowerSupplyEA(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "PSEASerialConnect.json";
		}
		protected override void ConstructCommunicator()
		{
			//DeviceCommunicator = new PowerSupplayEA_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings)
		{
			ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialAndTCPViewModel;
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new SerialAndTCPViewModel(
				115200, "COM1", 14323, 14320,
				502, "192168.10.38", "Serial");
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Identification");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PSEA");
		}


		protected override void InitRealCommunicator()
		{
			if ((ConnectionViewModel as SerialAndTCPViewModel).SelectedCommType == "Serial")
			{
				(DeviceCommunicator as PowerSupplayEA_Communicator).Init(
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.IsUdpSimulation,
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedCOM,
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedBaudrate);
			}
			else
			{
				(DeviceCommunicator as PowerSupplayEA_ModbusTcp).Init(
					(ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.IsUdpSimulation,
					(ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.Address,
					Device);
			}
		}

		protected override void InitSimulationCommunicator()
		{
			if ((ConnectionViewModel as SerialAndTCPViewModel).SelectedCommType == "Serial")
			{
				(DeviceCommunicator as PowerSupplayEA_Communicator).Init(
				(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.IsUdpSimulation,
				(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedCOM,
				(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedBaudrate,
				(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.RxPort,
				(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.TxPort,
				(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.Address);
			}
		}

		protected override bool IsSumulation()
		{
			if (!(ConnectionViewModel is SerialAndTCPViewModel serialConncet))
				return true;

			return serialConncet.SerialConncetVM.IsUdpSimulation;
		}
	}
}
