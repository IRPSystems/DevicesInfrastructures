using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeviceHandler.Views
{
	/// <summary>
	/// Interaction logic for ErrorBalloonView.xaml
	/// </summary>
	public partial class ErrorBalloonView : MetroWindow
	{
		private System.Timers.Timer _timer;

		public ErrorBalloonView()
		{
			InitializeComponent();

			_timer = new System.Timers.Timer(5000);
			_timer.Elapsed += _timer_Elapsed;
		}

		private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			_timer.Stop();

			this.Dispatcher.Invoke(() =>
			{
				Close();
			});
		}

		private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
		{
			_timer.Start();
		}

		private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_timer.Stop();
		}
	}
}
