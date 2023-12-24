﻿
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.BTMTempLogger;
using DeviceCommunicators.EvvaDevice;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
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


		public ObservableCollection<DeviceData> ReadAllFiles(
			string dir,
			string mcuFilePath,
			string mcuB2BFilePath,
			string dynoFilePath,
			string ni6002FilePath)
		{
			if (!Directory.Exists(dir))
				return null;

			ObservableCollection<DeviceData> devicesList = new ObservableCollection<DeviceData>();

			List<string> filesList = Directory.GetFiles(dir).ToList();

			foreach (string file in filesList)
			{
				
				string extension = System.IO.Path.GetExtension(file);
				if (extension.ToLower().EndsWith("xlsx"))
				{
					ReadFromExcel(file, devicesList);
				}
				else if(extension.ToLower().EndsWith("json"))
				{
					string path = file;
					if (Path.GetFileName(file) == "param_defaults.json")
						continue;

					else if (Path.GetFileName(file) == "Dyno Communication.json" && dynoFilePath != null)
						path = dynoFilePath;

					else if (Path.GetFileName(file) == "NI_6002.json" && ni6002FilePath != null)
						path = ni6002FilePath;

					ReadFromJson(path, devicesList);
				}
			}

			if (mcuFilePath != null)
			{
				ReadFromMCUJson(
					mcuFilePath,
					devicesList,
					"MCU",
					DeviceTypesEnum.MCU);
			}

			if (mcuB2BFilePath != null)
			{
				ReadFromMCUJson(
					mcuB2BFilePath,
					devicesList,
					"MCU - B2B",
					DeviceTypesEnum.MCU_B2B);
			}

			InitBTMTempLogger(devicesList);


			return devicesList;

		}

		public void ReadFromMCUJson(
			string path,
			ObservableCollection<DeviceData> devicesList,
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

		public void InitBTMTempLogger(ObservableCollection<DeviceData> devicesList)
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

		public void ReadFromJson(
			string path,
			ObservableCollection<DeviceData> devicesList)
		{
			FixJson(path);
			string jsonString = File.ReadAllText(path);

			JsonSerializerSettings settings = new JsonSerializerSettings();
			settings.Formatting = Formatting.Indented;
			settings.TypeNameHandling = TypeNameHandling.All;
			DeviceData device = JsonConvert.DeserializeObject(jsonString, settings) as DeviceData;
			if (device == null)
				return;


			int index = -1;
			DeviceData existingDevice = devicesList.ToList().Find(
				(d) => (d as DeviceData).DeviceType == device.DeviceType);
			if (existingDevice != null)
			{
				index = devicesList.IndexOf(existingDevice);
				if (existingDevice != null)
					devicesList.Remove(existingDevice);
			}

			if (index >= 0)
				devicesList.Insert(index, device);
			else
				devicesList.Add(device);
			


			if (device is DeviceData deviceData)
			{
				foreach (DeviceParameterData data in deviceData.ParemetersList)
					data.Device = device;
			}

		}

		

		public void ReadFromExcel(
			string path,
			ObservableCollection<DeviceData> devicesList)
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



		private void FixJson(string filePath)
		{
			string fileData = null;
			using (StreamReader sr = new StreamReader(filePath))
			{
				fileData = sr.ReadToEnd();
			}

			fileData = fileData.Replace(
				"Entities.Models.DeviceData, Entities",
				"DeviceCommunicators.Models.DeviceData, DeviceCommunicators");

			fileData = fileData.Replace(
				"Entities.Models.DeviceParameterData, Entities",
				"DeviceCommunicators.Models.DeviceParameterData, DeviceCommunicators");

			using (StreamWriter sw = new StreamWriter(filePath))
			{
				sw.Write(fileData);
			}



		}	
	}
}
