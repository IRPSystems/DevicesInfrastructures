
using DeviceCommunicators.Models;

namespace DeviceHandler.Models.DeviceFullDataModels
{
	public class DeviceFullData_NI_6002_2 : DeviceFullData_NI_6002
	{
		public DeviceFullData_NI_6002_2(DeviceData deviceData) :
			base(deviceData)
		{

		}

		protected override string GetConnectionFileName()
		{
			return "NI_6002_2Connect.json";
		}
		
	}
}
