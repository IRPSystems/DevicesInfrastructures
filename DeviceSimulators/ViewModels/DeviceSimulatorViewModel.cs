﻿
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;
using DeviceHandler.Interfaces;
using Entities.Enums;
using Entities.Models;
using LibUsbDotNet.DeviceNotify;
using System.Collections.ObjectModel;

namespace DeviceSimulators.ViewModels
{
	public class DeviceSimulatorViewModel: ObservableObject
	{
		#region Properties

		public string DeviceName { get; set; }
		public DeviceTypesEnum DeviceType { get; set; }

		public IConnectionViewModel ConnectVM { get; set; }

		public ObservableCollection<DeviceParameterData> ParametersList { get; set; }

		#endregion Properties

		public DeviceSimulatorViewModel(DeviceData deviceData)
		{
			if (deviceData.ParemetersList == null)
				return;

			DeviceName = deviceData.Name;
			DeviceType = deviceData.DeviceType;

			ParametersList = new ObservableCollection<DeviceParameterData>();

			foreach(DeviceParameterData parameterData in deviceData.ParemetersList) 
			{
				DeviceParameterData newParameter =
					parameterData.Clone() as DeviceParameterData;
				newParameter.Visibility = System.Windows.Visibility.Visible;
				newParameter.GetSetVisibility = System.Windows.Visibility.Collapsed;

				ParametersList.Add(newParameter);
			}
		}

		public virtual void Disconnect()
		{

		}
	}
}
