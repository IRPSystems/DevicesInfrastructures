
namespace DeviceHandler.Models
{
	public class SETTINGS_UPDATEDMessage
	{
		public bool IsMCUJsonPathChanged { get; set; }
		public bool IsMCUB2BJsonPathChanged { get; set; }
		public bool IsDynoJsonPathChanged { get; set; }
		public bool IsNI6002JsonPathChanged { get; set; }
	}
}
