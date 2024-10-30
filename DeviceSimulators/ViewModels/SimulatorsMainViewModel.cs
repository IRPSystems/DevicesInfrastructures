

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
using System.ComponentModel;
using System.Linq;

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
			RemoveSimulatorCommand = new RelayCommand(RemoveSimulator);
			RemoveAllCommand = new RelayCommand(RemoveAll);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);


			IsDevicesListEnable = false;

			_devicesContainer = new DevicesContainer();
			_devicesContainer.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			_devicesContainer.DevicesList = new ObservableCollection<DeviceData>();
			_devicesContainer.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();


			DeviceSimulators = new DeviceSimulatorsViewModel(_devicesContainer);

			_deviceSimulatorsUserData = 
				DeviceSimulatorsUserData.LoadDeviceSimulatorsUserData("DeviceSimulators");

			if(_deviceSimulatorsUserData.DeviceTypesList.Count > 0)
			{
				Load(_deviceSimulatorsUserData.DevicesFilesDir);

				foreach(DeviceTypesEnum type in _deviceSimulatorsUserData.DeviceTypesList)
				{
					DeviceData deviceData = DevicesList.ToList().Find(x => x.DeviceType == type);
					if (deviceData == null)
						continue;

					SelectedDevice = deviceData;
					AddSimulator();
				}
			}
		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
			_deviceSimulatorsUserData.DeviceTypesList.Clear();
			foreach (DeviceData device in _devicesContainer.DevicesList)
			{
				_deviceSimulatorsUserData.DeviceTypesList.Add(device.DeviceType);
			}

			DeviceSimulatorsUserData.SaveDeviceSimulatorsUserData(
				"DeviceSimulators",
				_deviceSimulatorsUserData);
		}

		#region Add/Remove simulator

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

		private void RemoveSimulator()
		{
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(SelectedDevice.DeviceType) == false)
				return;

			DeviceFullData deviceFullData =
				_devicesContainer.TypeToDevicesFullData[SelectedDevice.DeviceType];
			deviceFullData.Disconnect();

			_devicesContainer.DevicesFullDataList.Remove(deviceFullData);
			_devicesContainer.DevicesList.Remove(SelectedDevice);
			_devicesContainer.TypeToDevicesFullData.Remove(SelectedDevice.DeviceType);

			DeviceSimulators.Remove(deviceFullData.Device.DeviceType);
		}

		private void RemoveAll()
		{
			List<DeviceData> list = new List<DeviceData>(_devicesContainer.DevicesList);
			foreach(DeviceData device in list)
			{
				SelectedDevice = device;
				RemoveSimulator();
			}
		}

		#endregion Add/Remove simulator

		private void Load()
		{
			var dialog = new System.Windows.Forms.FolderBrowserDialog();
			if (string.IsNullOrEmpty(_deviceSimulatorsUserData.DevicesFilesDir) == false)
				dialog.InitialDirectory = _deviceSimulatorsUserData.DevicesFilesDir;
			var result = dialog.ShowDialog();
			if (result != System.Windows.Forms.DialogResult.OK)
				return;

			Load(dialog.SelectedPath);
		}

		private void Load(string path)
		{
			

			_deviceSimulatorsUserData.DevicesFilesDir = path;

			ReadDevicesFileService readDevicesFile = new ReadDevicesFileService();
			DevicesList = readDevicesFile.ReadAllFiles(
				path,
				path + "\\param_defaults.json",
				path + "\\param_defaults.json",
				path + "\\Dyno Communication.json",
				path + "\\NI_6002.json");
			if(DevicesList != null && DevicesList.Count > 0) 
				IsDevicesListEnable = true;
		}

		

		#endregion Methods

		#region Commands

		public RelayCommand LoadCommand { get; private set; }
		public RelayCommand AddSimulatorCommand { get; private set; }
		public RelayCommand RemoveSimulatorCommand { get; private set; }
		public RelayCommand RemoveAllCommand { get; private set; }


		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		#endregion Commands
	}
}
