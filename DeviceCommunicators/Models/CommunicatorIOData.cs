
using DeviceCommunicators.Enums;
using Entities.Models;
using System;

namespace DeviceCommunicators.Model
{
	public class CommunicatorIOData
	{
		public bool IsSet { get; set; }
		public DeviceParameterData Parameter { get; set; }
		public double Value { get; set; }
		public Action<DeviceParameterData, CommunicatorResultEnum, string> Callback { get; set; }
	}
}
