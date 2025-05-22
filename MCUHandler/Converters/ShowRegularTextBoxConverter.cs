
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;
using DeviceCommunicators.MCU;
using Entities.Models;

namespace DeviceHandler.Converters
{
    public class ShowRegularTextBoxConverter : IValueConverter
    {

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MCU_ParamData param &&
				string.IsNullOrEmpty(param.Format) == false && param.Format.ToLower() == "x")
            {
                return Visibility.Collapsed;
            }

            else if (value is IParamWithDropDown dropDown &&
				dropDown.DropDown != null && dropDown.DropDown.Count > 0)
            {
                return Visibility.Collapsed;
            }

			return Visibility.Visible;
		}

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
