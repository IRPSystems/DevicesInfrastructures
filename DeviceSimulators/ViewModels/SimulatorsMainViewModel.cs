﻿

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceSimulators.Models;
using Entities.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceSimulators.ViewModels
{
	public class SimulatorsMainViewModel: ObservableObject
	{
		#region Properties

		public DeviceSimulatorsViewModel DeviceSimulators {  get; set; }
		public ObservableCollection<DeviceData> DevicesList { get; set; }

		public bool IsDevicesListEnable { get; set; }
		public DeviceData SelectedDevice { get; set; }

		#endregion Properties

		#region Fields

		private DevicesContainer _devicesContainer;
		private DeviceSimulatorsUserData _deviceSimulatorsUserData;

		#endregion Fields

		#region Constructor

		public SimulatorsMainViewModel()
		{
			LoadCommand = new RelayCommand(Load);
			AddSimulatorCommand = new RelayCommand(AddSimulator);

			IsDevicesListEnable = false;

			_devicesContainer = new DevicesContainer();
			_devicesContainer.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			_devicesContainer.DevicesList = new ObservableCollection<DeviceData>();
			_devicesContainer.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();


			DeviceSimulators = new DeviceSimulatorsViewModel(_devicesContainer);

			_deviceSimulatorsUserData = 
				DeviceSimulatorsUserData.LoadDeviceSimulatorsUserData("DeviceSimulators");
		}

		#endregion Constructor

		#region Methods

		private void AddSimulator()
		{
			DeviceFullData deviceFullData = DeviceFullData.Factory(SelectedDevice);

			deviceFullData.Init("DeviceSimulators");

			_devicesContainer.DevicesFullDataList.Add(deviceFullData);
			_devicesContainer.DevicesList.Add(SelectedDevice);
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(SelectedDevice.DeviceType) == false)
				_devicesContainer.TypeToDevicesFullData.Add(SelectedDevice.DeviceType, deviceFullData);

			DeviceSimulators.UpdateDevices();
		}

		private void Load()
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			if(string.IsNullOrEmpty(_deviceSimulatorsUserData.DevicesFilesDir) == false)
				dialog.InitialDirectory = _deviceSimulatorsUserData.DevicesFilesDir;
			var result = dialog.ShowDialog();
			if (result != System.Windows.Forms.DialogResult.OK)
				return;

			_deviceSimulatorsUserData.DevicesFilesDir = dialog.SelectedPath;

			ReadDevicesFileService readDevicesFile = new ReadDevicesFileService();
			DevicesList = readDevicesFile.ReadAllFiles(
				dialog.SelectedPath,
				dialog.SelectedPath + "\\param_defaults.json",
				dialog.SelectedPath + "\\param_defaults.json",
				dialog.SelectedPath + "\\Dyno Communication.json",
				dialog.SelectedPath + "\\NI_6002.json");
			if(DevicesList != null && DevicesList.Count > 0) 
				IsDevicesListEnable = true;
		}

		#endregion Methods

		#region Commands

		public RelayCommand LoadCommand { get; private set; }
		public RelayCommand AddSimulatorCommand { get; private set; }

		#endregion Commands
	}
}
