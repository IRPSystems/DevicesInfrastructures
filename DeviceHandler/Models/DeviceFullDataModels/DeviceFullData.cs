
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using Entities.Enums;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.IO;

namespace DeviceHandler.Models.DeviceFullDataModels
{
    public abstract class DeviceFullData : ObservableObject
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
            LoggerService.Inforamtion(this, "Initiating " + Device.DeviceType);
            string fileName = GetConnectionFileName();
            ConstructCommunicator();

			

            LoggerService.Inforamtion(this, "Communicator constructed");

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, "Evva");
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);
            path = Path.Combine(path, fileName);

            if (File.Exists(path))
            {
                LoggerService.Inforamtion(this, "Communication file exit");

                string jsonString = File.ReadAllText(path);

                JsonSerializerSettings settings = new JsonSerializerSettings();
                settings.Formatting = Formatting.Indented;
                settings.TypeNameHandling = TypeNameHandling.All;
                
                try
                {
                    DeserializeConnectionViewModel(jsonString, settings);
                }
                catch
                {

                }
            }

            if (ConnectionViewModel == null)
            {
                LoggerService.Inforamtion(this, "Communication file DON'T exit");
                ConstructConnectionViewModel();
            }

            if (ConnectionViewModel == null)
            {
                LoggerService.Inforamtion(this, "ConnectionViewModel = null");
                return;
            }

            LoggerService.Inforamtion(this, "ConnectionViewModel initiated");

            ConnectionViewModel.ConnectEvent += Connect;
            ConnectionViewModel.DisconnectEvent += Disconnect;

            ConnectionViewModel.RefreshProperties();

            ParametersRepository = new ParametersRepositoryService(DeviceCommunicator);
            ParametersRepository.Name = Device.DeviceType.ToString();

            BuildCheckConnection();
        }

        private void BuildCheckConnection()
        {
            ConstructCheckConnection();

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
                InitRealCommunicator();
            }
            else
            {
                InitSimulationCommunicator();
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
            return IsSumulation();
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


        public static DeviceFullData Factory(DeviceData deviceData)
        {
            switch(deviceData.DeviceType)
            {

				case DeviceTypesEnum.Dyno: return new DevuceFullData_Dyno(deviceData);
		        case DeviceTypesEnum.MCU: return new DevuceFullData_MCU(deviceData);
				case DeviceTypesEnum.PowerSupplyBK: return new DevuceFullData_PowerSupplyBK(deviceData);
				case DeviceTypesEnum.PowerSupplyEA: return new DevuceFullData_PowerSupplyEA(deviceData);
				//case DeviceTypesEnum.KeySight: return new DevuceFullData_KeySight(deviceData);
				case DeviceTypesEnum.TorqueKistler: return new DevuceFullData_TorqueKistler(deviceData);
				case DeviceTypesEnum.MCU_B2B: return new DevuceFullData_MCU_B2B(deviceData);
				case DeviceTypesEnum.BTMTempLogger: return new DevuceFullData_BTMTempLogger(deviceData);
				case DeviceTypesEnum.SwitchRelay32: return new DevuceFullData_SwitchRelay32(deviceData);
				case DeviceTypesEnum.NI_6002: return new DevuceFullData_NI_6002(deviceData);
				case DeviceTypesEnum.Yokogawa_WT1804E: return new DevuceFullData_Yokogawa_WT1804E(deviceData);
				default: return null;
            }
        }


        protected abstract string GetConnectionFileName();
		protected abstract void ConstructCommunicator();
        protected abstract void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings);
        protected abstract void ConstructConnectionViewModel();
		protected abstract void ConstructCheckConnection();
        protected abstract void InitRealCommunicator();
		protected abstract void InitSimulationCommunicator();
        protected abstract bool IsSumulation();


		#endregion Methodes

		#region Events

		public event Action ConnectionEvent;

        #endregion Events
    }
}