using System;
using System.Windows.Data;

namespace Tetris.Converter
{
    class VolumeValuesConverter : IMultiValueConverter 
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values[0];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return new object[] { value, value };
        }
    }
}
