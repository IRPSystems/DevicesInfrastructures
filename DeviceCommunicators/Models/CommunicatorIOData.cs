
using DeviceCommunicators.Enums;
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


		public int SendCounter;
		public byte[] SendBuffer;
		public uint SendId;
		public System.Timers.Timer SendTimoutTimer;

		public DateTime SendStartTime;


		public CommunicatorIOData() 
		{
			SendTimoutTimer = new System.Timers.Timer(1000);
			SendTimoutTimer.Elapsed += SendTimoutTimer_Elapsed;
		}

		private void SendTimoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			TimeoutEvent?.Invoke(this);
		}

		public event Action<CommunicatorIOData> TimeoutEvent;
	}
}
