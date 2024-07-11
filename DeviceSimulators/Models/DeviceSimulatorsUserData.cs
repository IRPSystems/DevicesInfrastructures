
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DeviceSimulators.Models
{
	public class DeviceSimulatorsUserData: ObservableObject
	{
		
		public string DevicesFilesDir { get; set; }



		public static DeviceSimulatorsUserData LoadDeviceSimulatorsUserData(string dirName)
		{
			DeviceSimulatorsUserData deviceSimulatorsUserData = null;

			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
			{
				Directory.CreateDirectory(path);
			}
			path = Path.Combine(path, "DeviceSimulatorsUserData.json");
			if (File.Exists(path) == false)
			{
				return new DeviceSimulatorsUserData();
			}


			string jsonString = File.ReadAllText(path);
			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			deviceSimulatorsUserData = JsonConvert.DeserializeObject(jsonString, settings) as DeviceSimulatorsUserData;
			if (deviceSimulatorsUserData == null)
				return deviceSimulatorsUserData;

			return deviceSimulatorsUserData;
		}



		public static void SaveDeviceSimulatorsUserData(
			string dirName,
			DeviceSimulatorsUserData DeviceSimulatorsUserData)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			path = Path.Combine(path, dirName);
			if (Directory.Exists(path) == false)
				Directory.CreateDirectory(path);
			path = Path.Combine(path, "DeviceSimulatorsUserData.json");

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			var sz = JsonConvert.SerializeObject(DeviceSimulatorsUserData, settings);
			File.WriteAllText(path, sz);
		}
	}
}
