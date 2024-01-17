
using Newtonsoft.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Enums;
using System.Windows;
using System;
using System.Windows.Media;

namespace DeviceCommunicators.Models
{
	public class DeviceParameterData:ObservableObject, ICloneable
	{

		public string Name { get; set; }
		public string Units { get; set; }

		protected object _value;
		public virtual object Value
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
				_value = value;


			}
		}


		[JsonIgnore]
		public DeviceData Device { get; set; }
		public DeviceTypesEnum DeviceType { get; set; }


		[JsonIgnore]
		public bool IsExpanded { get; set; }
		[JsonIgnore]
		public bool IsSelected { get; set; }
		[JsonIgnore]
		public Visibility Visibility { get; set; }
		[JsonIgnore]
		public Visibility GetSetVisibility { get; set; }
		[JsonIgnore]
		public Brush Background { get; set; }

		[JsonIgnore]
		public bool IsAbsolute { get; set; }

		public DeviceParameterData()
		{
			
		}

		public virtual object Clone()
		{
			return MemberwiseClone();
		}


		public override string ToString()
		{
			string str = Name;
			if(Device != null) 
				str += " (" + Device.Name + ")";

			return str;
		}
	}
}
