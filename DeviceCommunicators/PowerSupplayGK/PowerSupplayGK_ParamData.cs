
using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.PowerSupplayGK
{
	public class PowerSupplayGK_ParamData : DeviceParameterData, IParamWithDropDown
	{
		public ushort ReadAddress { get; set; }
		public ushort WriteAddress { get; set; }
		public ushort WriteTriggerAddress { get; set; }
		public double Scale { get; set; }

		public List<DropDownParamData> DropDown { get; set; }
	}
}
