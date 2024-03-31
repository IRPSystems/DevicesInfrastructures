
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;

namespace DeviceHandler.ParamGetSetList
{
	public class ShowHexPerfixConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string str))
				return Visibility.Collapsed;

			if(str.ToLower() == "x")
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
