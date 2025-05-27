
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using DeviceHandler.Views;
using Entities.Enums;
using Entities.Models;
using Services.Services;
using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DeviceHandler.ViewModels
{
	public class ParamGroupViewModel : ObservableObject
	{
		#region Properties

		public ParamGroup ParamGroup { get; set; }
		public bool IsShowButtons { get; set; }

		#endregion Properties

		#region Fields


		private bool _isInGetAll;
		private int _getAllIndex;

		private bool _isInSetAll;
		private int _setAllIndex;

		private DevicesContainer _devicesContainer;

		#endregion Fields

		#region Constructor

		public ParamGroupViewModel(
			DevicesContainer devicesContainer,
			ParamGroup groupData,
			bool isShowButtons)
		{
			_devicesContainer = devicesContainer;
			ParamGroup = groupData;
			IsShowButtons = isShowButtons;

			GetCommand = new RelayCommand<DeviceParameterData>(Get);
			SetCommand = new RelayCommand<DeviceParameterData>(Set);
			SaveCommand = new RelayCommand<DeviceParameterData>(Save);

			SetAllBackForeGround();
		}

		#endregion Constructor

		#region Methods

		private void Get(DeviceParameterData param)
		{
			if (param.Name == "Parameter file Version")
				return;

			if (!(param is MCU_ParamData mcuParam))
				return;

			if(_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			DeviceFullData mcuFullData = 
				_devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];
			mcuFullData.DeviceCommunicator.GetParamValue(mcuParam, ResponseCallback);
		}

		private void Set(DeviceParameterData param)
		{
			if (!(param is MCU_ParamData mcuParam))
				return;

			double dVal = 0;
			if (param.Value == null)
				return;

			if (param.Value is string str)
			{
				if (string.IsNullOrEmpty(str))
					return;

				bool res = false;
				if (mcuParam.Format == "X")
				{
					str = str.Replace("0x", string.Empty);
					uint uVal;
					res = uint.TryParse(str, NumberStyles.HexNumber, null, out uVal);
					dVal = uVal;
				}
				else
				{
					res = double.TryParse(str, out dVal); 
					if (res == false)
					{
						if (mcuParam.DropDown != null && mcuParam.DropDown.Count > 0)
						{
							DropDownParamData dd =
								mcuParam.DropDown.Find((d) => d.Name == str);
							if (dd != null)
							{
								res = double.TryParse(dd.Value, out dVal);
							}
						}
					}
				}

				if (res == false)
					return;
			}
			else
				dVal = Convert.ToDouble(param.Value);

			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			DeviceFullData mcuFullData =
				_devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

			mcuFullData.DeviceCommunicator.SetParamValue(mcuParam, dVal, ResponseCallback);
		}

		private void Save(DeviceParameterData param)
		{
			if (!(param is MCU_ParamData mcuParam))
				return;

			if (mcuParam.Cmd == null)
				return;


			Set(param);

			byte[] id = new byte[3];

			using (var md5 = MD5.Create())
			{
				Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(mcuParam.Cmd)), 0, id, 0, 3);

			}

			var hex_id = BitConverter.ToString(id).Replace("-", "").ToLower();

			var msg = Convert.ToInt32(hex_id, 16); 
			
			if (_devicesContainer.TypeToDevicesFullData.ContainsKey(DeviceTypesEnum.MCU) == false)
				return;

			DeviceFullData mcuFullData =
				_devicesContainer.TypeToDevicesFullData[DeviceTypesEnum.MCU];

			mcuFullData.DeviceCommunicator.SetParamValue(
				new MCU_ParamData() { Cmd = "save_param" },
				msg,
				ResponseCallback);
		}


		public void GetAll()
		{
			_isInGetAll = true;
			_getAllIndex = 0;

			Get(ParamGroup.ParamList[_getAllIndex++]);
		}

		public void SetAll()
		{
			_isInSetAll = true;
			_setAllIndex = 0;

			while (ParamGroup.ParamList[_setAllIndex].Value == null ||
				(ParamGroup.ParamList[_setAllIndex].Value is string str && string.IsNullOrEmpty(str)))
			{
				_setAllIndex++;
			}

			Set(ParamGroup.ParamList[_setAllIndex++]);

			System.Threading.Thread.Sleep(1);
		}

		public void SetAllBackForeGround()
		{
			foreach (var param in ParamGroup.ParamList)
			{
				SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
			}
		}

		private void ResponseCallback(DeviceParameterData param, CommunicatorResultEnum result, string errDescription)
		{
			try
			{
				if (result == CommunicatorResultEnum.OK)
				{
					SetBackForeGround(
						Brushes.Transparent,
						Application.Current.FindResource("MahApps.Brushes.ThemeForeground") as SolidColorBrush,
						param);
					param.ErrorDescription = null;



					if (param is MCU_ParamData mcuParam)
					{
						param.Value = GetFormatedValuesService.GetString(mcuParam.Format, mcuParam.Value);
					}
				}
				else
				{
					SetBackForeGround(
						Brushes.Red,
						Brushes.White,
						param);


					if (result == CommunicatorResultEnum.NoResponse && string.IsNullOrEmpty(errDescription))
						errDescription = "No connection";

					param.ErrorDescription = errDescription;

					if (!_isInGetAll && !_isInSetAll)
					{
						Application.Current.Dispatcher.Invoke(() =>
						{
							ErrorBalloonView errorBalloonView = new ErrorBalloonView();
							errorBalloonView.DataContext = param;
							errorBalloonView.Show();
						});
					}

				}

				if (_isInGetAll)
				{
					if (_getAllIndex >= ParamGroup.ParamList.Count)
					{
						GetAllEndedEvent?.Invoke();

						_isInGetAll = false;

						return;
					}

					Get(ParamGroup.ParamList[_getAllIndex++]);
				}
				else if (_isInSetAll)
				{
					if (_setAllIndex >= ParamGroup.ParamList.Count)
					{
						SetAllEndedEvent?.Invoke();

						_isInSetAll = false;

						return;
					}

					while (ParamGroup.ParamList[_setAllIndex].Value == null ||
						(ParamGroup.ParamList[_setAllIndex].Value is string str && string.IsNullOrEmpty(str)))
					{
						_setAllIndex++;
						if (_setAllIndex >= ParamGroup.ParamList.Count)
						{
							SetAllEndedEvent?.Invoke();

							_isInSetAll = false;

							return;
						}
					}

					Set(ParamGroup.ParamList[_setAllIndex++]);
				}

			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Error at callback", ex);
			}
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

		//private void ComboBox_DropDownOpen(ComboBox comboBox)
		//{
		//	if (!IsShowButtons)
		//	{
		//		comboBox.IsDropDownOpen = false;
		//		return;
		//	}
		//}

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

		public void SetButtonsEnabled(bool isEnabled)
		{
			foreach (MCU_ParamData param in ParamGroup.ParamList)
				param.IsEnabled = isEnabled;
		}

		private void SetBackForeGround(
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
			foreach (MCU_ParamData param in ParamGroup.ParamList)
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

		public event Action GetAllEndedEvent;
		public event Action SetAllEndedEvent;

		#endregion Events
	}
}
