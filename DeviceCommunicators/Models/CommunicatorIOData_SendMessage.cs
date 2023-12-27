

namespace DeviceCommunicators.Models
{
	public class CommunicatorIOData_SendMessage: CommunicatorIOData
	{
		public bool IsExtented;
		public uint ID;
		public byte[] Buffer;
	}
}
