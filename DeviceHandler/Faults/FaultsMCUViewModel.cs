
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using Services.Services;
using DeviceHandler.Enums;
using System.Windows;
using Entities.Enums;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.Models;

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

		public FaultsMCUHalfViewModel FaultsMCUHalf_LSB { get; set; }
		public FaultsMCUHalfViewModel FaultsMCUHalf_MSB { get; set; }

		public bool IsShowFaultsOnly 
		{
			get => _isShowFaultsOnly;
			set
			{
				_isShowFaultsOnly = value;
				FaultsMCUHalf_LSB.IsShowFaultsOnly = value;
				FaultsMCUHalf_MSB.IsShowFaultsOnly = value;
			}
		}

		#endregion Properties

		#region Fields

		private bool _isShowFaultsOnly;
		public bool IsLoaded;

		private bool? _isErrorExist_LSB;
		private bool? _isErrorExist_MSB;

		private bool _isWindowOpen;

		private DevicesContainer _devicesContainer;

		#endregion Fields

		#region Constructor

		public FaultsMCUViewModel(DevicesContainer devicesContainer)
		{
			_devicesContainer = devicesContainer;

			FaultsMCUHalf_LSB = new FaultsMCUHalfViewModel("LSB", _devicesContainer);
			FaultsMCUHalf_MSB = new FaultsMCUHalfViewModel("MSB", _devicesContainer);


			FaultsMCUHalf_LSB.FaultEvent += FaultsMCUHalf_FaultEvent_LSB;
			FaultsMCUHalf_MSB.FaultEvent += FaultsMCUHalf_FaultEvent_MSB;

			IsLoaded = false;

			LoadedCommand = new RelayCommand(Loaded);
			ClosingCommand = new RelayCommand(Closing);

			_isWindowOpen = false;

			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU))
			{
				DeviceFullData mcuFullData = _devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

				mcuFullData.CheckCommunication.CommunicationStateReprotEvent += CheckCommunication_CommunicationStateReprotEvent;
			}

		}

		



		#endregion Constructor

		#region Methods

		public void Dispose()
		{
			FaultsMCUHalf_LSB.Dispose();
			FaultsMCUHalf_MSB.Dispose();
			IsLoaded = false;
		}


		public void Start()
		{
			if (!_isWindowOpen)
				return;

			FaultsMCUHalf_LSB.Start();
			FaultsMCUHalf_MSB.Start();
		}

		public void Stop()
		{
			FaultsMCUHalf_LSB.Stop();
			FaultsMCUHalf_MSB.Stop();
		}

		private void FaultsMCUHalf_FaultEvent_LSB(bool? isFalutExist)
		{
			_isErrorExist_LSB = isFalutExist;
		}

		private void FaultsMCUHalf_FaultEvent_MSB(bool? isFalutExist)
		{
			_isErrorExist_MSB = isFalutExist;
		}

		private void Loaded()
		{
			_isWindowOpen = true;
			Start();
		}

		private void Closing()
		{
			_isWindowOpen = false;
			Stop();
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


		#endregion Methods

		#region Commands

		public RelayCommand LoadedCommand { get; private set; }
		public RelayCommand ClosingCommand { get; private set; }

		#endregion Commands

	}


}
