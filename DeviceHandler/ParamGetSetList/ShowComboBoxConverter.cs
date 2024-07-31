
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;
using DeviceCommunicators.MCU;
using Entities.Models;

namespace DeviceHandler.ParamGetSetList
{
    public class ShowComboBoxConverter : IValueConverter
    {

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
			if (!(value is IParamWithDropDown param))
				return Visibility.Visible;

			if (param.DropDown != null && param.DropDown.Count > 0)
				return Visibility.Visible;


			return Visibility.Collapsed;
		}

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
