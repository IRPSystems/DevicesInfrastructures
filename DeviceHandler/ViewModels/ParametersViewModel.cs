
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DeviceCommunicators.EvvaDevice;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Entities.Models;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeviceHandler.ViewModel
{
    public class ParametersViewModel : ObservableObject
	{
		#region Properties

		public ObservableCollection<DeviceData> DevicesList { get; set; }

		public Syncfusion.UI.Xaml.TreeView.SelectionMode SourceSelectionMode { get; set; }

		#endregion Properties

		#region Fields

		public const string DragDropFormat = "DeviceParameter";

		private DragDropData _designDragDropData;
		private DevicesContainer _devicesContainer;

		private bool _isHideEvvaDevice;

		#endregion Fields


		#region Constructor

		public ParametersViewModel(
			DragDropData designDragDropData,
			DevicesContainer devicesContainer,
			bool isMultipleSourceSelection,
			bool isHideEvvaDevice = true)
		{
			_designDragDropData = designDragDropData;
			_devicesContainer = devicesContainer;
			_isHideEvvaDevice = isHideEvvaDevice;

			if(isMultipleSourceSelection)
				SourceSelectionMode = Syncfusion.UI.Xaml.TreeView.SelectionMode.Multiple;
			else
				SourceSelectionMode = Syncfusion.UI.Xaml.TreeView.SelectionMode.Single;

			ExpandAllCommand = new RelayCommand(ExpandAll);
			CollapseAllCommand = new RelayCommand(CollapseAll);


			BuildDevicesList();



			SetSearchedTest("");



			WeakReferenceMessenger.Default.Register<SETTINGS_UPDATEDMessage>(
				this, new MessageHandler<object, SETTINGS_UPDATEDMessage>(SETTINGS_UPDATEDMessageHandler));
			WeakReferenceMessenger.Default.Register<SETUP_UPDATEDMessage>(
				this, new MessageHandler<object, SETUP_UPDATEDMessage>(SETUP_UPDATEDMessageHandler));
		}

		#endregion Constructor

		#region Methods

		#region Expand/Collapse

		private void ExpandAll()
		{
			LoggerService.Inforamtion(this, "Expanding all the devices");

			foreach (DeviceData deviceBase in DevicesList)
				deviceBase.IsExpanded = true;
		}

		private void CollapseAll()
		{
			LoggerService.Inforamtion(this, "Collapsing all the devices");

			foreach (DeviceData deviceBase in DevicesList)
				deviceBase.IsExpanded = false;
		}

		#endregion Expand/Collapse

		#region Drag

		private void ListSourceParam_MouseEnter(MouseEventArgs e)
		{
			if (_designDragDropData.IsIgnor)
				return;

			if (e.MouseDevice.LeftButton == MouseButtonState.Pressed)
				_designDragDropData.IsMouseDown = true;
			else
				_designDragDropData.IsMouseDown = false;
		}

		private void ListSourceParam_PreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (_designDragDropData.IsIgnor)
				return;

			_designDragDropData.IsMouseDown = true;
			_designDragDropData.StartPoint = e.GetPosition(null);
		}

		private void ListSourceParam_PreviewMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			_designDragDropData.IsMouseDown = false;
		}

		private void ListSourceParam_MouseMove(MouseEventArgs e)
		{
			if (_designDragDropData.IsMouseDown == false)
				return;

			DragObject(e);
		}

		private void DragObject(MouseEventArgs e)
		{
			LoggerService.Inforamtion(this, "Object is draged");

			Point mousePos = e.GetPosition(null);
			Vector diff = _designDragDropData.StartPoint - mousePos;

			if (e.LeftButton == MouseButtonState.Pressed &&
				Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
				Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
			{

				TreeView treeView =
					FindAncestorService.FindAncestor<TreeView>((DependencyObject)e.OriginalSource);
				TreeViewItem treeViewItem =
					FindAncestorService.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

				DependencyObject sourceObject = treeViewItem;
				object item = null;

				if (treeView != null && treeViewItem != null)
				{
					item = treeViewItem.DataContext;

					if (item == null)
						return;
				}
				else
					return;

				if (!(item is DeviceParameterData param))
					return;

				DeviceParameterData actualParam = null;
				if (param.DeviceType != DeviceTypesEnum.EVVA)
				{
					DeviceFullData deviceFullData = _devicesContainer.TypeToDevicesFullData[param.DeviceType];
					if (deviceFullData == null)
						return;

					actualParam = deviceFullData.Device.ParemetersList.ToList().Find((p) => p.Name == param.Name);
					if (actualParam == null)
						return;

				}
				else
					actualParam = param;

				DataObject dragData = new DataObject(DragDropFormat, actualParam);
				DragDrop.DoDragDrop(sourceObject, dragData, DragDropEffects.Move);
			}
		}

		

		#endregion Drag

		#region Search parameter

		private void DeviceParamSearch_Text(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox tb))
				return;

			SetSearchedTest(tb.Text);
		}

		private void SetSearchedTest(string text)
		{
			foreach (DeviceData deviceBase in DevicesList)
			{
				if(!(deviceBase is DeviceData deviceData))
					continue;

				if (deviceData.ParemetersList == null)
					continue;

				HideShowParameters(deviceData.ParemetersList, text);

				if(deviceBase is MCU_DeviceData mcu_Device)
					mcu_Device.HideNotVisibleGroups();
			}
		}

		private void HideShowParameters(
			ObservableCollection<MCU_ParamData> list,
			string text)
		{
			foreach (DeviceParameterData data in list)
			{
				if (data is ParamGroup group)
				{
					HideShowParameters(group.ParamList, text);
					continue;
				}

				if (data.Name.ToLower().Contains(text.ToLower()))
					data.Visibility = Visibility.Visible;
				else
					data.Visibility = Visibility.Collapsed;
			}
		}

		private void HideShowParameters(
			ObservableCollection<DeviceParameterData> list,
			string text)
		{
			foreach (DeviceParameterData data in list)
			{
				if (data is ParamGroup group)
				{
					HideShowParameters(group.ParamList, text);
					continue;
				}

				if (data.Name.ToLower().Contains(text.ToLower()))
					data.Visibility = Visibility.Visible;
				else
					data.Visibility = Visibility.Collapsed;
			}
		}



		#endregion Search parameter


		private void ListSourceParam_MouseDoubleClick(MouseEventArgs e)
		{
			TreeViewItem treeViewItem =
						FindAncestorService.FindAncestor<TreeViewItem>((DependencyObject)e.OriginalSource);

			if (treeViewItem == null)
				return;

			if (!(treeViewItem.DataContext is DeviceParameterData paramData))
				return;

			ParamDoubleClickedEvent?.Invoke(paramData);
		}


		private void BuildDevicesList()
		{
			DevicesList = new ObservableCollection<DeviceData>();


			if (!_isHideEvvaDevice)
				AddEvvaDevice();


			foreach (DeviceData deviceBase in _devicesContainer.DevicesList)
			{
				if (_isHideEvvaDevice && deviceBase.Name == DeviceTypesEnum.EVVA.ToString())
					continue;

				DevicesList.Add(deviceBase.Clone() as DeviceData);
			}
		}

		private void AddEvvaDevice()
		{
			DeviceData deviceData = new DeviceData()
			{
				Name = DeviceTypesEnum.EVVA.ToString(),
				DeviceType = DeviceTypesEnum.EVVA,

			};

			deviceData.ParemetersList = new ObservableCollection<DeviceParameterData>()
			{
				new Evva_ParamData()
				{
					Name = "Safety officer on/off",
					Device = deviceData,
					DeviceType = DeviceTypesEnum.EVVA,
					DropDown = new List<DropDownParamData>()
					{
						new DropDownParamData() { Name = "OFF", Value = "0" },
						new DropDownParamData() { Name = "ON", Value = "1" },
					},
				}
			};

			DevicesList.Add(deviceData);
		}

		private void SETTINGS_UPDATEDMessageHandler(object sender, SETTINGS_UPDATEDMessage e)
		{
			BuildDevicesList();
		}

		private void SETUP_UPDATEDMessageHandler(object sender, SETUP_UPDATEDMessage e)
		{
			BuildDevicesList();
		}

		#endregion Methods


		#region Commands

		#region Drag

		private RelayCommand<MouseEventArgs> _ListSourceParam_MouseEnterCommand;
		public RelayCommand<MouseEventArgs> ListSourceParam_MouseEnterCommand
		{
			get
			{
				return _ListSourceParam_MouseEnterCommand ?? (_ListSourceParam_MouseEnterCommand =
					new RelayCommand<MouseEventArgs>(ListSourceParam_MouseEnter));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _ListSourceParam_PreviewMouseLeftButtonDownCommant;
		public RelayCommand<MouseButtonEventArgs> ListSourceParam_PreviewMouseLeftButtonDownCommant
		{
			get
			{
				return _ListSourceParam_PreviewMouseLeftButtonDownCommant ?? (_ListSourceParam_PreviewMouseLeftButtonDownCommant =
					new RelayCommand<MouseButtonEventArgs>(ListSourceParam_PreviewMouseLeftButtonDown));
			}
		}

		private RelayCommand<MouseButtonEventArgs> _ListSourceParam_PreviewMouseLeftButtonUpCommant;
		public RelayCommand<MouseButtonEventArgs> ListSourceParam_PreviewMouseLeftButtonUpCommant
		{
			get
			{
				return _ListSourceParam_PreviewMouseLeftButtonUpCommant ?? (_ListSourceParam_PreviewMouseLeftButtonUpCommant =
					new RelayCommand<MouseButtonEventArgs>(ListSourceParam_PreviewMouseLeftButtonUp));
			}
		}

		private RelayCommand<MouseEventArgs> _ListSourceParam_MouseMoveCommand;
		public RelayCommand<MouseEventArgs> ListSourceParam_MouseMoveCommand
		{
			get
			{
				return _ListSourceParam_MouseMoveCommand ?? (_ListSourceParam_MouseMoveCommand =
					new RelayCommand<MouseEventArgs>(ListSourceParam_MouseMove));
			}
		}





		#endregion Drag


		private RelayCommand<MouseEventArgs> _ListSourceParam_MouseDoubleClickCommand;
		public RelayCommand<MouseEventArgs> ListSourceParam_MouseDoubleClickCommand
		{
			get
			{
				return _ListSourceParam_MouseDoubleClickCommand ?? (_ListSourceParam_MouseDoubleClickCommand =
					new RelayCommand<MouseEventArgs>(ListSourceParam_MouseDoubleClick));
			}
		}



		public RelayCommand ExpandAllCommand { get; private set; }
		public RelayCommand CollapseAllCommand { get; private set; }

		private RelayCommand<TextChangedEventArgs> _DeviceParamSearch_TextChanged;
		public RelayCommand<TextChangedEventArgs> DeviceParamSearch_TextChanged
		{
			get
			{
				return _DeviceParamSearch_TextChanged ?? (_DeviceParamSearch_TextChanged =
					new RelayCommand<TextChangedEventArgs>(DeviceParamSearch_Text));
			}
		}

		#endregion Commands

		#region Events

		public event Action<DeviceParameterData> ParamDoubleClickedEvent;

		#endregion Events
	}
}
