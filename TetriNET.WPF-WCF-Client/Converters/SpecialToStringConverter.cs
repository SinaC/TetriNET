using System;
using System.Globalization;
using System.Windows.Data;
using TetriNET.Common.DataContracts;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Converters
{
    [ValueConversion(typeof(Specials), typeof(string))]
    public class SpecialToStringConverter : IValueConverter
    {
        // Specials -> string
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is Specials))
                throw new ArgumentException("value not of type Specials");
            Specials special = (Specials)value;
            return Mapper.MapSpecialToString(special);
        }

        // string -> Specials
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
