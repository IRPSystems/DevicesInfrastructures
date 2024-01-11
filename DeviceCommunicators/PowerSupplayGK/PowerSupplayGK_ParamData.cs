
using DeviceCommunicators.Models;

namespace DeviceCommunicators.PowerSupplayGK
{
	public class PowerSupplayGK_ParamData : DeviceParameterData
	{
		public ushort Address { get; set; }
		public ushort TriggerAddress { get; set; }
	}
}
