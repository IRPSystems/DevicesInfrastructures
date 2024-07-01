
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using System.Collections.ObjectModel;

namespace DeviceCommunicators.DBC
{
	public class DBC_DeviceData: DeviceData
	{
		public string DBCFilePath { get; set; }
		public ObservableCollection<DBC_File> DBC_FilesList { get; set; }
		public override ObservableCollection<DeviceParameterData> ParemetersList { get; set; }
	}
}
