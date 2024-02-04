
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.ATEBox
{
	public class ATEBox_ParamData: DeviceParameterData, IParamWithDropDown
	{
		public ATEBox_InterfaceTyleEnum InterfaceType { get; set; }
		public string Command { get; set; }
		public MCU_ParamData MCUParameter { get; set; }
		public int Channel { get; set; }
		public uint CANID { get; set; }

		public List<DropDownParamData> DropDown { get; set; }
	}
}
