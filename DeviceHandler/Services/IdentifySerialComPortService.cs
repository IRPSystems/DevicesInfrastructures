
using System.Collections.Generic;
using System.IO.Ports;
using System.Management;
using System;
using Communication.Services;

namespace DeviceHandler.Services
{
	public class IdentifySerialComPortService
	{	

		public static string GetSerialPortNameByIdentifier(string identifier)
		{
			
			try
			{
				using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'"))
				{
					foreach (ManagementObject obj in searcher.Get())
					{
						string name = obj["Name"].ToString();
						string port = name.Substring(name.LastIndexOf("(COM")).Replace("(", "").Replace(")", "");
						
						if(name.Contains(identifier))
							return port;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("An error occurred while querying for WMI data: " + ex.Message);
			}

			return null;
		}

		public static string GetSerialPortNameFromDevice(
			int baudRate,
			string idCommand,
			string identifier)
		{
			if(string.IsNullOrEmpty(idCommand))
				return null;

			string[] comPortsList = SerialPort.GetPortNames();

			foreach (string comPort in comPortsList)
			{
				SerialService serialService = new SerialService(comPort, baudRate);
				serialService.Init(false);
				if (serialService.IsInitialized == false)
					continue;

				serialService.Send(idCommand);

				string response;
				serialService.Read(out response);
				serialService.Dispose();
				if (response == null)
					continue;

				if(response.Contains(identifier))
					return comPort;
			}

			return null;
		}

	}
}
