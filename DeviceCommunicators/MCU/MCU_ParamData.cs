
using Entities.Models;
using System.Collections.Generic;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using DeviceCommunicators.Models;

namespace DeviceCommunicators.MCU
{

	
	public class MCU_ParamData : DeviceParameterData, IParamWithDropDown
	{
		public Action ValueChanged;

		public void GetMessageID(ref byte[] id)
		{
			if(Cmd == null) 
				return;

			using (var md5 = MD5.Create())
			{
				Array.Copy(md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(Cmd)), 0, id, 0, 3);
			}
		}

		private bool _isSettingValue;
		public override object Value
		{
			get
			{
				if (IsAbsolute)
				{
					double d;
					bool res = double.TryParse(_value.ToString(), out d);
					if (!res)
						return _value;

					return Math.Abs(d);
				}
				else
					return _value;

			}
			set
			{
				if(_value != value)
					ValueChanged?.Invoke();

				_value = value;
				if (_isSettingSelectedDropDown)
					return;

				_isSettingValue = true;
				if (DropDown != null)
				{
					SelectedDropDown = DropDown.Find((dd) => dd.Name == (_value as string));
				}
				_isSettingValue = false;

			}
		}

		private object _editValue;
		public override object EditValue
		{
			get => _editValue;
			set
			{
				if (_editValue != value)
					ValueChanged?.Invoke();

				_editValue = value;
				if (_isSettingSelectedDropDown)
					return;

				_isSettingValue = true;
				if (DropDown != null)
				{
					SelectedDropDown = DropDown.Find((dd) => dd.Name == (_editValue as string));
				}
				_isSettingValue = false;

			}
		}

		public string GroupName { get; set; }

		/// <summary>
		/// Describes the parameter
		/// </summary>
		public String Description { get; set; }

		/// <summary>
		/// parameter default
		/// </summary>
		public String Default { get; set; }

		/// <summary>
		/// parameter command
		/// </summary>
		public String Cmd { get; set; }

		/// <summary>
		/// parameter range
		/// </summary>
		public List<double> Range { get; set; }

		/// <summary>
		/// Allow formating the value
		/// </summary>
		public string Format { get; set; }

		/// <summary>
		/// parameter scale
		/// </summary>
		private double _scale = 1;
		public double Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = (value != 0) ? value : 1;
			}

		}

		public String Note { get; set; }

		/// <summary>
		/// parameter need save button
		/// </summary>
		public bool Save { get; set; } = false;

		/// <summary>
		/// 
		/// </summary>
		public List<DropDownParamData> DropDown { get; set; }




		private bool _isSettingSelectedDropDown;

		private DropDownParamData _selectedDropDown;
		[JsonIgnore]
		public DropDownParamData SelectedDropDown 
		{
			get => _selectedDropDown;
			set
			{
				_selectedDropDown = value;
				if (_isSettingValue)
					return;

				if (_selectedDropDown == null)
					return;

				_isSettingSelectedDropDown = true;
				int nVal;
				bool res = int.TryParse(_selectedDropDown.Value, out nVal);
				Value = nVal;
				_isSettingSelectedDropDown = false;
			}
		}


		private DropDownParamData _editSelectedDropDown;
		[JsonIgnore]
		public DropDownParamData EditSelectedDropDown
		{
			get => _editSelectedDropDown;
			set
			{
				_editSelectedDropDown = value;
				if (_isSettingValue)
					return;

				if (_editSelectedDropDown == null)
					return;

				_isSettingSelectedDropDown = true;
				int nVal;
				bool res = int.TryParse(_editSelectedDropDown.Value, out nVal);
				EditValue = nVal;
				_isSettingSelectedDropDown = false;
			}
		}

		[JsonIgnore]
		public object Data { get; set; }

		
	}

	public class ParamGroup: DeviceParameterData
	{
		private string _groupName;
		public string GroupName 
		{ 
			get => _groupName; 
			set
			{
				_groupName = value;
				Name = value;
			}
		}

		public string GroupDescription { get; set; }

		public GroupType GroupType { get; set; }

		public ObservableCollection<MCU_ParamData> ParamList { get; set; }

		public override object Clone()
		{
			ParamGroup paramGroup = MemberwiseClone() as ParamGroup;
			paramGroup.ParamList = new ObservableCollection<MCU_ParamData>();

			foreach (MCU_ParamData data in ParamList)
			{
				paramGroup.ParamList.Add(data.Clone() as MCU_ParamData);
			}

			return paramGroup;
		}

		public void HideNotVisibleGroups()
		{
			bool isVisible = false;
			foreach(MCU_ParamData param in ParamList)
			{
				if(param.Visibility == Visibility.Visible)
				{
					isVisible = true;
					break;
				}
			}

			if (isVisible)
				Visibility = Visibility.Visible;
			else
				Visibility = Visibility.Collapsed;
		}

		
	}

	public enum GroupType
	{
		GENERAL = 1,
		DRIVING_PROFILE = 2,
		REAL_TIME = 3,
		DEVICE_INFO = 4,
		APP_CONFIG = 5
	}

	
}
