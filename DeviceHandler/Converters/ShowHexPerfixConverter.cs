
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;
using DeviceCommunicators.MCU;

namespace DeviceHandler.Converters
{
	public class ShowHexPerfixConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is string format))
				return Visibility.Collapsed;

			if (string.IsNullOrEmpty(format))
				return Visibility.Collapsed;

			if(format.ToLower() == "x")
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
