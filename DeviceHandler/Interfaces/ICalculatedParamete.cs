﻿
using DeviceHandler.Models;
using System.Collections.ObjectModel;

namespace DeviceHandler.Interfaces
{
	public interface ICalculatedParamete
	{
		void Calculate();
		ObservableCollection<DeviceFullData> DevicesList { get; set; }
	}
}
