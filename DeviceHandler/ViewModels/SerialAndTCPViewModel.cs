
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DeviceHandler.ViewModels
{
	public class SerialAndTCPViewModel: ObservableObject, IConnectionViewModel
	{
		#region Properties

		public SerialConncetViewModel SerialConncetVM { get; set; }
		public TcpConncetViewModel TcpConncetVM { get; set; }

		public Visibility SerialConncetVisibility { get; set; }
		public Visibility TcpConncetVisibility { get; set; }

		public List<string> CommTypesList { get; set; }
		public string SelectedCommType { get; set; }

		public bool IsConnectButtonEnabled { get; set; }
		public bool IsDisconnectButtonEnabled { get; set; }

		#endregion Properties

		#region Constructor

		public SerialAndTCPViewModel(
			int baudRate, string com, int rxPort, int txPort,
			int port, string address,
			string selectedCommType)
		{

			ComType_SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(ComType_SelectionChanged);
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			SerialConncetVM = new SerialConncetViewModel(baudRate, com, rxPort, txPort);
			TcpConncetVM = new TcpConncetViewModel(port, rxPort, txPort);
			TcpConncetVM.Address = address;

			CommTypesList = new List<string>()
			{
				"Serial", "TCP"
			};

			SelectedCommType = selectedCommType;
		}

		#endregion Constructor

		#region Methods

		private void ComType_SelectionChanged(SelectionChangedEventArgs e)
		{
			SerialConncetVisibility = Visibility.Collapsed;
			TcpConncetVisibility = Visibility.Collapsed;

			if(SelectedCommType == "Serial")
				SerialConncetVisibility = Visibility.Visible;
			else if (SelectedCommType == "TCP")
				TcpConncetVisibility = Visibility.Visible;
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
		public RelayCommand<SelectionChangedEventArgs> ComType_SelectionChangedCommand { get; private set; }
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
