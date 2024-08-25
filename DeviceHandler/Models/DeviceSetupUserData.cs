
using CommunityToolkit.Mvvm.ComponentModel;
using Entities.Enums;
using System.Collections.ObjectModel;

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

		

		
	}
}
