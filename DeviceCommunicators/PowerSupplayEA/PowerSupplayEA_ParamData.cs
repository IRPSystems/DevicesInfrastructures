﻿
using DeviceCommunicators.Enums;
using DeviceCommunicators.Interfaces;
using DeviceCommunicators.Models;
using Entities.Models;
using System.Collections.Generic;

namespace DeviceCommunicators.PowerSupplayEA
{
	public class PowerSupplayEA_ParamData : DeviceParameterData, IPSParamData
	{
		public string Cmd { get; set; }
		public int Max { get; set; }   
		public ParamTypeEnum ParamType { get; set; }

		public ushort ModbusAddress { get; set; }
		public ushort NumOfRegisters { get; set; }
		public string ValueType { get; set; }

		public List<DropDownParamData> DropDown { get; set; }
	}
}
