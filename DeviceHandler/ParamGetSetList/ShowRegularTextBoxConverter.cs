
using System.Globalization;
using System.Windows.Data;
using System;
using System.Windows;
using DeviceCommunicators.MCU;

namespace DeviceHandler.ParamGetSetList
{
    public class ShowRegularTextBoxConverter : IValueConverter
    {

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is MCU_ParamData param))
                return Visibility.Visible;

            if (string.IsNullOrEmpty(param.Format) == false && param.Format.ToLower() == "x")
                return Visibility.Collapsed;

            if(param.DropDown == null || param.DropDown.Count == 0 )
                return Visibility.Visible;


			return Visibility.Collapsed;
		}

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
