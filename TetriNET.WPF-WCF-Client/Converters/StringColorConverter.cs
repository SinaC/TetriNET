using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TetriNET.WPF_WCF_Client.Converters
{
    [ValueConversion(typeof(Color), typeof(String))]
    public class StringColorConverter : IValueConverter
    {
        #region Implementation of IValueConverter

        // Color -> String
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Color))
                return new ArgumentException("value not of type Color");
            return value.ToString();
        }

        // String -> Color
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is String))
                return new ArgumentException("value not of type String");
            return ColorConverter.ConvertFromString(value as string);
        }

        #endregion
    }
}
