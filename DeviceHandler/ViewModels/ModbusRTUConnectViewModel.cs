
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Newtonsoft.Json;
using Services.Services;
using DeviceHandler.Interfaces;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;

namespace DeviceHandler.ViewModels
{
	public class ModbusRTUConnectViewModel : ObservableObject, IConnectionViewModel
	{
		#region Properties

		public string ComPort { get; set; }
		public int Baudrate { get; set; }
		public byte ModbusAddress { get; set; }
		public ushort StartAddress { get; set; }
		public ushort NoOfItems { get; set; }
		public ushort SizeOfItems { get; set; }

		public bool IsUdpSimulation { get; set; }

		[JsonIgnore]
		public ObservableCollection<string> COMList { get; set; }
		[JsonIgnore]
		public ObservableCollection<int> BaudratesList { get; set; }


		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		

		

		#endregion Properties

		#region Constructor

		public ModbusRTUConnectViewModel(
			string comPort,
			int baudrate,
			byte modbusAddress,
			ushort startAddress = 0,
			ushort noOfItems = 0,
			ushort sizeOfItems = 0)
		{
			ComPort = comPort;
			Baudrate = baudrate;
			ModbusAddress = modbusAddress;
			StartAddress = startAddress;
			NoOfItems = noOfItems;
			SizeOfItems = sizeOfItems;

			LoggerService.Inforamtion(this, "Starting ModbusRTUConnectViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			COM_DropDownOpenedCommand = new RelayCommand(COM_DropDownOpened);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;

			FindCOMs();

			BaudratesList = new ObservableCollection<int>()
			{
				9600, 57600, 115200, 128000, 256000
			};


			LoggerService.Inforamtion(this, "Ending Init of ModbusRTUConnectViewModel");
		}

		#endregion Constructor

		#region Methods

		public void RefreshProperties()
		{
			
		}

		private void FindCOMs()
		{
			string[] ports = SerialPort.GetPortNames();
			COMList = new ObservableCollection<string>(ports.ToList());
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

		#endregion Methods

		#region Commands

		[JsonIgnore]
		public RelayCommand ConnectCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand DisconnectCommand { get; private set; }
		[JsonIgnore]
		public RelayCommand COM_DropDownOpenedCommand { get; private set; }

		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}