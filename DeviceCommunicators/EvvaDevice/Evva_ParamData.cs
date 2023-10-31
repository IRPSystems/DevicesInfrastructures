

using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.EvvaDevice
{
	public class Evva_ParamData:DeviceParameterData, IParamWithDropDown
	{
		public List<DropDownParamData> DropDown { get; set; }
	}
}
