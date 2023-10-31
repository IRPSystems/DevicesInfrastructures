
using DeviceSimulators.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace DeviceSimulators.Selectors
{
	public class SimulatorTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			FrameworkElement element = container as FrameworkElement;

			if (item is MCUSimulatorMainWindowViewModel)
				return element.FindResource("MCUSimulatorTemplate") as DataTemplate;
			else if (item is SwitchRelaySimulatorMainWindowViewModel)
				return element.FindResource("SwitchRelaySimulatorTemplate") as DataTemplate;
			else 
				return element.FindResource("DeviceSimulatorTemplate") as DataTemplate;

		}
	}
}
