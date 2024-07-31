
using DeviceCommunicators.Models;

namespace DeviceCommunicators.ZimmerPowerMeter
{
	public class ZimmerPowerMeter_ParamData: DeviceParameterData
	{
		public string Command { set; get; }
		public int Channel { set; get; }
	}
}
