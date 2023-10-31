
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Interfaces;
using Entities.Models;
using Newtonsoft.Json;
using Services.Services;
using System;

namespace DeviceHandler.ViewModels
{
	public class NI6002ConncetViewModel : ObservableObject, IConnectionViewModel
	{
		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		public string DeviceName { get; set; }

		public bool IsUdpSimulation { get; set; }

		public NI6002ConncetViewModel() 
		{
			DeviceName = "Dev1";
			LoggerService.Inforamtion(this, "Starting NI6002ConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;

		
			LoggerService.Inforamtion(this, "Ending Init of NI6002ConncetViewModel");
		}

		#region Methods

		public void RefreshProperties() { }

		private void Connect()
		{
			ConnectEvent?.Invoke();
		}

		private void Disconnect()
		{
			DisconnectEvent?.Invoke();
		}

		#endregion Methods

		#region Commands

		[JsonIgnore]
		public RelayCommand ConnectCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand DisconnectCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}
