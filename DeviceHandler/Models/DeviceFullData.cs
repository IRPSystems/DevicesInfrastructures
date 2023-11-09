
using Communication.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.BTMTempLogger;
using DeviceCommunicators.Dyno;
using DeviceCommunicators.General;
using DeviceCommunicators.MCU;
using DeviceCommunicators.NI_6002;
using DeviceCommunicators.PowerSupplayBK;
using DeviceCommunicators.PowerSupplayEA;
using DeviceCommunicators.SwitchRelay32;
using DeviceCommunicators.TorqueKistler;
using DeviceCommunicators.YokogawaWT1804E;
using DeviceHandler.Enums;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Entities.Enums;
using Entities.Models;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.IO;
using System.Linq;

namespace DeviceHandler.Models
{
	public class DeviceFullData: ObservableObject
	{
		#region Properties

		public DeviceData Device { get; set; }
		public DeviceCommunicator DeviceCommunicator { get; set; }
		public CheckCommunicationService CheckCommunication { get; set; }
		public IConnectionViewModel ConnectionViewModel { get; set; }
		public ParametersRepositoryService ParametersRepository { get; set; }


		public CommunicationStateEnum CommState { get; set; }
		public string CommErrDescription { get; set; }

		#endregion Properties

		#region Fields

		private bool _isReconnect;

		#endregion Fields


		#region Constructor

		public DeviceFullData(DeviceData device)
		{
			Device = device;
			_isReconnect = false;
		}

		#endregion Constructor


		#region Methodes

		public void Init()
		{
			string fileName = "";
			switch(Device.DeviceType)
			{
				case DeviceTypesEnum.Dyno:
					fileName = "DynoCanConnect.json";
					DeviceCommunicator = new Dyno_Communicator();
					break;
				case DeviceTypesEnum.MCU:
					fileName = "MCUCanConnect.json";
					DeviceCommunicator = new MCU_Communicator();
					break;
				case DeviceTypesEnum.MCU_B2B:
					fileName = "MCU-B2BCanConnect.json";
					DeviceCommunicator = new MCU_Communicator();
					break;
				case DeviceTypesEnum.PowerSupplyBK:
					fileName = "PSBKSerialConnect.json";
					DeviceCommunicator = new PowerSupplayBK_Communicator();
					break;
				case DeviceTypesEnum.PowerSupplyEA:
					fileName = "PSEASerialConnect.json";
					DeviceCommunicator = new PowerSupplayEA_Communicator();
					break;
				case DeviceTypesEnum.BTMTempLogger:
					fileName = "BTMTempLoggerSerialConnect.json";
					DeviceCommunicator = new BTMTempLogger_Communicator();
					break;
				case DeviceTypesEnum.SwitchRelay32:
					fileName = "SwitchRelay32Connect.json";
					DeviceCommunicator = new SwitchCommunicator();
					break;
				case DeviceTypesEnum.NI_6002:
					fileName = "NI_6002Connect.json";
					DeviceCommunicator = new NI6002_Communicator();
					break;
				case DeviceTypesEnum.TorqueKistler:
					fileName = "TorqueKistlerConnect.json";
					DeviceCommunicator = new TorqueKistler_Communicator();
					break;

				case DeviceTypesEnum.Yokogawa_WT1804E:
					fileName = "Yokogawa_WT1804EConnect.json";
					DeviceCommunicator = new YokogawaWT1804E_Communicator();
					break;

				case DeviceTypesEnum.KeySight:
					break;
			}

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "Evva");
			if (Directory.Exists(path) == false)
				return;
			path = Path.Combine(path, fileName);

			if (File.Exists(path))
			{

				string jsonString = File.ReadAllText(path);

				JsonSerializerSettings settings = new JsonSerializerSettings();
				settings.Formatting = Formatting.Indented;
				settings.TypeNameHandling = TypeNameHandling.All;
				//ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as IConnectionViewModel;

				try
				{
					switch (Device.DeviceType)
					{
						case DeviceTypesEnum.Dyno:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
							break;
						case DeviceTypesEnum.MCU:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
							if (!(ConnectionViewModel is CanConnectViewModel))
								ConnectionViewModel = new CanConnectViewModel(500000, 171, 12223, 12220);
							break;
						case DeviceTypesEnum.MCU_B2B:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as CanConnectViewModel;
							if(!(ConnectionViewModel is CanConnectViewModel))
								ConnectionViewModel = new CanConnectViewModel(500000, 171, 19223, 19220);
							break;
						case DeviceTypesEnum.PowerSupplyBK:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
							break;
						case DeviceTypesEnum.PowerSupplyEA:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
							break;
						case DeviceTypesEnum.BTMTempLogger:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
							break;
						case DeviceTypesEnum.SwitchRelay32: 
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as TcpConncetViewModel;
							break;
						case DeviceTypesEnum.NI_6002:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as NI6002ConncetViewModel;
							break;
						case DeviceTypesEnum.TorqueKistler:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as SerialConncetViewModel;
							break;
						case DeviceTypesEnum.Yokogawa_WT1804E:
							ConnectionViewModel = JsonConvert.DeserializeObject(jsonString, settings) as YokogawaWT1804EConncetViewModel;
							break;

						case DeviceTypesEnum.KeySight:
							break;
					}
				}
				catch 
				{ 
					
				}
			}

			if(ConnectionViewModel == null)
			{
				switch (Device.DeviceType)
				{
					case DeviceTypesEnum.Dyno:
						ConnectionViewModel = new CanConnectViewModel(250000, 1, 11223, 11220);
						break;
					case DeviceTypesEnum.MCU:
						ConnectionViewModel = new CanConnectViewModel(500000, 171, 12223, 12220);
						break;
					case DeviceTypesEnum.MCU_B2B:
						ConnectionViewModel = new CanConnectViewModel(500000, 171, 19223, 19220);
						break;
					case DeviceTypesEnum.PowerSupplyBK:
						ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 13323, 13320);
						break;
					case DeviceTypesEnum.PowerSupplyEA:
						ConnectionViewModel = new SerialConncetViewModel(115200, "COM1", 14323, 14320);
						break;
					case DeviceTypesEnum.BTMTempLogger:
						ConnectionViewModel = new SerialConncetViewModel(9600, "COM1", 15323, 15320);
						break;
					case DeviceTypesEnum.SwitchRelay32:
						ConnectionViewModel = new TcpConncetViewModel(4196, 16323, 16320);
						break;
					case DeviceTypesEnum.NI_6002:
						ConnectionViewModel = new NI6002ConncetViewModel();
						break;
					case DeviceTypesEnum.TorqueKistler:
						ConnectionViewModel = new SerialConncetViewModel(57600, "COM1", 17323, 17320);
						break;
					case DeviceTypesEnum.Yokogawa_WT1804E:
						ConnectionViewModel = new YokogawaWT1804EConncetViewModel();
						break;
					case DeviceTypesEnum.KeySight:
						break;
				}
			}

			if (ConnectionViewModel == null)
				return;

			ConnectionViewModel.ConnectEvent += Connect;
			ConnectionViewModel.DisconnectEvent += Disconnect;

			ConnectionViewModel.RefreshProperties();
		//	Connect();

			ParametersRepository = new ParametersRepositoryService(DeviceCommunicator);
			ParametersRepository.Name = Device.DeviceType.ToString();

			BuildCheckConnection();
		}

		private void BuildCheckConnection()
		{
			if (DeviceCommunicator is Dyno_Communicator)
			{
				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "Enable");

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"Dyno");
			}
			else if (DeviceCommunicator is MCU_Communicator)
			{
				if (!(Device is MCU_DeviceData mcu_DeviceData))
					return;

				
				CheckCommunication = new CheckCommunicationService(
					this,
					new MCU_ParamData()
					{
						Cmd = "",
						Name = "Check MCU Comm",
					},
					"MCU");
			}
			else if (DeviceCommunicator is PowerSupplayBK_Communicator)
			{

				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as DeviceParameterData).Name == "MEASure voltage in supply");

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"PSBK");
			}
			else if (DeviceCommunicator is PowerSupplayEA_Communicator)
			{

				PowerSupplayEA_ParamData data = new PowerSupplayEA_ParamData() { Name = "Identity", Cmd = "*IDN?" };

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"PSEA");
			}

			else if (DeviceCommunicator is BTMTempLogger_Communicator)
			{

				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => (p as BTMTempLogger_ParamData).Channel == 1);

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"BTMTempLogger");
			}

			else if (DeviceCommunicator is SwitchCommunicator)
			{

				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "All relay status");

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"SwitchRelay");
			}

			else if (DeviceCommunicator is NI6002_Communicator)
			{

				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Read Anolog input 0");

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"NI_6002");
			}

			else if (DeviceCommunicator is TorqueKistler_Communicator)
			{

				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Torque");

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"TorqueKistler");
			}

			else if (DeviceCommunicator is YokogawaWT1804E_Communicator)
			{

				DeviceParameterData data = Device.ParemetersList.ToList().Find((p) => p.Name == "Controller Efficiency");

				CheckCommunication = new CheckCommunicationService(
					this,
					data,
					"Yokogawa_WT1804E");
			}
		}

		public void InitCheckConnection()
		{	

			if (CheckCommunication == null)
				return;

			CheckCommunication.CommunicationStateReprotEvent += CommStateEventHandler;

			CheckCommunication.Init();
		}




		public void Connect()
		{
			if (ConnectionViewModel == null)
				return;

			if (!IsUDPSimulation())
			{
				if (DeviceCommunicator is Dyno_Communicator)
				{
					
					(DeviceCommunicator as Dyno_Communicator).Init(
						(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
						(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
						(ConnectionViewModel as CanConnectViewModel).NodeID,
						CanPCanService.GetHWId((ConnectionViewModel as CanConnectViewModel).SelectedHwId));
				}
				else if (DeviceCommunicator is MCU_Communicator)
				{
					(DeviceCommunicator as MCU_Communicator).Init(
						(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
						(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
						(ConnectionViewModel as CanConnectViewModel).NodeID,
						CanPCanService.GetHWId((ConnectionViewModel as CanConnectViewModel).SelectedHwId));
				}
				else if (DeviceCommunicator is PowerSupplayBK_Communicator)
				{
					(DeviceCommunicator as PowerSupplayBK_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
				}
				else if (DeviceCommunicator is PowerSupplayEA_Communicator)
				{
					(DeviceCommunicator as PowerSupplayEA_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
				}
				else if (DeviceCommunicator is BTMTempLogger_Communicator)
				{
					(DeviceCommunicator as BTMTempLogger_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
				}
				else if (DeviceCommunicator is SwitchCommunicator)
				{
					(DeviceCommunicator as SwitchCommunicator).Init(
						(ConnectionViewModel as TcpConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as TcpConncetViewModel).Address,
						(ConnectionViewModel as TcpConncetViewModel).Port);
				}
				else if (DeviceCommunicator is NI6002_Communicator)
				{
					(DeviceCommunicator as NI6002_Communicator).Init(
						(ConnectionViewModel as NI6002ConncetViewModel).DeviceName);
				}
				else if (DeviceCommunicator is TorqueKistler_Communicator)
				{
					(DeviceCommunicator as TorqueKistler_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate);
				}
				else if (DeviceCommunicator is YokogawaWT1804E_Communicator)
				{
					(DeviceCommunicator as YokogawaWT1804E_Communicator).Init(
						false,
						(ConnectionViewModel as YokogawaWT1804EConncetViewModel).Address);
				}
			}
			else
			{
				if (DeviceCommunicator is Dyno_Communicator)
				{
					(DeviceCommunicator as Dyno_Communicator).Init(
						(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
						(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
						(ConnectionViewModel as CanConnectViewModel).NodeID,
						0,
						(ConnectionViewModel as CanConnectViewModel).RxPort,
						(ConnectionViewModel as CanConnectViewModel).TxPort,
						(ConnectionViewModel as CanConnectViewModel).Address);
				}
				else if (DeviceCommunicator is MCU_Communicator)
				{
					(DeviceCommunicator as MCU_Communicator).Init(
						(ConnectionViewModel as CanConnectViewModel).SelectedAdapter,
						(ConnectionViewModel as CanConnectViewModel).SelectedBaudrate,
						(ConnectionViewModel as CanConnectViewModel).NodeID,
						0,
						(ConnectionViewModel as CanConnectViewModel).RxPort,
						(ConnectionViewModel as CanConnectViewModel).TxPort,
						(ConnectionViewModel as CanConnectViewModel).Address);
				}
				else if (DeviceCommunicator is PowerSupplayBK_Communicator)
				{
					(DeviceCommunicator as PowerSupplayBK_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
						(ConnectionViewModel as SerialConncetViewModel).RxPort,
						(ConnectionViewModel as SerialConncetViewModel).TxPort,
						(ConnectionViewModel as SerialConncetViewModel).Address);
				}
				else if (DeviceCommunicator is PowerSupplayEA_Communicator)
				{
					(DeviceCommunicator as PowerSupplayEA_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
						(ConnectionViewModel as SerialConncetViewModel).RxPort,
						(ConnectionViewModel as SerialConncetViewModel).TxPort,
						(ConnectionViewModel as SerialConncetViewModel).Address);
				}
				else if (DeviceCommunicator is BTMTempLogger_Communicator)
				{
					(DeviceCommunicator as BTMTempLogger_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
						(ConnectionViewModel as SerialConncetViewModel).RxPort,
						(ConnectionViewModel as SerialConncetViewModel).TxPort,
						(ConnectionViewModel as SerialConncetViewModel).Address);
				}
				else if (DeviceCommunicator is SwitchCommunicator)
				{
					(DeviceCommunicator as SwitchCommunicator).Init(
						(ConnectionViewModel as TcpConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as TcpConncetViewModel).Address,
						(ConnectionViewModel as TcpConncetViewModel).Port);
				}
				else if (DeviceCommunicator is NI6002_Communicator)
				{
					(DeviceCommunicator as NI6002_Communicator).Init(
						(ConnectionViewModel as NI6002ConncetViewModel).DeviceName,
						IsUDPSimulation());
				}
				else if (DeviceCommunicator is TorqueKistler_Communicator)
				{
					(DeviceCommunicator as TorqueKistler_Communicator).Init(
						(ConnectionViewModel as SerialConncetViewModel).IsUdpSimulation,
						(ConnectionViewModel as SerialConncetViewModel).SelectedCOM,
						(ConnectionViewModel as SerialConncetViewModel).SelectedBaudrate,
						(ConnectionViewModel as SerialConncetViewModel).RxPort,
						(ConnectionViewModel as SerialConncetViewModel).TxPort,
						(ConnectionViewModel as SerialConncetViewModel).Address);
				}
				else if (DeviceCommunicator is YokogawaWT1804E_Communicator)
				{
					(DeviceCommunicator as YokogawaWT1804E_Communicator).Init(
						true,
						(ConnectionViewModel as YokogawaWT1804EConncetViewModel).Address);
				}
			}

			if (!DeviceCommunicator.IsInitialized)
			{
				Disconnect();
				return;
			}

			LoggerService.Inforamtion(this, "Ended connecting");

			ConnectionViewModel.IsConnectButtonEnabled = !DeviceCommunicator.IsInitialized;
			ConnectionViewModel.IsDisconnectButtonEnabled = DeviceCommunicator.IsInitialized;

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(ConnectionViewModel, settings);

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "Evva");
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);

			string fileName = "";
			switch (Device.DeviceType)
			{
				case DeviceTypesEnum.Dyno:
					fileName = "DynoCanConnect.json";
					break;
				case DeviceTypesEnum.MCU:
					fileName = "MCUCanConnect.json";
					break;
				case DeviceTypesEnum.MCU_B2B:
					fileName = "MCU-B2BCanConnect.json";
					break;
				case DeviceTypesEnum.PowerSupplyBK:
					fileName = "PSBKSerialConnect.json";
					break;
				case DeviceTypesEnum.PowerSupplyEA:
					fileName = "PSEASerialConnect.json";
					break;
				case DeviceTypesEnum.BTMTempLogger:
					fileName = "BTMTempLoggerSerialConnect.json";
					break;
				case DeviceTypesEnum.SwitchRelay32:
					fileName = "SwitchRelay32Connect.json";
					break;
				case DeviceTypesEnum.NI_6002:
					fileName = "NI_6002Connect.json";
					break;
				case DeviceTypesEnum.TorqueKistler:
					fileName = "TorqueKistlerConnect.json";
					break;
				case DeviceTypesEnum.Yokogawa_WT1804E:
					fileName = "Yokogawa_WT1804EConnect.json";
					break;

				case DeviceTypesEnum.KeySight:
					break;
			}


			path = Path.Combine(path, fileName);
			File.WriteAllText(path, sz);



			ParametersRepository.Init();

			if (_isReconnect)
				InitCheckConnection();

			_isReconnect = true;

			ConnectionEvent?.Invoke();

			
		}

		public void Disconnect()
		{
			LoggerService.Inforamtion(this, "Disconnect");

			if (DeviceCommunicator != null)
				DeviceCommunicator.Dispose();

			if (ConnectionViewModel != null && DeviceCommunicator != null)
			{
				ConnectionViewModel.IsConnectButtonEnabled = !DeviceCommunicator.IsInitialized;
				ConnectionViewModel.IsDisconnectButtonEnabled = DeviceCommunicator.IsInitialized;

				//ConnectionViewModel.ConnectEvent -= Connect;
				//ConnectionViewModel.DisconnectEvent -= Disconnect;
			}

			if (CheckCommunication != null)
			{
				CheckCommunication.Dispose();
				CheckCommunication.CommunicationStateReprotEvent -= CommStateEventHandler;
			}

			if (ParametersRepository != null)
				ParametersRepository.Dispose();

			ConnectionEvent?.Invoke();
		}


		private bool IsUDPSimulation()
		{
			if(ConnectionViewModel == null) 
				return false;

			if(ConnectionViewModel is CanConnectViewModel canConnect)
			{
				if (canConnect.SelectedAdapter == "UDP Simulator")
					return true;

				return false;
			}
			else if (ConnectionViewModel is SerialConncetViewModel serialConncet)
			{
				return serialConncet.IsUdpSimulation;
			}
			else if (ConnectionViewModel is TcpConncetViewModel tcpConncet)
			{
				return tcpConncet.IsUdpSimulation;
			}
			else if (ConnectionViewModel is YokogawaWT1804EConncetViewModel yoko)
			{
				return yoko.IsUdpSimulation;
			}
			else if (ConnectionViewModel is NI6002ConncetViewModel ni)
			{
				return ni.IsUdpSimulation;
			}

			return false;
		}

		private void CommStateEventHandler(CommunicationStateEnum state, string errorDescription)
		{
			try
			{
				CommState = state;
				CommErrDescription = errorDescription;
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set the Dyno communication state", ex);
			}
		}

		#endregion Methodes

		#region Events

		public event Action ConnectionEvent;

		#endregion Events
	}
}
