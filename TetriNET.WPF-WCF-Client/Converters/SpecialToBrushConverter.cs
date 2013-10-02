using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.Converters
{
    [ValueConversion(typeof(Specials), typeof(Brush))]
    class SpecialToBrushConverter : IValueConverter
    {
        // Specials -> brush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Specials))
                throw new ArgumentException("value not of type Specials");
            Specials special = (Specials)value;
            return TextureManager.TextureManager.TexturesSingleton.Instance.GetBigSpecial(special) ?? new SolidColorBrush(Colors.Pink);
        }

        // brush -> Specials
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
