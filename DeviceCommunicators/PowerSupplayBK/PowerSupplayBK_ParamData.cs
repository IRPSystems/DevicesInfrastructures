
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.PowerSupplayBK
{
	public class PowerSupplayBK_ParamData: DeviceParameterData, IParamWithDropDown
	{
		public string Command { set; get; }
		public int TypeCommand { set; get; } // 0 - not return  result , Number- How many parameters return from the power supply
											 // 
		public List<DropDownParamData> DropDown { get; set; }
	}
}
