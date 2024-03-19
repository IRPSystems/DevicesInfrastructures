
using DeviceCommunicators.Dyno;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using System.Linq;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_PowerSupplyEA : DeviceFullData
	{

		public override DeviceCommunicator DeviceCommunicator 
		{ 
			get
			{
				if (ConnectionViewModel == null)
					return null;

				if ((ConnectionViewModel as SerialAndTCPViewModel).SelectedCommType == "Serial")
					return _eapsCommunicator;
				else
					return _eapsModbusTcp;
			}

		}

		private PowerSupplayEA_Communicator _eapsCommunicator;
		private PowerSupplayEA_ModbusTcp _eapsModbusTcp;

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
			SerialAndTCPViewModel connectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialAndTCPViewModel;
			ConstructConnectionViewModel();
			if(connectionViewModel!= null) 
			{
				(ConnectionViewModel as SerialAndTCPViewModel).Copy(connectionViewModel);
			}

			ConnectionViewModel.RefreshProperties();

			_eapsCommunicator = new PowerSupplayEA_Communicator();
			_eapsModbusTcp = new PowerSupplayEA_ModbusTcp();
		}

		protected override void ConstructConnectionViewModel()
		{
			ConnectionViewModel = new SerialAndTCPViewModel(
				115200, "COM1", 14323, 14320,
				502, "192.168.10.28", "Serial");

			_eapsCommunicator = new PowerSupplayEA_Communicator();
			_eapsModbusTcp = new PowerSupplayEA_ModbusTcp();
		}

		protected override void ConstructCheckConnection()
		{
			DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Max Voltage");

			CheckCommunication = new CheckCommunicationService(
				this,
				data,
				"PSEA");
		}


		protected override void InitRealCommunicator()
		{
			if ((ConnectionViewModel as SerialAndTCPViewModel).SelectedCommType == "Serial")
			{
				_eapsCommunicator.Init(
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.IsUdpSimulation,
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedCOM,
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedBaudrate);
			}
			else
			{
				_eapsModbusTcp.Init(
					(ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.IsUdpSimulation,
					(ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.Address,
					Device);
			}
		}

		protected override void InitSimulationCommunicator()
		{
			if ((ConnectionViewModel as SerialAndTCPViewModel).SelectedCommType == "Serial")
			{
				_eapsCommunicator.Init(
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
