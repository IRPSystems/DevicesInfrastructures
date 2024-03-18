﻿
using DeviceCommunicators.Enums;
using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.PowerSupplayEA
{
	public class PowerSupplayEA_ParamData : DeviceParameterData, IParamWithDropDown
	{
		public string Cmd { get; set; }
		public int Max { get; set; }   
		public ParamTypeEnum ParamType { get; set; }
		public ushort ModbusAddress { get; set; }
		public List<DropDownParamData> DropDown { get; set; }
	}
}
