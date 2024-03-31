
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using DeviceCommunicators.MCU;

namespace DeviceHandler.ParamGetSetList
{
	public class ParamGetSetListViewModel: ObservableObject
	{
		#region Properties

		public bool IsShowButtons { get; set; }
		public ObservableCollection<DeviceParameterData> ParamsList { get; set; }

		#endregion Properties

		#region Fields


		#endregion Fields

		#region Constructor

		public ParamGetSetListViewModel(
			ObservableCollection<MCU_ParamData> paramsList,
			bool isShowButtons)
		{
			IsShowButtons = isShowButtons;

			ParamsList = new ObservableCollection<DeviceParameterData>();
			foreach(MCU_ParamData param in paramsList)
				ParamsList.Add(param);

			Init();
		}

		public ParamGetSetListViewModel(
			ObservableCollection<DeviceParameterData> paramsList,
			bool isShowButtons)
		{
			ParamsList = paramsList;
			IsShowButtons = isShowButtons;

			Init();
		}



		#endregion Constructor

		#region Methods

		private void Init()
		{

			GetCommand = new RelayCommand<DeviceParameterData>(Get);
			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			SaveCommand = new RelayCommand<DeviceParameterData>(Save);

			SetAllBackForeGround();
		}

		private void Get(DeviceParameterData param)
		{
			GetEvent?.Invoke(param);
		}

		private void Set(DeviceParameterData param)
		{
			SetEvent?.Invoke(param);
		}

		private void Save(DeviceParameterData param)
		{
			SaveEvent?.Invoke(param);
		}

		


		private void ComboBox_DropDownClosed(ComboBox comboBox)
		{
			if (!IsShowButtons)
				return;

			if (!(comboBox.DataContext is DeviceParameterData param))
				return;

			SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}

		private void TextBox_KeyUp(KeyEventArgs e)
		{
			if (!IsShowButtons)
				return;

			DeviceParameterData param = null;
			if (e.Source is TextBox textBox)
				param = textBox.DataContext as DeviceParameterData;
			else if (e.Source is ComboBox comboBox)
				param = comboBox.DataContext as DeviceParameterData;



			if (e.Key == Key.Enter)
			{
				Set(param);

				e.Handled = true;

				SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
				return;
			}

			SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}

		private void HexTextBox_EnterEvent(DeviceParameterData param)
		{
			Set(param);
			SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
		}

		private void HexTextBox_HexKeyDownEvent(KeyEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (!(textBox.DataContext is DeviceParameterData param))
				return;

			SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}



		public void SetAllBackForeGround()
		{
			foreach (var param in ParamsList)
			{
				SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
			}
		}

		public void SetButtonsEnabled(bool isEnabled)
		{
			foreach (DeviceParameterData param in ParamsList)
				param.IsEnabled = isEnabled;
		}

		public void SetBackForeGround(
			Brush background,
			Brush foreground,
			DeviceParameterData param)
		{
			Application.Current.Dispatcher.Invoke(() =>
			{
				param.Background = background;
				param.Foreground = foreground;
			});
		}

		public void ChangeDarkLight()
		{
			foreach (DeviceParameterData param in ParamsList)
			{
				if (param.Background != Brushes.Transparent)
					continue;

				param.Foreground = Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush;
			}
		}

		#endregion Methods

		#region Commands

		public RelayCommand<DeviceParameterData> GetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> SetCommand { get; private set; }
		public RelayCommand<DeviceParameterData> SaveCommand { get; private set; }


		private RelayCommand<ComboBox> _ComboBox_DropDownClosedCommand;
		public RelayCommand<ComboBox> ComboBox_DropDownClosedCommand
		{
			get
			{
				return _ComboBox_DropDownClosedCommand ?? (_ComboBox_DropDownClosedCommand =
					new RelayCommand<ComboBox>(ComboBox_DropDownClosed));
			}
		}

		//private RelayCommand<ComboBox> _ComboBox_DropDownOpenCommand;
		//public RelayCommand<ComboBox> ComboBox_DropDownOpenCommand
		//{
		//	get
		//	{
		//		return _ComboBox_DropDownOpenCommand ?? (_ComboBox_DropDownOpenCommand =
		//			new RelayCommand<ComboBox>(ComboBox_DropDownOpen));
		//	}
		//}

		private RelayCommand<KeyEventArgs> _TextBox_KeyUpCommand;
		public RelayCommand<KeyEventArgs> TextBox_KeyUpCommand
		{
			get
			{
				return _TextBox_KeyUpCommand ?? (_TextBox_KeyUpCommand =
					new RelayCommand<KeyEventArgs>(TextBox_KeyUp));
			}
		}

		private RelayCommand<DeviceParameterData> _HexTextBox_EnterEventCommand;
		public RelayCommand<DeviceParameterData> HexTextBox_EnterEventCommand
		{
			get
			{
				return _HexTextBox_EnterEventCommand ?? (_HexTextBox_EnterEventCommand =
					new RelayCommand<DeviceParameterData>(HexTextBox_EnterEvent));
			}
		}

		private RelayCommand<KeyEventArgs> _HexTextBox_HexKeyDownEventCommand;
		public RelayCommand<KeyEventArgs> HexTextBox_HexKeyDownEventCommand
		{
			get
			{
				return _HexTextBox_HexKeyDownEventCommand ?? (_HexTextBox_HexKeyDownEventCommand =
					new RelayCommand<KeyEventArgs>(HexTextBox_HexKeyDownEvent));
			}
		}

		#endregion Commands

		#region Events

		public event Action<DeviceParameterData> GetEvent;
		public event Action<DeviceParameterData> SetEvent;
		public event Action<DeviceParameterData> SaveEvent;

		#endregion Events
	}
}
