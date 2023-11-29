﻿
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.BTMTempLogger;
using DeviceCommunicators.EvvaDevice;
using DeviceCommunicators.MCU;
using Entities.Enums;
using Entities.Models;
using ExcelDataReader;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;

namespace DeviceCommunicators.Services
{
	public class ReadDevicesFileService: ObservableObject
	{


		public ObservableCollection<DeviceBase> ReadAllFiles(
			string dir,
			string mcuFilePath,
			string mcuB2BFilePath,
			string dynoFilePath,
			string ni6002FilePath)
		{
			if (!Directory.Exists(dir))
				return null;

			ObservableCollection<DeviceBase> devicesList = new ObservableCollection<DeviceBase>();

			List<string> filesList = Directory.GetFiles(dir).ToList();

			foreach (string file in filesList)
			{
				
				string extension = System.IO.Path.GetExtension(file);
				if (extension.ToLower().EndsWith("xlsx"))
				{
					Read(file, devicesList);
				}
				else if(extension.ToLower().EndsWith("json"))
				{
					if (Path.GetFileName(file) == "param_defaults.json")
						continue;

					ReadFromJson(file, devicesList);
				}
			}

			ReadFromMCUJson(
				mcuFilePath,
				devicesList,
				"MCU",
				DeviceTypesEnum.MCU);
			ReadFromMCUJson(
				mcuB2BFilePath,
				devicesList,
				"MCU - B2B",
				DeviceTypesEnum.MCU_B2B);

			//ReadFromDynoJson(
			//	dynoFilePath,
			//	devicesList);

			//ReadFromBKJson(
			//	dir,
			//	devicesList);

			//ReadFromEAJson(
			//	dir,
			//	devicesList);

			//ReadFromTKJson(
			//	dir,
			//	devicesList);

			//ReadFromSwitchRelay32Json(
			//	dir,
			//	devicesList);

			//InitBTMTempLogger(devicesList);

			//ReadFromWT1804EJson(
			//	dir,
			//	devicesList);

			//ReadFromNI6002Json(
			//	ni6002FilePath,
			//	devicesList);




			return devicesList;

		}

		public void ReadFromMCUJson(
			string path,
			ObservableCollection<DeviceBase> devicesList,
			string name, 
			DeviceTypesEnum deviceTypes)
		{
			MCU_DeviceData deviceData = devicesList.ToList().Find((d) => d.Name == name) as MCU_DeviceData;
			if (deviceData == null)
			{
				deviceData = new MCU_DeviceData(name, deviceTypes);
				devicesList.Add(deviceData);
			}

			MCU_ListHandlerService mcu_ListHandler = new MCU_ListHandlerService();

			

			mcu_ListHandler.ReadMCUDeviceData(path, deviceData);
		}

		public void ReadFromJson(
			string path,
			ObservableCollection<DeviceBase> devicesList)
		{
			//string path = Path.Combine(dir, "Power Supply BK.json");

			// Remove previouse PSBK
			int index = -1;
			DeviceBase dynoDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == DeviceTypesEnum.PowerSupplyBK);
			if (dynoDevice != null)
			{
				index = devicesList.IndexOf(dynoDevice);
				if (dynoDevice != null)
					devicesList.Remove(dynoDevice);
			}


			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
			if (device != null)
			{
				if (index >= 0)
					devicesList.Insert(index, device);
				else
					devicesList.Add(device);
			}

			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

		}

		public void ReadFromEAJson(
			string dir,
			ObservableCollection<DeviceBase> devicesList)
		{
			string path = Path.Combine(dir, "Power Supply EA.json");

			// Remove previouse PSEA
			int index = -1;
			DeviceBase dynoDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == DeviceTypesEnum.PowerSupplyEA);
			if (dynoDevice != null)
			{
				index = devicesList.IndexOf(dynoDevice);
				if (dynoDevice != null)
					devicesList.Remove(dynoDevice);
			}


			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
			if (device != null)
			{
				if (index >= 0)
					devicesList.Insert(index, device);
				else
					devicesList.Add(device);
			}

			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

		}


		public void ReadFromTKJson(
			string dir,
			ObservableCollection<DeviceBase> devicesList)
		{
			string path = Path.Combine(dir, "Torque Kistler.json");

			int index = -1;
			DeviceBase dynoDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == DeviceTypesEnum.TorqueKistler);
			if (dynoDevice != null)
			{
				index = devicesList.IndexOf(dynoDevice);
				if (dynoDevice != null)
					devicesList.Remove(dynoDevice);
			}


			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
			if (device != null)
			{
				if (index >= 0)
					devicesList.Insert(index, device);
				else
					devicesList.Add(device);
			}

			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

		}

		public void ReadFromSwitchRelay32Json(
			string dir,
			ObservableCollection<DeviceBase> devicesList)
		{
			string path = Path.Combine(dir, "SwitchRelay32.json");

			int index = -1;
			DeviceBase dynoDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == DeviceTypesEnum.SwitchRelay32);
			if (dynoDevice != null)
			{
				index = devicesList.IndexOf(dynoDevice);
				if (dynoDevice != null)
					devicesList.Remove(dynoDevice);
			}


			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
			if (device != null)
			{
				if (index >= 0)
					devicesList.Insert(index, device);
				else
					devicesList.Add(device);
			}

			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

		}


		public void InitBTMTempLogger(ObservableCollection<DeviceBase> devicesList)
		{
			DeviceData btmTempLogger = new DeviceData()
			{
				Name = "BTM Temp Logger",
				DeviceType = DeviceTypesEnum.BTMTempLogger,
			};

			btmTempLogger.ParemetersList = new ObservableCollection<DeviceParameterData>();
			for (int i = 1; i <= 12; i++)
			{
				DeviceParameterData param = new BTMTempLogger_ParamData()
				{
					Channel = i,
					Name = "Channel " + i,
					Units = "°C",
					DeviceType = DeviceTypesEnum.BTMTempLogger,
					Device = btmTempLogger,
				};

				btmTempLogger.ParemetersList.Add(param);
			}

			devicesList.Add(btmTempLogger);
		}




        public void ReadFromNI6002Json(
            string path,
            ObservableCollection<DeviceBase> devicesList)
        {

            int index = -1;
            DeviceBase dynoDevice = devicesList.ToList().Find(
                (d) => (d as DeviceData).DeviceType == DeviceTypesEnum.NI_6002);
            if (dynoDevice != null)
            {
                index = devicesList.IndexOf(dynoDevice);
                if (dynoDevice != null)
                    devicesList.Remove(dynoDevice);
            }


            string jsonString = File.ReadAllText(path);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Formatting = Formatting.Indented;
            settings.TypeNameHandling = TypeNameHandling.All;
            DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
            if (device != null)
            {
                if (index >= 0)
                    devicesList.Insert(index, device);
                else
                    devicesList.Add(device);
            }

            if (device is DeviceData deviceData)
            {
                foreach (DeviceParameterData data in deviceData.ParemetersList)
                    data.Device = device;
            }

        }






        public void ReadFromWT1804EJson(
			string dir,
			ObservableCollection<DeviceBase> devicesList)
		{
			string path = Path.Combine(dir, "YOKOGAWA WT1804E.json");

			int index = -1;
			DeviceBase dynoDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == DeviceTypesEnum.Yokogawa_WT1804E);
			if (dynoDevice != null)
			{
				index = devicesList.IndexOf(dynoDevice);
				if (dynoDevice != null)
					devicesList.Remove(dynoDevice);
			}


			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
			if (device != null)
			{
				if (index >= 0)
					devicesList.Insert(index, device);
				else
					devicesList.Add(device);
			}

			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

		}

		public void ReadFromDynoJson(
			string path,
			ObservableCollection<DeviceBase> devicesList)
		{
			if (File.Exists(path) == false)
			{
				LoggerService.Error(this, "File " + path + " not found");
				return;
			}

			// Remove previouse dyno
			int index = -1;
			DeviceBase dynoDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == DeviceTypesEnum.Dyno);
			if (dynoDevice != null)
			{
				index = devicesList.IndexOf(dynoDevice);
				if (dynoDevice != null)
					devicesList.Remove(dynoDevice);
			}


			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceBase device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceBase;
			if (device != null)
			{
				if(index >= 0) 
					devicesList.Insert(index, device);
				else
					devicesList.Add(device);
			}

			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

			
		}


		public void Read(
			string path,
			ObservableCollection<DeviceBase> devicesList)
		{

			IExcelDataReader reader;
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
			var stream = File.Open(path, FileMode.Open, FileAccess.Read);
			reader = ExcelReaderFactory.CreateReader(stream);

			//// reader.IsFirstRowAsColumnNames
			var conf = new ExcelDataSetConfiguration
			{
				ConfigureDataTable = _ => new ExcelDataTableConfiguration
				{
					UseHeaderRow = true
				}
			};

			var dataSet = reader.AsDataSet(conf);

			// Now you can get data from each sheet by its index or its "name"
			var dataTable = dataSet.Tables[0];

			int row = 0;
			for (; row < dataTable.Rows.Count; row++)
			{
				var v = dataTable.Rows[row][0];
				if (v != null && !string.IsNullOrEmpty(v.ToString()))
					break;
			}

			var deviceData = dataTable.Rows[row][0];
			string device = deviceData.ToString();

			var nameData = dataTable.Rows[row][1];
			string name = nameData.ToString();

			var typeData = dataTable.Rows[row][2];
			string typeStr = typeData.ToString();
			DeviceTypesEnum type;
			Enum.TryParse(typeStr, out type);


			DeviceData data = new DeviceData()
			{
				Name = deviceData + " " + name,
				DeviceType = type,
				ParemetersList = new ObservableCollection<DeviceParameterData>(),
				IsExpanded = true,
			};

			row++;

			for (; row < dataTable.Rows.Count; row++)
			{
				var v = dataTable.Rows[row][0];
				if (v != null && v.ToString() == "Name")
					break;
			}

			row++;

			ReadParameters(
				dataTable,
				ref row,
				data);



			reader.Close();

			devicesList.Add(data);
		}

		private void ReadParameters(
			DataTable dataTable,
			ref int row,
			DeviceData deviceData)
		{
			for (; row < dataTable.Rows.Count; row++)
			{
				var data = dataTable.Rows[row][0];
				if (data == null || string.IsNullOrEmpty(data.ToString()))
					break;

				DeviceParameterData parameter = new DeviceParameterData()
				{
					Name = data.ToString(),
					Device = deviceData,
					DeviceType = deviceData.DeviceType
				};

				deviceData.ParemetersList.Add(parameter);
			}
		}

		

	}
}
