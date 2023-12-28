
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using Entities.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using TempLoggerViewer.ViewModels;
using DeviceSimulators.ViewModels;
using System.Windows.Controls;
using System.Windows;
using System;
using DeviceCommunicators.Enums;

namespace TestDevices
{
	public class TestDevicesMainViewModel
	{
		#region Properties

		public DevicesContainer DevicesContainter { get; set; }
		public DockingTestDevicesViewModel Docking { get; set; }
		public DeviceData SelectedDevice { get; set; }

		public string Version { get; set; }

		#endregion Properties

		#region Fields

		#endregion Fields

		#region Constructor

		public TestDevicesMainViewModel()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			InitDevicesContainter();

			CommunicationViewModel communicationSettings =
				new CommunicationViewModel(DevicesContainter);
			DeviceSimulatorsViewModel deviceSimulatorsViewModel = 
				new DeviceSimulatorsViewModel(DevicesContainter);

			Docking = new DockingTestDevicesViewModel(
				communicationSettings, 
				deviceSimulatorsViewModel);

			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			DeviceSimulatorCommand = new RelayCommand(DeviceSimulator);

			StartRecordingCommand = new RelayCommand(StartRecording);
			StopRecordingCommand = new RelayCommand(StopRecording);

			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			GetCommand = new RelayCommand<DeviceParameterData>(Get);


		}

		#endregion Constructor

		#region Methods

		private void Closing(CancelEventArgs e)
		{
		}

		private void InitDevicesContainter()
		{
			DevicesContainter = new DevicesContainer();
			DevicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>();
			DevicesContainter.DevicesList = new ObservableCollection<DeviceData>();
			DevicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();

			string path = Directory.GetCurrentDirectory();
			path = Path.Combine(path, "Device Communications");

			ReadDevicesFileService readDevicesFile = new ReadDevicesFileService();
			ObservableCollection<DeviceData> deviceList = readDevicesFile.ReadAllFiles(
				path,
				Path.Combine(path, "param_defaults.json"),
				Path.Combine(path, "param_defaults.json"),
				Path.Combine(path, "Dyno Communication.json"),
				Path.Combine(path, "NI_6002.json"));

			foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			{
				device.Disconnect();
			}

			DevicesContainter.DevicesFullDataList.Clear();
			DevicesContainter.DevicesList.Clear();
			DevicesContainter.TypeToDevicesFullData.Clear();


			foreach (DeviceData device in deviceList)
			{
				DeviceFullData deviceFullData = DeviceFullData.Factory(device);

				deviceFullData.Init();

				DevicesContainter.DevicesFullDataList.Add(deviceFullData);
				DevicesContainter.DevicesList.Add(device);
				if (DevicesContainter.TypeToDevicesFullData.ContainsKey(device.DeviceType) == false)
					DevicesContainter.TypeToDevicesFullData.Add(device.DeviceType, deviceFullData);
			}

			//foreach (DeviceFullData device in DevicesContainter.DevicesFullDataList)
			//{
			//	device.Connect();
			//	device.InitCheckConnection();
			//}
		}

		private void InitCommunicationSettings()
		{
			Docking.OpenCommSettings();
		}

		private void DeviceSimulator()
		{
			Docking.OpenDeviceSimulators();
		}

		private void SearchText_TextChanged(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (!(textBox.DataContext is DeviceData deviceData))
				return;

			foreach (DeviceParameterData param in deviceData.ParemetersList)
			{
				if (param.Name.ToLower().Contains(textBox.Text.ToLower()))
					param.Visibility = Visibility.Visible;
				else
					param.Visibility = Visibility.Collapsed;
			}
		}

		private void SelectedDevicechanged(SelectionChangedEventArgs e)
		{
			
			
			DeviceFullData deviceFullData = 
				DevicesContainter.TypeToDevicesFullData[SelectedDevice.DeviceType];
			deviceFullData.Connect();
			deviceFullData.InitCheckConnection();
		}

		private void Set(DeviceParameterData deviceParam)
		{
			DeviceFullData deviceFullData = DevicesContainter.TypeToDevicesFullData[deviceParam.DeviceType];
			deviceFullData.DeviceCommunicator.SetParamValue(deviceParam, Convert.ToDouble(deviceParam.Value), MessageCallback);
		}

		private void Get(DeviceParameterData deviceParam)
		{
			DeviceFullData deviceFullData = DevicesContainter.TypeToDevicesFullData[deviceParam.DeviceType];
			deviceFullData.DeviceCommunicator.GetParamValue(deviceParam, MessageCallback);
		}

		private void MessageCallback(DeviceParameterData param, CommunicatorResultEnum result, string resultDescription)
		{
			if (result != CommunicatorResultEnum.OK)
			{
				MessageBox.Show("Failed to get response\r\n" + resultDescription);
			}
		}

		private void StartRecording()
		{
		
		}

		private void StopRecording()
		{
			
		}



		#endregion Methods

		#region Commands

		public RelayCommand CommunicationSettingsCommand { get; private set; }
		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }
		public RelayCommand DeviceSimulatorCommand { get; private set; }

		public RelayCommand StartRecordingCommand { get; private set; }
		public RelayCommand StopRecordingCommand { get; private set; }

		public RelayCommand<DeviceParameterData> SetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> GetCommand { get; private set; }



		private RelayCommand<TextChangedEventArgs> _SearchText_TextChangedCommand;
		public RelayCommand<TextChangedEventArgs> SearchText_TextChangedCommand
		{
			get
			{
				return _SearchText_TextChangedCommand ?? (_SearchText_TextChangedCommand =
					new RelayCommand<TextChangedEventArgs>(SearchText_TextChanged));
			}
		}

		private RelayCommand<SelectionChangedEventArgs> _SelectedDevicechangedCommand;
		public RelayCommand<SelectionChangedEventArgs> SelectedDevicechangedCommand
		{
			get
			{
				return _SelectedDevicechangedCommand ?? (_SelectedDevicechangedCommand =
					new RelayCommand<SelectionChangedEventArgs>(SelectedDevicechanged));
			}
		}

		#endregion Commands
	}
}
