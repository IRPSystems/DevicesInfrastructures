
using Communication.Services;
using DeviceCommunicators.Dyno;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceCommunicators.PowerSupplayEA;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Newtonsoft.Json;
using Services.Services;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

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
		protected override void ConstructCommunicator(LogLineListService logLineList)
		{
			//DeviceCommunicator = new PowerSupplayEA_Communicator();
		}

		protected override void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
			LogLineListService logLineList)
		{
			SerialAndTCPViewModel connectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialAndTCPViewModel;
			ConstructConnectionViewModel(logLineList);
			if(connectionViewModel!= null) 
			{
				(ConnectionViewModel as SerialAndTCPViewModel).Copy(connectionViewModel);
			}

			ConnectionViewModel.RefreshProperties();

			_eapsCommunicator = new PowerSupplayEA_Communicator(logLineList);
			_eapsModbusTcp = new PowerSupplayEA_ModbusTcp(logLineList);

			(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.ComIdentifier = "";
			(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.DeviceIdentifier = "EA Elektro-Automatik GmbH & Co. KG";
			(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.IdCommand = "*IDN?";


		}

		

		protected override void ConstructConnectionViewModel(LogLineListService logLineList)
		{
			ConnectionViewModel = new SerialAndTCPViewModel(
				115200, "", 14323, 14320,
				502, "", "Serial",
				"", "EA Elektro-Automatik GmbH & Co. KG", "*IDN?");

			_eapsCommunicator = new PowerSupplayEA_Communicator(logLineList);
			_eapsModbusTcp = new PowerSupplayEA_ModbusTcp(logLineList);

			(ConnectionViewModel as SerialAndTCPViewModel).TcpConncetVM.EASearchIPEvent +=
				TcpConncetVM_EASearchIPEvent;
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
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.SelectedBaudrate,
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.DeviceIdentifier);
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
					(ConnectionViewModel as SerialAndTCPViewModel).SerialConncetVM.DeviceIdentifier,
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

		private void TcpConncetVM_EASearchIPEvent()
		{
			Task task = Task.Run(() =>
			{
				EASearchIP();
			});

		}

		private void EASearchIP()
		{ 
			if (!(DeviceCommunicator is PowerSupplayEA_ModbusTcp eaCommunicator))
				return;

			if (!(ConnectionViewModel is SerialAndTCPViewModel serialTcpConncet))
				return;

			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					serialTcpConncet.TcpConncetVM.SearchNoticeVisibility =
						System.Windows.Visibility.Visible;
					serialTcpConncet.IsEnabled = false;
				});
			}

			List<string> ipsList = eaCommunicator.FindEaIPs();



			if (Application.Current != null)
			{
				Application.Current.Dispatcher.Invoke(() =>
				{
					serialTcpConncet.TcpConncetVM.EAIPsList =
					new ObservableCollection<string>(ipsList);
					serialTcpConncet.TcpConncetVM.Address = ipsList[0];

					serialTcpConncet.TcpConncetVM.SearchNoticeVisibility =
						System.Windows.Visibility.Collapsed;
					serialTcpConncet.IsEnabled = true;
				});
			}

			InitCheckConnection();
		}
	}
}
