
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Net;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Controls;
using System.Windows;
using System;
using Newtonsoft.Json;
using Services.Services;
using DeviceHandler.Interfaces;
using Communication.Services;
using System.Globalization;

namespace DeviceHandler.ViewModels
{
	public class CanConnectViewModel : ObservableObject, IConnectionViewModel
	{
		#region Properties

		[JsonIgnore]
		public ObservableCollection<string> HwIDsList { get; set; }
		[JsonIgnore]
		public ObservableCollection<int> BaudratesList { get; set; }
		[JsonIgnore]
		public ObservableCollection<string> AdaptersList { get; set; }

		public string SelectedHwId { get; set; }
		public int SelectedBaudrate { get; set; }
		public string SelectedAdapter { get; set; }

		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		public uint SyncNodeID { get; set; }
		public uint AsyncNodeID { get; set; }

		public int RxPort { get; set; }
		public int TxPort { get; set; }
		public string Address { get; set; }

		[JsonIgnore]
		public GridLength UdpRowHeight { get; set; }

		#endregion Properties

		#region Constructor

		public CanConnectViewModel(
			int baudRate,
			uint syncId,
			uint asyncId,
			int rxPort,
			int txPort)
		{
			LoggerService.Inforamtion(this, "Starting CanConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;


			BuildBuatrateList();
			BuildAdaptersList();
			GetIpAddress();

			HwIDsList = CanPCanService.GetHwIDs();

			UdpRowHeight = new GridLength(0);

			SelectedBaudrate = baudRate;
			SelectedAdapter = "PCAN";

			SyncNodeID = syncId;
			AsyncNodeID = asyncId;

			RxPort = rxPort;
			TxPort = txPort;

			Addapter_SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(Addapter_SelectionChanged);
			HWID_DropDownOpenedCommand = new RelayCommand(HWID_DropDownOpened);

			LoggerService.Inforamtion(this, "Ending Init of CanConnctViewModel");
		}

		#endregion Constructor

		#region Methods

		public void RefreshProperties()
		{
			HandleSelectedAddapter();
		}

		private void BuildBuatrateList()
		{
			BaudratesList = new ObservableCollection<int>()
			{
				1000000,
				800000,
				500000,
				250000,
				125000,
				100000,
			};
		}

		private void BuildAdaptersList()
		{
			AdaptersList = new ObservableCollection<string>()
			{
				"PCAN",
				"Ixxat",
				"Sloki",
				"UDP Simulator",
			};
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

		public ushort GetSelectedHWId(string selectedHwId)
		{
			if (string.IsNullOrEmpty(selectedHwId))
				return 0;

			int index = selectedHwId.IndexOf("(");
			if (index < 0)
				return 0;

			selectedHwId = selectedHwId.Substring(index + 1);

			index = selectedHwId.IndexOf(")");
			if (index < 0)
				return 0;

			selectedHwId = selectedHwId.Substring(0, index);
			selectedHwId = selectedHwId.Trim('h');

			ushort hwId;
			bool ret = ushort.TryParse(selectedHwId, NumberStyles.HexNumber, null, out hwId);

			return hwId;
		}

		private void HWID_DropDownOpened()
		{
			HwIDsList = CanPCanService.GetHwIDs();
		}

		private void Addapter_SelectionChanged(SelectionChangedEventArgs e)
		{
			HandleSelectedAddapter();
		}

		private void HandleSelectedAddapter()
		{
			if (SelectedAdapter.StartsWith("UDP"))
				UdpRowHeight = new GridLength(40);
			else
				UdpRowHeight = new GridLength(0);
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
		public RelayCommand<SelectionChangedEventArgs> Addapter_SelectionChangedCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand HWID_DropDownOpenedCommand { get; private set; }
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