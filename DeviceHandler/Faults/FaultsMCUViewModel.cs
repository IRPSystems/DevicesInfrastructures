
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Services.Services;
using DeviceHandler.Enums;
using System.Windows;
using Entities.Enums;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.Models;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using DeviceCommunicators.MCU;
using System.Linq;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Messaging;

namespace DeviceHandler.Faults
{
	public class FaultsMCUViewModel: ObservableObject
	{
		public enum ReadStatEnum
		{
			Number,
			Name,
			Opcode,
			Description,
		}

		#region Properties

		public ObservableCollection<FaultsMCUHalfViewModel> FaultsMCUHalfList { get; set; }

		public bool IsShowFaultsOnly 
		{
			get => _isShowFaultsOnly;
			set
			{
				_isShowFaultsOnly = value;

				foreach(FaultsMCUHalfViewModel vm in FaultsMCUHalfList)
					vm.IsShowFaultsOnly = value;
			}
		}

		#endregion Properties

		#region Fields

		private bool _isShowFaultsOnly;
		public bool IsLoaded;


		private bool _isWindowOpen;

		private DevicesContainer _devicesContainer;

		#endregion Fields

		#region Constructor

		public FaultsMCUViewModel(DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			

			IsLoaded = false;

			LoadedCommand = new RelayCommand(Loaded);
			ClosingCommand = new RelayCommand(Closing);

			_isWindowOpen = false;

			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
			{
				DeviceFullData mcuFullData = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

				mcuFullData.CheckCommunication.CommunicationStateReprotEvent += CheckCommunication_CommunicationStateReprotEvent;

				InitFaultsMCUHalfList();
			}

			WeakReferenceMessenger.Default.Register<SETTINGS_UPDATEDMessage>(
				this, new MessageHandler<object, SETTINGS_UPDATEDMessage>(SETTINGS_UPDATEDMessageHandler));

		}

		#endregion Constructor

		#region Methods

		public void Dispose()
		{
			foreach (FaultsMCUHalfViewModel vm in FaultsMCUHalfList)
				vm.Dispose();
			IsLoaded = false;
		}


		public void Start()
		{
			if (!_isWindowOpen)
				return;

			if (FaultsMCUHalfList == null)
				return;

			foreach (FaultsMCUHalfViewModel vm in FaultsMCUHalfList)
				vm.Start();
		}

		public void Stop()
		{
			if (FaultsMCUHalfList == null)
				return;

			foreach (FaultsMCUHalfViewModel vm in FaultsMCUHalfList)
				vm.Stop();
		}


		public void Loaded()
		{
			_isWindowOpen = true;
			Start();
		}

		public void Closing()
		{
			_isWindowOpen = false;
			Stop();
		}

		private void InitFaultsMCUHalfList()
		{
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			DeviceFullData mcuFullData = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

			ParamGroup paramGroup =
				((MCU_DeviceData)mcuFullData.Device).MCU_GroupList.ToList().Find((g) =>
				 g != null &&
				 g.GroupName != null &&
				 g.GroupName == "Monitor");

			if (paramGroup == null)
				return;

			List<MCU_ParamData> paramDataList =
				paramGroup.ParamList.ToList().Where((p) => p.Cmd.Contains("fltv")).ToList();
			if (paramDataList == null || paramDataList.Count == 0)
			{
				LoggerService.Error(this, "Failed to find the faults parameter in the params list");
				return;
			}

			FaultsMCUHalfList = new ObservableCollection<FaultsMCUHalfViewModel>();
			foreach (var paramData in paramDataList)
			{
				FaultsMCUHalfViewModel faultsMCU = new FaultsMCUHalfViewModel(
					paramData,
					_devicesContainer);
				FaultsMCUHalfList.Add(faultsMCU);
			}

		}

		private void CheckCommunication_CommunicationStateReprotEvent(
			CommunicationStateEnum state, 
			string errorDescription)
		{
			if (Application.Current == null)
				return;

			

			try
			{

				switch (state)
				{
					case CommunicationStateEnum.Connected:
						Application.Current.Dispatcher.Invoke(() =>
						{
							Start();
						});

						break;

					case CommunicationStateEnum.Initiated:


						break;

					case CommunicationStateEnum.None:
					case CommunicationStateEnum.Disconnected:

						Application.Current.Dispatcher.Invoke(() =>
						{
							Stop();
						});

						break;
				}

			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to set connection status", ex);
			}
		}

		public void SETTINGS_UPDATEDMessageHandler(object sender, SETTINGS_UPDATEDMessage e)
		{
			Stop();
			InitFaultsMCUHalfList();
			Start();
		}


		#endregion Methods

		#region Commands

		public RelayCommand LoadedCommand { get; private set; }
		public RelayCommand ClosingCommand { get; private set; }

		#endregion Commands

	}


}
