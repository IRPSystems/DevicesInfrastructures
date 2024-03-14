
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Net.Sockets;
using System.Net;
using CommunityToolkit.Mvvm.Input;
using System;
using Services.Services;
using DeviceHandler.Interfaces;
using System.Windows;

namespace DeviceHandler.ViewModels
{
	public class IPAddressOnlyConncetViewModel : ObservableObject, IConnectionViewModel
	{
		#region Properties

		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		public string Address { get; set; }


		public bool IsUdpSimulation { get; set; }


		#endregion Properties

		#region Constructor

		public IPAddressOnlyConncetViewModel()
		{

			LoggerService.Inforamtion(this, "Starting YokogawaWT1804EConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;

			GetIpAddress();

			LoggerService.Inforamtion(this, "Ending Init of YokogawaWT1804EConncetViewModel");
		}

		#endregion Constructor

		#region Methods


		private void GetIpAddress()
		{
			Address = null;
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					Address = ip.ToString();
				}
			}
		}

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
