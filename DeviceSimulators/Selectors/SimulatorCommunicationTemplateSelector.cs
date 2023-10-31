
using DeviceHandler.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DeviceSimulators.Selectors
{
	public class SimulatorCommunicationTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (item is CanConnectViewModel)
				return element.FindResource("CanCommTemplate") as DataTemplate;
			else if (item is SerialConncetViewModel)
				return element.FindResource("SerialCommTemplate") as DataTemplate;
			else if (item is TcpConncetViewModel)
				return element.FindResource("TcpCommTemplate") as DataTemplate;

			return null;
		}
	}
}
