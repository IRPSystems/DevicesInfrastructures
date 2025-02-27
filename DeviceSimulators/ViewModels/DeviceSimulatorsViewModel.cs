﻿
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceCommunicators.Models;
using DeviceHandler.Models;
using DeviceHandler.Models.DeviceFullDataModels;
using Entities.Enums;
using Entities.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DeviceSimulators.ViewModels
{
    public class DeviceSimulatorsViewModel: ObservableObject
    {
        public ObservableCollection<DeviceSimulatorViewModel> ViewModelsList { get; set; }

		private DevicesContainer _devicesContainer;


		public DeviceSimulatorsViewModel(DevicesContainer devicesContainer)
        {
			_devicesContainer = devicesContainer;

			ClosingCommand = new RelayCommand<CancelEventArgs>(Closing);


			ViewModelsList = new ObservableCollection<DeviceSimulatorViewModel>();

			UpdateDevices();
		}

		private void Closing(CancelEventArgs e)
        {
            foreach(DeviceSimulatorViewModel vm in ViewModelsList)
            {
                vm.Disconnect();
            }
        }

		private void SearchText_TextChanged(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (!(textBox.DataContext is DeviceSimulatorViewModel simulator))
				return;

			foreach (DeviceParameterData param in simulator.ParametersList)
			{
				if (param.Name.ToLower().Contains(textBox.Text.ToLower()))
					param.Visibility = Visibility.Visible;
				else
					param.Visibility = Visibility.Collapsed;
			}
		}

		public void UpdateDevices()
		{
			foreach (DeviceFullData deviceFullData in _devicesContainer.DevicesFullDataList)
			{
				bool isAlreadyExist = IsAlreadyExist(deviceFullData);
				if (isAlreadyExist)
					continue;

				switch (deviceFullData.Device.DeviceType)
				{

					case DeviceTypesEnum.Dyno:
						ViewModelsList.Add(new DynoSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.MCU:
					case DeviceTypesEnum.MCU_B2B:
						ViewModelsList.Add(new MCUSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.PowerSupplyBK:
						ViewModelsList.Add(new PSBKSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.PowerSupplyEA:
						ViewModelsList.Add(new PSEASimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.BTMTempLogger:
						ViewModelsList.Add(new BTMTempLoggerSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.SwitchRelay32:
						ViewModelsList.Add(new SwitchRelaySimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.TorqueKistler:
						ViewModelsList.Add(new TKSimulatorMainWindowViewModel(deviceFullData.Device));
						break;

					case DeviceTypesEnum.PowerSupplyKeysight:
						ViewModelsList.Add(new PSKeysightSimulatorMainWindowViewModel(deviceFullData.Device));

						break;

					case DeviceTypesEnum.Yokogawa_WT1804E:
						break;

				}
			}

		}

		private bool IsAlreadyExist(DeviceFullData deviceFullData)
		{
			foreach(DeviceSimulatorViewModel simulator in ViewModelsList)
			{
				if(simulator.DeviceName == deviceFullData.Device.Name)
					return true;
			}

			return false;
		}

		public void Remove(DeviceTypesEnum deviceTypes)
		{

			DeviceSimulatorViewModel sim = 
				ViewModelsList.ToList().Find((d) => d.DeviceType == deviceTypes);
			if(sim != null) 
				ViewModelsList.Remove(sim);
		}


		public RelayCommand<CancelEventArgs> ClosingCommand { get; private set; }

		private RelayCommand<TextChangedEventArgs> _SearchText_TextChangedCommand;
		public RelayCommand<TextChangedEventArgs> SearchText_TextChangedCommand
		{
			get
			{
				return _SearchText_TextChangedCommand ?? (_SearchText_TextChangedCommand =
					new RelayCommand<TextChangedEventArgs>(SearchText_TextChanged));
			}
		}
	}
}
