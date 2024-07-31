
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
using Entities.Models;

namespace DeviceHandler.ParamGetSetList
{
	public class ParamGetSetListViewModel: ObservableObject
	{
		#region Properties

		public ObservableCollection<DeviceParameterData> ParamsList { get; set; }

		public Visibility HelpToolVisibility { get; set; }
		public Visibility SaveVisibility { get; set; }
		public Visibility ButtonsVisibility { get; set; }

		public Action<KeyEventArgs> TextBox_KeyUpEvent { get; set; }
		public Action<ComboBox> ComboBox_SelectionChangedEvent { get; set; }
		public Action<DeviceParameterData> HexTextBox_EnterEvent { get; set; }
		public Action<KeyEventArgs> HexTextBox_HexKeyDownEvent { get; set; }
		public Action<KeyEventArgs> HexTextBox_HexKeyUpEvent { get; set; }

		public Action<DeviceParameterData> ButtonGet_ClickEvent { get; set; }
		public Action<DeviceParameterData> ButtonSet_ClickEvent { get; set; }
		public Action<DeviceParameterData> ButtonSave_ClickEvent { get; set; }


		public bool IsInitiating { get; set; }

		#endregion Properties

		#region Fields


		#endregion Fields

		#region Constructor

		public ParamGetSetListViewModel(
			ObservableCollection<MCU_ParamData> paramsList,
			bool isShowButtons,
			bool isShowSave)
		{	

			ParamsList = new ObservableCollection<DeviceParameterData>();
			foreach (MCU_ParamData param in paramsList)
				ParamsList.Add(param);

			Init(
				isShowButtons,
				true,
				isShowSave);
		}

		public ParamGetSetListViewModel(
			ObservableCollection<DeviceParameterData> paramsList,
			bool isShowButtons,
			bool isShowHelpTool,
			bool isShowSave)
		{
			ParamsList = paramsList;

			Init(
				isShowButtons,
				isShowHelpTool,
				isShowSave);
		}

		#endregion Constructor

		#region Methods

		private void Init(
			bool isShowButtons,
			bool isShowHelpTool,
			bool isShowSave)
		{
			TextBox_KeyUpEvent = TextBox_KeyUp;
			ComboBox_SelectionChangedEvent = ComboBox_SelectionChanged;
			HexTextBox_EnterEvent = HexTextBox_Enter;
			HexTextBox_HexKeyDownEvent = HexTextBox_HexKeyDown;
			HexTextBox_HexKeyUpEvent = TextBox_KeyUp;

			ButtonGet_ClickEvent = Get;
			ButtonSet_ClickEvent = Set;
			ButtonSave_ClickEvent = Save;

			if (isShowHelpTool)
				HelpToolVisibility = Visibility.Visible;
			else
				HelpToolVisibility = Visibility.Collapsed;

			if (isShowButtons)
				ButtonsVisibility = Visibility.Visible;
			else
				ButtonsVisibility = Visibility.Collapsed;

			if (isShowSave)
				SaveVisibility = Visibility.Visible;
			else
				SaveVisibility = Visibility.Collapsed;

			

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

		


		public void ComboBox_SelectionChanged(ComboBox comboBox)
		{
			if(IsInitiating) 
				return;

			if (ButtonsVisibility == Visibility.Collapsed)
				return;

			if (!(comboBox.DataContext is DeviceParameterData param))
				return;

			if(param is IParamWithDropDown dropDown && 
				(dropDown.DropDown == null || dropDown.DropDown.Count == 0))
			{
				return;
			}

			SetBackForeGround(
						Application.Current.FindResource("MahApps.Brushes.Accent2") as SolidColorBrush,
						Brushes.White,
						param);
		}

		private void TextBox_KeyUp(KeyEventArgs e)
		{
			if (ButtonsVisibility == Visibility.Collapsed)
				return;

			if (e.Key == Key.Tab)
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

		private void HexTextBox_Enter(DeviceParameterData param)
		{
			Set(param);
			SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
		}

		private void HexTextBox_HexKeyDown(KeyEventArgs e)
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

		public static void SetBackForeGround(
			Brush background,
			Brush foreground,
			DeviceParameterData param)
		{
			if (Application.Current == null)
				return;

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


		private RelayCommand<ComboBox> _ComboBox_DropDownClosedCommand;
		public RelayCommand<ComboBox> ComboBox_DropDownClosedCommand
		{
			get
			{
				return _ComboBox_DropDownClosedCommand ?? (_ComboBox_DropDownClosedCommand =
					new RelayCommand<ComboBox>(ComboBox_SelectionChanged));
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
