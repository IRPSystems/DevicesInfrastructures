
using CommunityToolkit.Mvvm.ComponentModel;
using DeviceCommunicators.Models;
using Newtonsoft.Json;
using Services.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace DeviceCommunicators.MCU
{
	public class MCU_ListHandlerService: ObservableObject
	{


		#region Methods

		public void ReadMCUDeviceData(
			string path,
			MCU_DeviceData mcu_device)
		{
			if(File.Exists(path) == false)
			{ 
				LoggerService.Error(this, "File " + path + " not found");
				return;
			}

			ReaMcuFullList(path, mcu_device);
		}

		public void ReaMcuFullList(
			string path,
			MCU_DeviceData mcu_device)
		{
			try
			{
				mcu_device.MCU_FullList = new ObservableCollection<DeviceParameterData>();

				string jsonString = File.ReadAllText(path);
				string extension = Path.GetExtension(path);
				if(extension.ToString() != ".json") // Encoded file, need decoding
				{
					jsonString = DecodFile(jsonString);
				}

				mcu_device.MCU_GroupList = JsonConvert.DeserializeObject<ObservableCollection<ParamGroup>>(jsonString);

				foreach (ParamGroup group in mcu_device.MCU_GroupList)
				{
					group.Name = group.GroupName;

					foreach (DeviceParameterData data in group.ParamList)
					{
						if (data.Units == "�C")
							data.Units = "˚C";
						else if (data.Units == "�")
							data.Units = "˚";


						if (data is MCU_ParamData mcu_Param)
							mcu_Param.GroupName = group.GroupName;

						MCU_ParamData existing = mcu_device.MCU_FullList.ToList().Find((p) =>
							IsSameName(((MCU_ParamData)p).Cmd, ((MCU_ParamData)data).Cmd)) as MCU_ParamData;
						if (existing != null)
						{
							int index = existing.Name.IndexOf("(");
							if (index < 0)
								existing.Name += " (" + existing.GroupName + ")";

							data.Name += " (" + group.GroupName + ")";

						}

						data.Device = mcu_device;
						data.DeviceType = mcu_device.DeviceType;

						mcu_device.MCU_FullList.Add(data);
					}
				}
			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to read the MCU parameters list", ex);

			}

		}

		private string DecodFile(string jsonString)
		{
			byte[] fileBytes = Encoding.ASCII.GetBytes(jsonString);

			string encodingPassword = "itay";
			fileBytes = EncryptDecrypt(fileBytes, encodingPassword);
			jsonString = Encoding.Default.GetString(fileBytes);

			return jsonString;
		}

		public byte[] EncryptDecrypt(byte[] data, string password)
		{
			byte[] Keys = Encoding.ASCII.GetBytes(password);

			for (int i = 0; i < data.Length; i++)
			{
				data[i] = (byte)(data[i] ^ Keys[i % Keys.Length]);
			}

			return data;
		}


		public static bool IsSameName(
			string listParam,
			string checkedParam)
		{
			int index = listParam.IndexOf("(");
			if (index < 0)
				return (listParam == checkedParam);


			string listParamName = listParam;
			listParamName = listParamName.Substring(0, index);
			listParamName = listParamName.Trim();

			return (listParamName == checkedParam);
		}



		private bool IsEqualForShortList(
			string param1,
			string param2)
		{
			string para1Name = param1;
			int index = para1Name.IndexOf("(");
			if(index >= 0 && para1Name[para1Name.Length - 1] == ')')
			{
				para1Name = para1Name.Substring(0, index);
				para1Name = para1Name.Trim();
			}

			string para2Name = param2;
			index = para2Name.IndexOf("(");
			if (index >= 0 && para2Name[para2Name.Length - 1] == ')')
			{
				para2Name = para2Name.Substring(0, index);
				para2Name = para2Name.Trim();
			}

			if (para1Name == para2Name)
				return true;

			return false;
		}

		#endregion Methods
	}
}
