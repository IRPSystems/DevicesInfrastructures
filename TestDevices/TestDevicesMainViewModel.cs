
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using DeviceCommunicators.Services;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.ViewModels;
using Entities.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using TempLoggerViewer.ViewModels;
using DeviceSimulators.ViewModels;
using System.Windows.Controls;
using System.Windows;
using System;
using DeviceCommunicators.Enums;
using DeviceHandler.ViewModel;
using Entities.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TestDevices
{
	public class TestDevicesMainViewModel: ObservableObject
	{
		#region Properties

		public DevicesContainer DevicesContainter { get; set; }
		public DockingTestDevicesViewModel Docking { get; set; }
		public DeviceData SelectedDevice { get; set; }

		public ParametersViewModel FullParametersList { get; set; }
		public SelectedParametersListViewModel SelectedParametersList { get; set; }

		public ObservableCollection<int> RecordingRateList { get; set; }
		public int RecordingRate { get; set; }
		public ParamRecordingService ParamRecording { get; set; }
		public bool IsRecordStartEnable { get; set; }
		public bool IsRecordStopEnable { get; set; }
		public string RecordDirectory { get; set; }

		public string Version { get; set; }

		

		#endregion Properties

		#region Fields

		

		#endregion Fields

		#region Constructor

		public TestDevicesMainViewModel()
		{
			Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

			InitDevicesContainter();

			IsRecordStartEnable = true;
			IsRecordStopEnable = false;

			CommunicationViewModel communicationSettings =
				new CommunicationViewModel(DevicesContainter);
			DeviceSimulatorsViewModel deviceSimulatorsViewModel = 
				new DeviceSimulatorsViewModel(DevicesContainter);

			Docking = new DockingTestDevicesViewModel(
				communicationSettings, 
				deviceSimulatorsViewModel);

			ParamRecording = new ParamRecordingService();

			RecordingRateList = new ObservableCollection<int>()
			{
				1, 5, 10, 20
			};

			RecordingRate = 5;

			SelectedParametersList = new SelectedParametersListViewModel(DevicesContainter, "Recordings");
			SelectedParametersList.IsLimitParametersList = true;
			SelectedParametersList.LimitOfParametersList = 40;


			string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			path = Path.Combine(path, "TestDevice");
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);

			path = Path.Combine(path, "Recordings");
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);

			RecordDirectory = path;


			CommunicationSettingsCommand = new RelayCommand(InitCommunicationSettings);
			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);
			DeviceSimulatorCommand = new RelayCommand(DeviceSimulator);

			StartRecordingCommand = new RelayCommand(StartRecording);
			StopRecordingCommand = new RelayCommand(StopRecording);

			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			GetCommand = new RelayCommand<DeviceParameterData>(Get);

			BrowseRecordFileCommand = new RelayCommand(BrowseRecordFile);


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

			DevicesContainer devicesContainter = new DevicesContainer();
			devicesContainter.DevicesFullDataList = new ObservableCollection<DeviceFullData>() { deviceFullData };
			devicesContainter.DevicesList = new ObservableCollection<DeviceData>() { deviceFullData.Device };
			devicesContainter.TypeToDevicesFullData = new Dictionary<DeviceTypesEnum, DeviceFullData>();
			devicesContainter.TypeToDevicesFullData.Add(deviceFullData.Device.DeviceType, deviceFullData);

			DragDropData dragDropData = new DragDropData();
			FullParametersList = new ParametersViewModel(dragDropData, devicesContainter, true);
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
			if(SelectedDevice == null || SelectedParametersList == null || SelectedParametersList.ParametersList.Count == 0 ||
				ParamRecording == null) 
			{
				return;
			}

			

			DeviceFullData deviceFullData = DevicesContainter.TypeToDevicesFullData[SelectedDevice.DeviceType];
			ParamRecording.StartRecording(
				RecordDirectory,
				5,
				SelectedParametersList.ParametersList,
				deviceFullData);



			IsRecordStartEnable = false;
			IsRecordStopEnable = true;
		}

		private void StopRecording()
		{
			if (SelectedDevice == null || ParamRecording == null)
			{
				return;
			}			

			DeviceFullData deviceFullData = DevicesContainter.TypeToDevicesFullData[SelectedDevice.DeviceType];
			ParamRecording.StopRecording(deviceFullData);



			IsRecordStartEnable = true;
			IsRecordStopEnable = false;
		}

		private void BrowseRecordFile()
		{
			CommonOpenFileDialog commonOpenFile = new CommonOpenFileDialog();
			commonOpenFile.IsFolderPicker = true;
			CommonFileDialogResult results = commonOpenFile.ShowDialog();
			if (results != CommonFileDialogResult.Ok)
				return;


			RecordDirectory = commonOpenFile.FileName;
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

		public RelayCommand BrowseRecordFileCommand { get; private set; }



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
