
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;
using System;

namespace DeviceHandler.Converters
{
	public class BracketedUnitsConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is string units))
				return null;

			return "[" + units + "]";
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
