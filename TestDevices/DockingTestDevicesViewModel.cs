using Controls.ViewModels;
using DeviceHandler.ViewModels;
using DeviceHandler.Views;
using DeviceSimulators.ViewModels;
using DeviceSimulators.Views;
using Syncfusion.Windows.Tools.Controls;
using System;
using System.IO;
using System.Windows.Controls;

namespace TempLoggerViewer.ViewModels
{
	public class DockingTestDevicesViewModel : DocingBaseViewModel
	{
		#region Fields

		private ContentControl _communicationSettings;
		private ContentControl _deviceSimulatorsViewModel;

		#endregion Fields

		#region Constructor

		public DockingTestDevicesViewModel(
			CommunicationViewModel communicationSettings,
			DeviceSimulatorsViewModel deviceSimulatorsViewModel) :
			base("DockingMain")
		{

			CreateWindows(
				communicationSettings,
				deviceSimulatorsViewModel);
		}

		#endregion Constructor

		#region Methods

		private void CreateWindows(
			CommunicationViewModel communicationSettings,
			DeviceSimulatorsViewModel deviceSimulatorsViewModel)
		{
			DockFill = true;




			_communicationSettings = new ContentControl();
			CommunicationView communication = new CommunicationView() { DataContext = communicationSettings };
			_communicationSettings.Content = communication;
			SetHeader(_communicationSettings, "Communication Settings");
			SetFloatParams(_communicationSettings);
			Children.Add(_communicationSettings);

			_deviceSimulatorsViewModel = new ContentControl();
			DeviceSimulatorsView deviceSimulators = new DeviceSimulatorsView() { DataContext = deviceSimulatorsViewModel };
			_deviceSimulatorsViewModel.Content = deviceSimulators;
			SetHeader(_deviceSimulatorsViewModel, "Device Simulators");
			SetFloatParams(_deviceSimulatorsViewModel);
			Children.Add(_deviceSimulatorsViewModel);

		}


		private void SetFloatParams(ContentControl control)
		{
			SetSizetoContentInDock(control, true);
			SetSizetoContentInFloat(control, true);
			SetState(control, DockState.Hidden);
		}


		public void OpenCommSettings()
		{
			SetState(_communicationSettings, DockState.Float);
		}

		public void OpenDeviceSimulators()
		{
			SetState(_deviceSimulatorsViewModel, DockState.Float);
		}


		public void RestorWindowsLayout()
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, "TestDevices");
			path = Path.Combine(path, "Default.txt");
			if (System.IO.File.Exists(path))
				LoadDockState(path);
		}

		#endregion Methods
	}
}
