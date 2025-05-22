
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceHandler.Plots
{
	public class RegisterBitData: ObservableObject
	{

		public string Name { get; set; }

		public string HexValue { get; set; }

		public string DecValue { get; set; }

		public int Index { get; set; }

		public bool? IsOn { get; set; }

		public bool IsVisible { get; set; }
	}
}
