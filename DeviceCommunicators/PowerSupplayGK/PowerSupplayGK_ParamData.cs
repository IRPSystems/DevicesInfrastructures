
using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.PowerSupplayGK
{
	public class PowerSupplayGK_ParamData : DeviceParameterData, IParamWithDropDown
	{
		public string Cmd { get; set; }
		public int Max { get; set; }   
		public List<DropDownParamData> DropDown { get; set; }
	}
}
