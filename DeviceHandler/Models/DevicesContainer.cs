
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Enums;
using Entities.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DeviceHandler.Models
{
    public class DevicesContainer: ObservableObject
    {
		public ObservableCollection<DeviceFullData> DevicesFullDataList { get; set; }
		public ObservableCollection<DeviceData> DevicesList { get; set; }
		public Dictionary<DeviceTypesEnum, DeviceFullData> TypeToDevicesFullData { get; set; }
	}
}
