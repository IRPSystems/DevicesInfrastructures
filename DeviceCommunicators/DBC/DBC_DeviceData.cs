
using CommunityToolkit.Mvvm.Messaging;
using DBCFileParser.Model;
using DBCFileParser.Services;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using Entities.Enums;
using Services.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace DeviceCommunicators.DBC
{
	public class DBC_DeviceData: DeviceData
	{
		public string DBCFilePath { get; set; }
		public ObservableCollection<DBC_File> DBC_FilesList { get; set; }
		public override ObservableCollection<DeviceParameterData> ParemetersList { get; set; }

		public void DBCLoad(string dbcFilePath)
		{
			try
			{
				if (string.IsNullOrEmpty(dbcFilePath))
					return;

				if (File.Exists(dbcFilePath) == false)
				{
					MessageBox.Show($"The file {dbcFilePath} was not found", "Error");
					return;
				}


				var dbc = Parser.ParseFromPath(dbcFilePath);
				if (dbc == null)
					return;

				DBC_File dBC_File = new DBC_File()
				{
					Name = Path.GetFileName(dbcFilePath),
					FilePath = dbcFilePath,
					ParamsList = new ObservableCollection<DBC_ParamGroup>(),
				};

				DBC_FilesList.Add(dBC_File);

				foreach (Message message in dbc.Messages)
				{
					DBC_ParamGroup dbcGroup = new DBC_ParamGroup()
					{
						Name = message.Name,
						ID = message.ID,
						DeviceType = DeviceTypesEnum.DBC,
						ParamsList = new ObservableCollection<DBC_ParamData>()
					};
					dBC_File.ParamsList.Add(dbcGroup);

					foreach (Signal signal in message.Signals)
					{

						if (signal.Unit == "�C")
							signal.Unit = "˚C";
						else if (signal.Unit == "�")
							signal.Unit = "˚";

						DBC_ParamData dbcParam = new DBC_ParamData()
						{
							Name = signal.Name,
							Units = signal.Unit,
							Signal = signal,
							ParentMessage = message,
							DeviceType = DeviceTypesEnum.DBC,
						};

						dbcGroup.ParamsList.Add(dbcParam);
						ParemetersList.Add(dbcParam);
					}
				}


				


			}
			catch (Exception ex)
			{
				LoggerService.Error(this, "Failed to load DBC file", "Error", ex);
			}
		}
	}
}
