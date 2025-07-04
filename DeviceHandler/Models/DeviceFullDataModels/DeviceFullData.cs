﻿
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.General;
using DeviceCommunicators.Models;
using DeviceHandler.Enums;
using DeviceHandler.Interfaces;
using DeviceHandler.Services;
using DeviceHandler.ViewModels;
using Entities.Enums;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.IO;
using System.Windows;

namespace DeviceHandler.Models.DeviceFullDataModels
{
    public abstract class DeviceFullData : ObservableObject
    {
        #region Properties

        public DeviceData Device { get; set; }
        public virtual DeviceCommunicator DeviceCommunicator { get; set; }
        public CheckCommunicationService CheckCommunication { get; set; }
        public IConnectionViewModel ConnectionViewModel { get; set; }
        public ParametersRepositoryService ParametersRepository { get; set; }


        public CommunicationStateEnum CommState { get; set; }
        public string CommErrDescription { get; set; }

        #endregion Properties

        #region Fields

        private bool _isReconnect;
        private string _appName;

        #endregion Fields


        #region Constructor

        public DeviceFullData(DeviceData device)
        {
            Device = device;
            _isReconnect = false;
        }

        #endregion Constructor


        #region Methodes

        public void Init(
            string appName,
            LogLineListService logLineList)
        {
			_appName = appName;

			LoggerService.Inforamtion(this, "Initiating " + Device.DeviceType);
            string fileName = GetConnectionFileName();
            ConstructCommunicator(logLineList);

			

            LoggerService.Inforamtion(this, "Communicator constructed");

            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            path = Path.Combine(path, appName);
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
                    DeserializeConnectionViewModel(jsonString, settings, logLineList);
                }
                catch
                {

                }
            }

            if (ConnectionViewModel == null)
            {
                LoggerService.Inforamtion(this, "Communication file DON'T exit");
                ConstructConnectionViewModel(logLineList);
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

            GetRepository();

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



        private bool _isFirst = true;
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
                if(Device.DeviceType == DeviceTypesEnum.PowerSupplyEA ) 
                { 
                    if(ConnectionViewModel is SerialAndTCPViewModel serialTcpConncet)
                    {
						serialTcpConncet.TcpConncetVM.AddressTBVisibility = System.Windows.Visibility.Collapsed;
						serialTcpConncet.TcpConncetVM.AddressCBVisibility = System.Windows.Visibility.Visible;
						serialTcpConncet.TcpConncetVM.SearchNoticeVisibility = Visibility.Collapsed;
					}
                }

                if(ConnectionViewModel is SerialConncetViewModel && _isFirst)
                {
					_isFirst = false;
					(ConnectionViewModel as SerialConncetViewModel).TryFindCom();
                    Connect();
                    return;
				}

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
            path = Path.Combine(path, _appName);
            if (Directory.Exists(path) == false)
                Directory.CreateDirectory(path);

			string fileName = GetConnectionFileName();


			path = Path.Combine(path, fileName);
            File.WriteAllText(path, sz);



            ParametersRepository.Init();

            if (_isReconnect)
                InitCheckConnection();

            _isReconnect = true;

            ConnectionEvent?.Invoke();

			_isFirst = true;

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

			_isFirst = true;
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

				case DeviceTypesEnum.Dyno: return new DeviceFullData_Dyno(deviceData);
		        case DeviceTypesEnum.MCU: return new DeviceFullData_MCU(deviceData);
				case DeviceTypesEnum.PowerSupplyBK: return new DeviceFullData_PowerSupplyBK(deviceData);
				case DeviceTypesEnum.PowerSupplyEA: return new DeviceFullData_PowerSupplyEA(deviceData);
				case DeviceTypesEnum.ScopeKeysight: return new DeviceFullData_Scope_KeySight(deviceData);
				case DeviceTypesEnum.TorqueKistler: return new DeviceFullData_TorqueKistler(deviceData);
				case DeviceTypesEnum.MCU_B2B: return new DeviceFullData_MCU_B2B(deviceData);
				case DeviceTypesEnum.BTMTempLogger: return new DeviceFullData_BTMTempLogger(deviceData);
				case DeviceTypesEnum.SwitchRelay32: return new DeviceFullData_SwitchRelay32(deviceData);
				case DeviceTypesEnum.NI_6002: return new DeviceFullData_NI_6002(deviceData);
				case DeviceTypesEnum.Yokogawa_WT1804E: return new DeviceFullData_Yokogawa_WT1804E(deviceData);
				case DeviceTypesEnum.FieldLogger: return new DeviceFullData_FieldLogger(deviceData);
				case DeviceTypesEnum.PowerSupplyGK: return new DeviceFullData_PowerSupplyGK(deviceData);
				case DeviceTypesEnum.BrainChild: return new DeviceFullData_BrainChild(deviceData);
				case DeviceTypesEnum.Dyno3: return new DeviceFullData_Dyno3(deviceData);
				case DeviceTypesEnum.PowerSupplyKeysight: return new DeviceFullData_PowerSupplyKeysight(deviceData);
				case DeviceTypesEnum.ZimmerPowerMeter: return new DeviceFullData_ZimmerPowerMeter(deviceData);
				case DeviceTypesEnum.NI_6002_2: return new DeviceFullData_NI_6002_2(deviceData);
				case DeviceTypesEnum.MCU_2: return new DeviceFullData_MCU_2(deviceData);
                case DeviceTypesEnum.NumatoGPIO: return new DeviceFullData_NumatoGPIO(deviceData);
                case DeviceTypesEnum.Printer_TSC: return new DeviceFullData_Printer_TSC(deviceData);
                case DeviceTypesEnum.RigolM300: return new DeviceFullData_RigolM300(deviceData);
                case DeviceTypesEnum.MX180TP: return new DeviceFullData_MX180TP(deviceData);
                case DeviceTypesEnum.ITM3100: return new DeviceFullData_IT_M3100(deviceData);
                default: return null;
            }
        }

        protected virtual void GetRepository()
        {
			ParametersRepository = new ParametersRepositoryService(DeviceCommunicator, Device.AcquisitionRate);
			ParametersRepository.Name = Device.DeviceType.ToString();
		}

        protected abstract string GetConnectionFileName();
		protected abstract void ConstructCommunicator(LogLineListService logLineList);
        protected abstract void DeserializeConnectionViewModel(
			string jsonString,
			JsonSerializerSettings settings,
            LogLineListService logLineList);
        protected abstract void ConstructConnectionViewModel(LogLineListService logLineList);
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