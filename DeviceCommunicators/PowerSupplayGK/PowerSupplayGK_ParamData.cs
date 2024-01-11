
using DeviceCommunicators.Models;

namespace DeviceCommunicators.PowerSupplayGK
{
	public class PowerSupplayGK_ParamData : DeviceParameterData
	{
		public ushort ReadAddress { get; set; }
		public ushort WriteAddress { get; set; }
		public ushort WriteTriggerAddress { get; set; }
	}
}
