
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
using System.Windows.Input;
using System.Collections.Generic;

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

		public uint? RequiredDeviceId { get; set; }

		//[JsonIgnore]
		//public string SyncNodeID_Text
		//{
		//	get => _syncNodeID_Text;
		//	set
		//	{
		//		_syncNodeID_Text = value;
		//	}
		//}

		public uint SyncNodeID 
		{
			get => _syncNodeID;
			set
			{
				_syncNodeID = value;
			}
		}

		public uint AsyncNodeID { get; set; }

		public int RxPort { get; set; }
		public int TxPort { get; set; }
		public string Address { get; set; }

		[JsonIgnore]
		public GridLength UdpRowHeight { get; set; }

		#endregion Properties

		#region Fields

		//private string _syncNodeID_Text;
		private uint _syncNodeID;
		

		#endregion Fields

		#region Constructor

		public CanConnectViewModel(
			int baudRate,
			uint syncId,
			uint asyncId,
			int rxPort,
			int txPort,
			uint? requiredDeviceId = null)
		{
			RequiredDeviceId = requiredDeviceId;

			LoggerService.Inforamtion(this, "Starting CanConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;


			BuildBuatrateList();
			BuildAdaptersList();
			GetIpAddress();

			HWID_DropDownOpened();

			UdpRowHeight = new GridLength(0);

			SelectedBaudrate = baudRate;
			SelectedAdapter = "PCAN";

			SyncNodeID = syncId;
			AsyncNodeID = asyncId;

			RxPort = rxPort;
			TxPort = txPort;

			Addapter_SelectionChangedCommand = new RelayCommand<SelectionChangedEventArgs>(Addapter_SelectionChanged);
			HWID_DropDownOpenedCommand = new RelayCommand(HWID_DropDownOpened);

			HexKeyDownEventCommand = new RelayCommand<KeyEventArgs>(HexKeyDownEvent);
			HexTextChangedEventCommand = new RelayCommand<TextChangedEventArgs>(HexTextChangedEvent);

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

			if (RequiredDeviceId == null)
				return;

			List<string> hwToRemove = new List<string>();
			foreach (string hwIdDesc in HwIDsList)
			{
				ushort hwId = CanPCanService.GetHWId(hwIdDesc);
				CanPCanService canPCan = new CanPCanService(
					SelectedBaudrate,
					hwId,
					0xAB,
					0xAB,
					0);

				uint deviceId = canPCan.GetDeviceId();
				if(deviceId != RequiredDeviceId)
					hwToRemove.Add(hwIdDesc);

				canPCan = null;
			}

			GC.Collect();

			foreach (string hwIdDesc in hwToRemove)
				HwIDsList.Remove(hwIdDesc);

			if (HwIDsList.Count == 0)
				LoggerService.Error(this, "No PCAN with the correct Device ID was found");
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

		private void HexKeyDownEvent(KeyEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (textBox.Text == "Msg Hex ID")
				return;

			if (textBox.CaretIndex < 3)
				return;


			if ((textBox.Text.Length + 1) > 3)
				e.Handled = true;
		}

		private void HexTextChangedEvent(TextChangedEventArgs e)
		{
			if (!(e.Source is TextBox textBox))
				return;

			if (textBox.Text == "Msg Hex ID")
				return;

			uint id;
			bool res = uint.TryParse(textBox.Text, System.Globalization.NumberStyles.HexNumber, null, out id);
			if (res)
			{
				if (id > 0x7FF)
				{
					MessageBox.Show($"The ID 0x{id.ToString("X")} is larger than the legal ID limit");
					e.Handled = true;
				}
			}
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

		[JsonIgnore]
		public RelayCommand<KeyEventArgs> HexKeyDownEventCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand<TextChangedEventArgs> HexTextChangedEventCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}