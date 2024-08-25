
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Enums;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace DeviceHandler.Models
{
	public class DeviceSetupUserData : ObservableObject
	{
		
		public string MCUJsonPath { get; set; }
		public string MCUB2BJsonPath { get; set; }
		public string DynoCommunicationPath { get; set; }
		public string NI6002CommunicationPath { get; set; }

		public string YokoConfigFilePath { get; set; }

		public ObservableCollection<DeviceTypesEnum> SetupDevicesList { get; set; }

		public string LastSetupPath { get; set; }
		public string LastParamsDBCPath { get; set; }

		

		public static DeviceSetupUserData LoadDeviceSetupUserData(string dirName)
		{

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				return new DeviceSetupUserData();
			}
			path = Path.Combine(path, "DeviceSetupUserData.json");
			if (File.Exists(path) == false)
			{
				return new DeviceSetupUserData();
			}


			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceSetupUserData deviceSetupUserData = JsonConvert.DeserializeObject(jsonString, settings) as DeviceSetupUserData;
			if (deviceSetupUserData == null)
				return deviceSetupUserData;
			
			
			string errorDesc = string.Empty;
			if (File.Exists(deviceSetupUserData.DynoCommunicationPath) == false)
			{
				errorDesc += "The path \"" + deviceSetupUserData.DynoCommunicationPath + "\" was not found.\r\n\r\n";
				deviceSetupUserData.DynoCommunicationPath = "Data\\Device Communications\\Dyno Communication.json";
			}
			if (File.Exists(deviceSetupUserData.MCUJsonPath) == false)
			{
				errorDesc += "The path \"" + deviceSetupUserData.MCUJsonPath + "\" was not found.\r\n\r\n";
				deviceSetupUserData.MCUJsonPath = "Data\\Device Communications\\param_defaults.json";
			}
			if (File.Exists(deviceSetupUserData.MCUB2BJsonPath) == false)
			{
				errorDesc += "The path \"" + deviceSetupUserData.MCUB2BJsonPath + "\" was not found.\r\n\r\n";
				deviceSetupUserData.MCUB2BJsonPath = "Data\\Device Communications\\param_defaults.json";
			}
			if (File.Exists(deviceSetupUserData.NI6002CommunicationPath) == false)
			{
				errorDesc += "The path \"" + deviceSetupUserData.NI6002CommunicationPath + "\" was not found.\r\n\r\n";
				deviceSetupUserData.NI6002CommunicationPath = "Data\\Device Communications\\NI_6002.json";
			}

			if (string.IsNullOrEmpty(errorDesc) == false)
			{
				errorDesc += "The default paths will be used";
				LoggerService.Error(deviceSetupUserData, errorDesc, "Error");
			}


			return deviceSetupUserData;
		}



		public static void SaveDeviceSetupUserData(
			string dirName,
			DeviceSetupUserData deviceSetupUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			path = Path.Combine(path, "DeviceSetupUserData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(deviceSetupUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
