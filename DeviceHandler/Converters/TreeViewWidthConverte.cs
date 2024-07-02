
using System.Globalization;
using System.Windows.Data;
using System;

namespace DeviceHandler.Converters
{
	public class TreeViewWidthConverte : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double dValue))
				return value;

			return dValue - 50;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
