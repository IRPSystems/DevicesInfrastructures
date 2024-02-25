
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
	public class TcpConncetViewModel : ObservableObject, IConnectionViewModel
	{
		#region Properties

		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		public int Port { get; set; }
		public string Address { get; set; }


		public int RxPort { get; set; }
		public int TxPort { get; set; }

		public bool IsUdpSimulation { get; set; }

		[JsonIgnore]
		public GridLength UdpRowHeight { get; set; }


		#endregion Properties

		#region Constructor

		public TcpConncetViewModel(
			int port, 
			string address,
			int rxPort, 
			int txPort)
		{
			Port = port;	
			Address = address;
			RxPort = rxPort;
			TxPort = txPort;

			LoggerService.Inforamtion(this, "Starting TcpConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);
			IsUdpSimulationClickCommand = new RelayCommand(IsUdpSimulationClick);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;

			if(string.IsNullOrEmpty(address))
				GetIpAddress();

			HandleSelectedAddapter();

			LoggerService.Inforamtion(this, "Ending Init of TcpConncetViewModel");
		}

		#endregion Constructor

		#region Methods

		public void RefreshProperties()
		{
			HandleSelectedAddapter();
		}

		private void HandleSelectedAddapter()
		{
			if (IsUdpSimulation)
				UdpRowHeight = new GridLength(40);
			else
				UdpRowHeight = new GridLength(0);
		}

		private void IsUdpSimulationClick()
		{
			HandleSelectedAddapter();
		}

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
		[JsonIgnore]
		public RelayCommand IsUdpSimulationClickCommand { get; private set; }



		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}
