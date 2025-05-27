
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;
using DeviceCommunicators.Models;
using Services.Services;
using System.Windows.Controls;

namespace DeviceHandler.Converters
{
	public class AutomationIdBuilderConverte : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is FrameworkElement frameworkElement))
				return null;

			string controlName = frameworkElement.Name;

			object dataContext = frameworkElement.DataContext;
			if (frameworkElement.DataContext == null)
			{
				FrameworkElement parent = GetParament(frameworkElement);
				if(parent == null)
					return controlName;

				dataContext = parent.DataContext;
			}

			if (!(dataContext is DeviceParameterData param))
				return controlName;

			return $"{param.Name}_{controlName}";
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}


		private FrameworkElement GetParament(FrameworkElement frameworkElement)
		{
			FrameworkElement parent =
					FindAncestorService.FindAncestor<DataGridCell>(frameworkElement);
			if(parent != null) 
				return parent;

			parent =
					FindAncestorService.FindAncestor<ListViewItem>(frameworkElement);
			if (parent != null)
				return parent;

			parent =
					FindAncestorService.FindAncestor<TreeViewItem>(frameworkElement);
			if (parent != null)
				return parent;

			return null;
		}
	}
}
