
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;
using DeviceCommunicators.MCU;

namespace DeviceHandler.ParamGetSetList
{
	public class ShowHexPerfixConverter : IValueConverter
	{

		object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is MCU_ParamData mcuPara))
				return Visibility.Collapsed;

			if (string.IsNullOrEmpty(mcuPara.Format))
				return Visibility.Collapsed;

			if(mcuPara.Format.ToLower() == "x")
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
