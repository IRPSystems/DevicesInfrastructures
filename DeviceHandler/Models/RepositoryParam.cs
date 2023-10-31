
using DeviceCommunicators.Enums;
using DeviceHandler.Enums;
using Entities.Models;
using System;

namespace DeviceHandler.Models
{
	public class RepositoryParam
	{
		public DeviceParameterData Parameter { get; set; }
		public RepositoryPriorityEnum Priority { get; set; }
		public event Action<DeviceParameterData, CommunicatorResultEnum,string> ReceivedMessageEvent;

		public CommunicatorResultEnum IsReceived { get; set; }
		public string ErrDescription { get; set; }

		public int Counter { get; set; }

		public void RaisEvent(
			CommunicatorResultEnum result,
			string errDescription)
		{
			ReceivedMessageEvent?.Invoke(Parameter, result, errDescription);
		}
	}
}
