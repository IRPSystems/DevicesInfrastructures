
using DBCFileParser.Model;
using DeviceCommunicators.Models;
using System.Collections.ObjectModel;

namespace DeviceCommunicators.DBC
{
	public class DBC_ParamData: DeviceParameterData
	{
		public ushort StartBit { get; set; }
		public ushort Length { get; set; }

		public double Factor { get; set; } = 1;
		public double Offset { get; set; }

		public double Minimum { get; set; }
		public double Maximum { get; set; }

		public Signal Signal { get; set; }
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
