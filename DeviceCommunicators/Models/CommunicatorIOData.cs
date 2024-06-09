
using DeviceCommunicators.Enums;
using DeviceCommunicators.MCU;
using DeviceCommunicators.Models;
using System;

namespace DeviceCommunicators.Models
{
	public class CommunicatorIOData
	{
		public bool IsSet { get; set; }
		public DeviceParameterData Parameter { get; set; }
		public double Value { get; set; }
		public Action<DeviceParameterData, CommunicatorResultEnum, string> Callback { get; set; }

		#region For async communication

		public int SendCounter;
		public byte[] SendBuffer;
		public uint SendId;
		public System.Timers.Timer SendTimoutTimer;

		public DateTime SendStartTime;

		#endregion For async communication


		public CommunicatorIOData() 
		{
			SendTimoutTimer = new System.Timers.Timer(MCU_Communicator.GetResponsesTimeout);
			SendTimoutTimer.Elapsed += SendTimoutTimer_Elapsed;
		}

		private void SendTimoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			TimeoutEvent?.Invoke(this);
		}

		public event Action<CommunicatorIOData> TimeoutEvent;
	}
}
