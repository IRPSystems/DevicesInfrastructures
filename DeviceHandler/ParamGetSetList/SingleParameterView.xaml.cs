using CommunityToolkit.Mvvm.Input;
using Controls.Views;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using Entities.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DeviceHandler.ParamGetSetList
{
	/// <summary>
	/// Interaction logic for SingleParameterView.xaml
	/// </summary>
	public partial class SingleParameterView : UserControl
	{
		#region Properties

		#region ActualUnits

		public static readonly DependencyProperty ActualUnitsProperty = DependencyProperty.Register(
			"ActualUnits", typeof(string), typeof(SingleParameterView));

		public string ActualUnits
		{
			get => (string)GetValue(ActualUnitsProperty);
			set => SetValue(ActualUnitsProperty, value);
		}

		#endregion ActualUnits

		#region HelpToolVisibility

		public static readonly DependencyProperty HelpToolVisibilityProperty = DependencyProperty.Register(
			"HelpToolVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility HelpToolVisibility
		{
			get => (Visibility)GetValue(HelpToolVisibilityProperty);
			set => SetValue(HelpToolVisibilityProperty, value);
		}

		#endregion HelpToolVisibility

		#region ButtonsVisibility

		public static readonly DependencyProperty ButtonsVisibilityProperty = DependencyProperty.Register(
			"ButtonsVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility ButtonsVisibility
		{
			get => (Visibility)GetValue(ButtonsVisibilityProperty);
			set => SetValue(ButtonsVisibilityProperty, value);
		}

		#endregion ButtonsVisibility

		#region SaveVisibility

		public static readonly DependencyProperty SaveVisibilityProperty = DependencyProperty.Register(
			"SaveVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility SaveVisibility
		{
			get => (Visibility)GetValue(SaveVisibilityProperty);
			set => SetValue(SaveVisibilityProperty, value);
		}

		#endregion SaveVisibility



		#region HexTextBoxVisibility

		public static readonly DependencyProperty HexTextBoxVisibilityProperty = DependencyProperty.Register(
			"HexTextBoxVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility HexTextBoxVisibility
		{
			get => (Visibility)GetValue(HexTextBoxVisibilityProperty);
			set => SetValue(HexTextBoxVisibilityProperty, value);
		}

		#endregion HexTextBoxVisibility

		#region RegularTextBoxVisibility

		public static readonly DependencyProperty RegularTextBoxVisibilityProperty = DependencyProperty.Register(
			"RegularTextBoxVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility RegularTextBoxVisibility
		{
			get => (Visibility)GetValue(RegularTextBoxVisibilityProperty);
			set => SetValue(RegularTextBoxVisibilityProperty, value);
		}

		#endregion RegularTextBoxVisibility

		#region CombotBoxVisibility

		public static readonly DependencyProperty CombotBoxVisibilityProperty = DependencyProperty.Register(
			"CombotBoxVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility CombotBoxVisibility
		{
			get => (Visibility)GetValue(CombotBoxVisibilityProperty);
			set => SetValue(CombotBoxVisibilityProperty, value);
		}

		#endregion CombotBoxVisibility

		#region CombotTextBoxVisibility

		public static readonly DependencyProperty CombotTextBoxVisibilityProperty = DependencyProperty.Register(
			"CombotTextBoxVisibility", typeof(Visibility), typeof(SingleParameterView));

		public Visibility CombotTextBoxVisibility
		{
			get => (Visibility)GetValue(CombotTextBoxVisibilityProperty);
			set => SetValue(CombotTextBoxVisibilityProperty, value);
		}

		#endregion CombotTextBoxVisibility



		#region IsReadOnly

		public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
			"IsReadOnly", typeof(bool), typeof(SingleParameterView));

		public bool IsReadOnly
		{
			get => (bool)GetValue(IsReadOnlyProperty);
			set => SetValue(IsReadOnlyProperty, value);
		}

		#endregion IsReadOnly



		#region TextBoxKeyUpEvent

		public static readonly DependencyProperty TextBoxKeyUpEventProperty = DependencyProperty.Register(
			"TextBoxKeyUpEvent", typeof(Action<KeyEventArgs>), typeof(SingleParameterView));

		public Action<KeyEventArgs> TextBoxKeyUpEvent
		{
			get => (Action<KeyEventArgs>)GetValue(TextBoxKeyUpEventProperty);
			set => SetValue(TextBoxKeyUpEventProperty, value);
		}

		#endregion TextBoxKeyUpEvent

		#region ComboBox_SelectionChangedEvent

		public static readonly DependencyProperty ComboBox_SelectionChangedEventProperty = DependencyProperty.Register(
			"ComboBox_SelectionChangedEvent", typeof(Action<ComboBox>), typeof(SingleParameterView));

		public Action<ComboBox> ComboBox_SelectionChangedEvent
		{
			get => (Action<ComboBox>)GetValue(ComboBox_SelectionChangedEventProperty);
			set => SetValue(ComboBox_SelectionChangedEventProperty, value);
		}

		#endregion ComboBox_SelectionChangedEvent

		#region HexTextBox_EnterEvent

		public static readonly DependencyProperty HexTextBox_EnterEventProperty = DependencyProperty.Register(
			"HexTextBox_EnterEvent", typeof(Action<DeviceParameterData>), typeof(SingleParameterView));

		public Action<DeviceParameterData> HexTextBox_EnterEvent
		{
			get => (Action<DeviceParameterData>)GetValue(HexTextBox_EnterEventProperty);
			set => SetValue(HexTextBox_EnterEventProperty, value);
		}

		#endregion HexTextBox_EnterEvent

		#region HexTextBox_KeyDownEvent

		public static readonly DependencyProperty HexTextBox_KeyDownEventProperty = DependencyProperty.Register(
			"HexTextBox_KeyDownEvent", typeof(Action<KeyEventArgs>), typeof(SingleParameterView));

		public Action<KeyEventArgs> HexTextBox_KeyDownEvent
		{
			get => (Action<KeyEventArgs>)GetValue(HexTextBox_KeyDownEventProperty);
			set => SetValue(HexTextBox_KeyDownEventProperty, value);
		}

		#endregion HexTextBox_KeyDownEvent

		#region HexTextBox_KeyUpEvent

		public static readonly DependencyProperty HexTextBox_KeyUpEventProperty = DependencyProperty.Register(
			"HexTextBox_KeyUpEvent", typeof(Action<KeyEventArgs>), typeof(SingleParameterView));

		public Action<KeyEventArgs> HexTextBox_KeyUpEvent
		{
			get => (Action<KeyEventArgs>)GetValue(HexTextBox_KeyUpEventProperty);
			set => SetValue(HexTextBox_KeyUpEventProperty, value);
		}

		#endregion HexTextBox_KeyUpEvent

		#region ButtonGet_ClickEvent

		public static readonly DependencyProperty ButtonGet_ClickEventProperty = DependencyProperty.Register(
			"ButtonGet_ClickEvent", typeof(Action<DeviceParameterData>), typeof(SingleParameterView));

		public Action<DeviceParameterData> ButtonGet_ClickEvent
		{
			get => (Action<DeviceParameterData>)GetValue(ButtonGet_ClickEventProperty);
			set => SetValue(ButtonGet_ClickEventProperty, value);
		}

		#endregion ButtonGet_ClickEvent

		#region ButtonSet_ClickEvent

		public static readonly DependencyProperty ButtonSet_ClickEventProperty = DependencyProperty.Register(
			"ButtonSet_ClickEvent", typeof(Action<DeviceParameterData>), typeof(SingleParameterView));

		public Action<DeviceParameterData> ButtonSet_ClickEvent
		{
			get => (Action<DeviceParameterData>)GetValue(ButtonSet_ClickEventProperty);
			set => SetValue(ButtonSet_ClickEventProperty, value);
		}

		#endregion ButtonSet_ClickEvent

		#region ButtonSave_ClickEvent

		public static readonly DependencyProperty ButtonSave_ClickEventProperty = DependencyProperty.Register(
			"ButtonSave_ClickEvent", typeof(Action<DeviceParameterData>), typeof(SingleParameterView));

		public Action<DeviceParameterData> ButtonSave_ClickEvent
		{
			get => (Action<DeviceParameterData>)GetValue(ButtonSave_ClickEventProperty);
			set => SetValue(ButtonSave_ClickEventProperty, value);
		}

		#endregion ButtonSave_ClickEvent



		#region MCUParam

		public static readonly DependencyProperty MCUParamProperty = DependencyProperty.Register(
			"ParamData", typeof(DeviceParameterData), typeof(SingleParameterView));

		public DeviceParameterData ParamData
		{
			get => (DeviceParameterData)GetValue(MCUParamProperty);
			set
			{
				SetValue(MCUParamProperty, value);
			}
		}

		#endregion MCUParam

		#endregion Properties

		private HexTextBoxView _hexTextBoxView;

		#region Constructor

		public SingleParameterView()
		{
			InitializeComponent();

			foreach(UIElement element in grdValues.Children)
			{
				if (!(element is HexTextBoxView hexTextBoxView))
					continue;

				_hexTextBoxView = hexTextBoxView;
			}

			_hexTextBoxView.EnterEvent += _hexTextBoxView_EnterEvent;
			_hexTextBoxView.HexKeyDownEvent += _hexTextBoxView_HexKeyDownEvent;
			_hexTextBoxView.HexKeyUpEvent += _hexTextBoxView_HexKeyUpEvent;
		}

		#endregion Constructor

		#region Methods

		

		private void ParamChanged()
		{

			if(string.IsNullOrEmpty(ParamData.Units) == false) 
			{
				ActualUnits = "[" + ParamData.Units + "]";
			}

			MCU_ParamData mcuParam = ParamData as MCU_ParamData;

			if (mcuParam != null && mcuParam.Save)
				SaveVisibility = SaveVisibility;
			else
				SaveVisibility = Visibility.Collapsed;


			HexTextBoxVisibility = Visibility.Collapsed;
			RegularTextBoxVisibility = Visibility.Collapsed;
			CombotBoxVisibility = Visibility.Collapsed;
			CombotTextBoxVisibility = Visibility.Collapsed;

			if (mcuParam != null && string.IsNullOrEmpty(mcuParam.Format) == false)
				HexTextBoxVisibility = Visibility.Visible;

			else if (ParamData is IParamWithDropDown withDropDown && withDropDown.DropDown != null && withDropDown.DropDown.Count != 0)
			{
				if (ButtonsVisibility == Visibility.Visible)
				{
					CombotBoxVisibility = Visibility.Visible;
					CombotTextBoxVisibility = Visibility.Collapsed;
				}
				else
				{
					CombotBoxVisibility = Visibility.Collapsed;
					CombotTextBoxVisibility = Visibility.Visible;
				}
			}
			else
				RegularTextBoxVisibility = Visibility.Visible;

			IsReadOnly = false;
			if (ButtonsVisibility != Visibility.Visible)
				IsReadOnly = true;
		}

		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			ParamChanged();
		}

		private void tb_KeyUp(object sender, KeyEventArgs e)
		{
			TextBoxKeyUpEvent?.Invoke(e);
		}

		//private void ComboBox_DropDownClosed(object sender, EventArgs e)
		//{
		//	ComboBox_DropDownClosedEvent?.Invoke(sender as ComboBox);
		//}

		private void cb_DropDownOpened(object sender, EventArgs e)
		{
			if (!(DataContext is ParamGetSetListViewModel vm))
				return;

			if (vm.ButtonsVisibility == Visibility.Collapsed)
				return;


			if (!(sender is ComboBox comboBox))
				return;

			comboBox.IsDropDownOpen = false;
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			ComboBox_SelectionChangedEvent?.Invoke(sender as ComboBox);
		}

		private void _hexTextBoxView_EnterEvent(object sender, EventArgs e)
		{
			HexTextBox_EnterEvent?.Invoke(ParamData);
		}

		private void _hexTextBoxView_HexKeyDownEvent(object sender, KeyEventArgs e)
		{
			HexTextBox_KeyDownEvent?.Invoke(e);
		}

		private void _hexTextBoxView_HexKeyUpEvent(object sender, KeyEventArgs e)
		{
			HexTextBox_KeyUpEvent?.Invoke(e);
		}

		private void ButtonGet_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button button))
				return;

			if (!(button.DataContext is DeviceParameterData param))
				return;

			ButtonGet_ClickEvent?.Invoke(param);
		}

		private void ButtonSet_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button button))
				return;

			if (!(button.DataContext is DeviceParameterData param))
				return;

			ButtonSet_ClickEvent?.Invoke(param);
		}

		private void ButtonSave_Click(object sender, RoutedEventArgs e)
		{
			if (!(sender is Button button))
				return;

			if (!(button.DataContext is DeviceParameterData param))
				return;

			ButtonSave_ClickEvent?.Invoke(param);
		}



		#endregion Methods

		#region Commands


		#endregion Commands

		
	}
}
