using System;
using System.Windows.Data;

namespace Tetris.Converter
{
    class SoundVolumeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var volume = value as double?;
            return volume != null ? System.Convert.ToInt32(volume * 100) : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
