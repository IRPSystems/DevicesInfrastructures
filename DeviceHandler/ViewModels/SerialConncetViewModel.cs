
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows.Controls;
using Services.Services;
using DeviceHandler.Interfaces;
using Communication.Services;

namespace DeviceHandler.ViewModels
{
	public class SerialConncetViewModel: ObservableObject, IConnectionViewModel
	{
		#region Properties

		[JsonIgnore]
		public ObservableCollection<string> COMList { get; set; }
		[JsonIgnore]
		public ObservableCollection<int> BaudratesList { get; set; }

		public string SelectedCOM { get; set; }
		public int SelectedBaudrate { get; set; }

		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		public int RxPort { get; set; }
		public int TxPort { get; set; }
		public string Address { get; set; }

		public bool IsUdpSimulation { get; set; }


		[JsonIgnore]
		public GridLength UdpRowHeight { get; set; }

		#endregion Properties

		#region Constructor

		public SerialConncetViewModel(int baudRate, string com, int rxPort, int txPort)
		{
			LoggerService.Inforamtion(this, "Starting SerialConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);
			IsUdpSimulationClickCommand = new RelayCommand(IsUdpSimulationClick);

			COM_DropDownOpenedCommand = new RelayCommand(COM_DropDownOpened);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;

			BaudratesList = new ObservableCollection<int>()
			{
				9600, 57600, 115200, 128000, 256000
			};

			FindCOMs();

			IsUdpSimulation = false;
			HandleSelectedAddapter();

			RxPort = rxPort;
			TxPort = txPort;
			SelectedCOM = com;
			SelectedBaudrate = baudRate;
			GetIpAddress();



			LoggerService.Inforamtion(this, "Ending Init of SerialConnctViewModel");
		}

		#endregion Constructor

		#region Methods

		private void FindCOMs()
		{
			string[] ports = SerialPort.GetPortNames();
			COMList = new ObservableCollection<string>(ports.ToList());
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

		private void Connect()
		{
			ConnectEvent?.Invoke();
		}

		private void Disconnect()
		{
			DisconnectEvent?.Invoke();
		}

		private void COM_DropDownOpened()
		{
			FindCOMs();
		}


		public void Copy(SerialConncetViewModel source)
		{
			SelectedCOM = source.SelectedCOM;
			SelectedBaudrate = source.SelectedBaudrate;
			Address = source.Address;
			RxPort = source.RxPort;
			TxPort = source.TxPort;
			IsUdpSimulation = source.IsUdpSimulation;
		}

		#endregion Methods

		#region Commands

		[JsonIgnore]
		public RelayCommand<SelectionChangedEventArgs> Addapter_SelectionChangedCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand ConnectCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand DisconnectCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand IsUdpSimulationClickCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand COM_DropDownOpenedCommand { get; private set; }


		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}
