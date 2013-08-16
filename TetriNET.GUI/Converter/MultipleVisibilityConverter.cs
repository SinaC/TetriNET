using System;
using System.Linq;
using System.Windows.Data;
using System.Windows;

namespace Tetris.Converter
{
    class MultipleBoolToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(values.Any(v => (bool)v != false))
                return Visibility.Hidden;

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
