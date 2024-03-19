
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;

namespace DeviceHandler.ViewModels
{
	public class SerialAndTCPViewModel: ObservableObject, IConnectionViewModel
	{
		#region Properties

		public SerialConncetViewModel SerialConncetVM { get; set; }
		public TcpConncetViewModel TcpConncetVM { get; set; }


		public GridLength SerialHeight { get; set; }
		public GridLength TCPHeight { get; set; }

		public List<string> CommTypesList { get; set; }
		public string SelectedCommType { get; set; }

		public bool IsConnectButtonEnabled 
		{
			get => _isConnectButtonEnabled;
			set
			{
				_isConnectButtonEnabled = value;
				if (SelectedCommType == "Serial")
					SerialConncetVM.IsConnectButtonEnabled = value;
				else
					TcpConncetVM.IsConnectButtonEnabled = value;
			}

		}

		public bool IsDisconnectButtonEnabled
		{
			get => _isDisconnectButtonEnabled;
			set
			{
				_isDisconnectButtonEnabled = value;
				if (SelectedCommType == "Serial")
					SerialConncetVM.IsDisconnectButtonEnabled = value;
				else
					TcpConncetVM.IsDisconnectButtonEnabled = value;
			}

		}

		#endregion Properties

		#region Fields

		private bool _isConnectButtonEnabled;
		private bool _isDisconnectButtonEnabled;

		#endregion Fields

		#region Constructor

		public SerialAndTCPViewModel(
			int baudRate, string com, int rxPort, int txPort,
			int port, string address,
			string selectedCommType)
		{

			ComType_SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(ComType_SelectionChanged);

			SerialConncetVM = new SerialConncetViewModel(baudRate, com, rxPort, txPort);
			SerialConncetVM.ConnectEvent += Connect;
			SerialConncetVM.DisconnectEvent += Disconnect;

			TcpConncetVM = new TcpConncetViewModel(port, rxPort, txPort);
			TcpConncetVM.ConnectEvent += Connect;
			TcpConncetVM.DisconnectEvent += Disconnect;
			TcpConncetVM.Address = address;

			CommTypesList = new List<string>()
			{
				"Serial", "TCP"
			};

			SelectedCommType = selectedCommType;

			ComType_SelectionChanged(null);
		}

		#endregion Constructor

		#region Methods


		private void ComType_SelectionChanged(SelectionChangedEventArgs e)
		{
			SerialHeight = new GridLength(0);
			TCPHeight = new GridLength(0);

			if (SelectedCommType == "Serial")
				SerialHeight = new GridLength(1, GridUnitType.Auto);
			else if (SelectedCommType == "TCP")
				TCPHeight = new GridLength(1, GridUnitType.Auto);

			OnPropertyChanged(nameof(SerialHeight));
			OnPropertyChanged(nameof(TCPHeight));
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

		public void Copy(SerialAndTCPViewModel source)
		{
			SerialConncetVM.Copy(source.SerialConncetVM);
			TcpConncetVM.Copy(source.TcpConncetVM);
			SelectedCommType = source.SelectedCommType;

			ComType_SelectionChanged(null);
		}

		#endregion Methods

		#region Commands

		[JsonIgnore]
		public RelayCommand<SelectionChangedEventArgs> ComType_SelectionChangedCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}
