
namespace DeviceHandler.Models
{
	public class SETTINGS_UPDATEDMessage
	{
		public bool IsMCUJsonPathChanged { get; set; }
		public bool IsMCUB2BJsonPathChanged { get; set; }
		public bool IsDynoJsonPathChanged { get; set; }
		public bool IsNI6002JsonPathChanged { get; set; }

		public bool IsMotorCommandsPathChanged { get; set; }
		public bool IsControllerCommandsPathChanged { get; set; }

		public string MotorCommandsPath { get; set; }
		public string ControllerCommandsPath { get; set; }
	}
}
