
using DBCFileParser.Model;
using DeviceCommunicators.Models;
using System.Collections.ObjectModel;

namespace DeviceCommunicators.DBC
{
	public class DBC_ParamData: DeviceParameterData
	{
		public Signal Signal { get; set; }
		public Message ParentMessage { get; set; }
	}

	public class DBC_ParamGroup : DeviceParameterData
	{
		public uint ID { get; set; }

		public ObservableCollection<DBC_ParamData> ParamsList { get; set; }

		public Message Message { get; set; }

		public override string ToString()
		{
			return Name + " - " + ID;
		}
	}
}
