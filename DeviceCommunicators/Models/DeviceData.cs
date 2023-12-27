
using Entities.Enums;
using Entities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace DeviceCommunicators.Models
{
	public class DeviceData:  ICloneable
	{
		[JsonIgnore]
		public bool IsExpanded { get; set; }
		[JsonIgnore]
		public bool IsSelected { get; set; }
		[JsonIgnore]
		public Visibility Visibility { get; set; }


		public string Name { get; set; }
		public DeviceTypesEnum DeviceType { get; set; }


		public virtual ObservableCollection<DeviceParameterData> ParemetersList { get; set; }

		public virtual object Clone()
		{
			DeviceData deviceData = MemberwiseClone() as DeviceData;
			deviceData.ParemetersList = new ObservableCollection<DeviceParameterData>();

			foreach (DeviceParameterData data in ParemetersList)
			{
				deviceData.ParemetersList.Add(data.Clone() as DeviceParameterData);
			}

			return deviceData;
		}
	}
}
