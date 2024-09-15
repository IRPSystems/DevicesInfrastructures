using MahApps.Metro.Controls;

namespace DeviceHandler.Faults
{
	/// <summary>
	/// Interaction logic for FaultsMCUView.xaml
	/// </summary>
	public partial class FaultsMCUView : MetroWindow
	{
		public FaultsMCUView()
		{
			InitializeComponent();
		}

		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
			Hide();
		}
	}
}
