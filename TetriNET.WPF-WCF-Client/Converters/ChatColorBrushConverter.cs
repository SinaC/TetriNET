using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TetriNET.WPF_WCF_Client.ViewModels.PartyLine;

namespace TetriNET.WPF_WCF_Client.Converters
{
    [ValueConversion(typeof(ChatColor), typeof(Brush))]
    public class ChatColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ChatColor))
                throw new ArgumentException("value not of type ChatColor");
            ChatColor cc = (ChatColor)value;
            switch (cc)
            {
                case ChatColor.Black:
                    return new SolidColorBrush(Colors.Black);
                case ChatColor.Blue:
                    return new SolidColorBrush(Colors.Blue);
                case ChatColor.Green:
                    return new SolidColorBrush(Colors.Green);
                case ChatColor.Orange:
                    return new SolidColorBrush(Colors.Orange);
                case ChatColor.Red:
                    return new SolidColorBrush(Colors.Red);
                case ChatColor.Yellow:
                    return new SolidColorBrush(Colors.Yellow);
                default:
                    return new SolidColorBrush(Colors.DeepPink); // default value
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
