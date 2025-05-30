﻿
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceHandler.Interfaces;
using Entities.Models;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.Management;
using System.Threading.Tasks;
using System.Threading;

namespace DeviceHandler.ViewModels
{
	public class PrinterTSCConncetViewModel : ObservableObject, IConnectionViewModel
	{
		[JsonIgnore]

		private ObservableCollection<string> DeviceList = new ObservableCollection<string>();

		public bool IsConnectButtonEnabled { get; set; }
		[JsonIgnore]
		public bool IsDisconnectButtonEnabled { get; set; }

		public string DeviceName { get; set; }

		public bool IsUdpSimulation { get; set; }

		public PrinterTSCConncetViewModel()
		{
			LoggerService.Inforamtion(this, "Starting PrinterTSCConncetViewModel");
			ConnectCommand = new RelayCommand(Connect);
			DisconnectCommand = new RelayCommand(Disconnect);

			IsConnectButtonEnabled = true;
			IsDisconnectButtonEnabled = false;

			BuildDetectedDevicesList();


			LoggerService.Inforamtion(this, "Ending Init of PrinterTSCConncetViewModel");
		}

		#region Methods

		private void BuildDetectedDevicesList()
		{
			// Set the query to retrieve printers from Device Manager

			string query = "SELECT * FROM Win32_Printer";

            // Set a timeout for the operation
            using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(4)))
            {
                try
                {
                    // Run the search in a separate task with the cancellation token
                    Task.Run(() =>
                    {
                        // Create a ManagementObjectSearcher with the query
                        using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                        {
                            // Perform the query and get the collection of printers
                            ManagementObjectCollection printers = searcher.Get();

                            // Iterate over the printers and add their names to the list, add only connected printers
                            foreach (ManagementObject printer in printers)
                            {
                                if (cts.Token.IsCancellationRequested)
                                {
                                    Console.WriteLine("Loop terminated due to timeout.");
                                    break;
                                }

                                string printerName = printer["Name"] as string;
                                if (printerName != null && printerName.Contains("TSC"))
                                {
                                    DeviceName = printerName;
                                    DeviceList.Add(printerName);
									break;
                                }
                            }
                        }
                    }, cts.Token).Wait(cts.Token); // Wait for the task to complete or be canceled
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Operation timed out and was canceled.");
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
