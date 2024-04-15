
using DeviceCommunicators.Models;
using Entities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DeviceCommunicators.PowerSupplayGK
{
	public class PowerSupplayGK_ParamData : DeviceParameterData, IParamWithDropDown
	{
		public ushort ReadAddress { get; set; }
		public ushort WriteAddress { get; set; }
		public ushort WriteTriggerAddress { get; set; }
		public double Scale { get; set; }





		private bool _isSettingValue;
		public override object Value
		{
			get
			{
				
				return _value;

			}
			set
			{

				_value = value;
				if (_isSettingSelectedDropDown)
					return;

				_isSettingValue = true;
				if (DropDown != null)
				{
					SelectedDropDown = DropDown.Find((dd) => dd.Name == (_value as string));
					if(SelectedDropDown == null)
						SelectedDropDown = DropDown.Find((dd) => dd.Value == (_value.ToString()));

				}
				_isSettingValue = false;

			}
		}

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

				_isSettingSelectedDropDown = true;
				int nVal;
				bool res = int.TryParse(_selectedDropDown.Value, out nVal);
				Value = nVal;
				_isSettingSelectedDropDown = false;
			}
		}

		public List<DropDownParamData> DropDown { get; set; }
	}
}
