
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Newtonsoft.Json;
using Services.Services;
using DeviceHandler.Interfaces;

namespace DeviceHandler.ViewModels
{
	public class ModbusTCPConnectViewModel : ObservableObject, IConnectionViewModel
	{
		#region Properties

		public string IPAddress { get; set; }
		public ushort Port { get; set; }
		public byte ModbusAddress { get; set; }
		public ushort StartAddress { get; set; }
		public ushort NoOfItems { get; set; }
		public ushort SizeOfItems { get; set; }

		public bool IsUdpSimulation { get; set; }


		[JsonIgnore]
		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		

		//public int RxPort { get; set; }
		//public int TxPort { get; set; }
		//public string Address { get; set; }

		

		#endregion Properties

		#region Constructor

		public ModbusTCPConnectViewModel(
			string idAddress,
			ushort port,
			byte modbusAddress,
			ushort startAddress,
			ushort moOfItems,
			ushort sizeOfItems)
		{
			IPAddress = idAddress;
			Port = port;
			ModbusAddress = modbusAddress;
			StartAddress = startAddress;
			NoOfItems = NoOfItems;
			SizeOfItems = sizeOfItems;

			LoggerService.Inforamtion(this, "Starting CanConnctViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;



			LoggerService.Inforamtion(this, "Ending Init of CanConnctViewModel");
		}

		#endregion Constructor

		#region Methods

		public void RefreshProperties()
		{
			
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

		#endregion Commands

		#region Events

		public event Action ConnectEvent;
		public event Action DisconnectEvent;

		#endregion Events
	}
}