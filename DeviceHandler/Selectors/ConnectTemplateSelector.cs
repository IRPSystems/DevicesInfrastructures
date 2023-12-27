using DeviceHandler.ViewModels;
using System.Windows;
using System.Windows.Controls;


namespace DeviceHandler.Selectors
{
	public class ConnectTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;


			if (item is CanConnectViewModel)
				return element.FindResource("CanConnectTemplate") as DataTemplate;
			if (item is SerialConncetViewModel)
				return element.FindResource("SerialConnectTemplate") as DataTemplate;
			if (item is TcpConncetViewModel)
				return element.FindResource("TcpConnectTemplate") as DataTemplate;
			if (item is YokogawaWT1804EConncetViewModel)
				return element.FindResource("YokogawaWT1804EConnectTemplate") as DataTemplate;
			if (item is NI6002ConncetViewModel)
				return element.FindResource("NI6002ConnectTemplate") as DataTemplate;
			if (item is ModbusTCPConnectViewModel)
				return element.FindResource("ModbusTCPConnectTemplate") as DataTemplate;

			return null;
		}
	}
}
