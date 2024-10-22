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

		//private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		//{
		//	if (DataContext is FaultsMCUViewModel faultsMCUViewModel)
		//	{
		//		faultsMCUViewModel.ClosingCommand?.Execute(null);
		//	}

		//	e.Cancel = true;
		//	Hide();
		//}

		//private void MetroWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
		//{
		//	if (DataContext is FaultsMCUViewModel faultsMCUViewModel)
		//	{
		//		faultsMCUViewModel.LoadedCommand?.Execute(null);
		//	}
		//}
    }
}
