
using Entities.Models;
using System.Windows;

namespace DeviceCommunicators.Models
{
	public class Dyno_ParamData: DeviceParameterData
	{
		public const int BaseUniqueParamID = 24575;
		public const byte SetFirstByte = 0x23;
		public const byte GetFirstByte = 0x40;
		public const byte ResponseGetFirstByte = 0x4B;

		public int Index { get; set; }
		public byte SubIndex { get; set; }
		public string Description { get; set; }
		public double Coefficient { get; set; }


		

		public Dyno_ParamData()
		{
			GetSetVisibility = Visibility.Visible;
		}
	}
}
