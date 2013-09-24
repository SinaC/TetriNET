using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TetriNET.WPF_WCF_Client.Models;

namespace TetriNET.WPF_WCF_Client.Converters
{
    [ValueConversion(typeof (ChatColor), typeof (Brush))]
    public class ChatColorBrushConverter : IValueConverter
    {
        private static readonly Brush Black = new SolidColorBrush(Colors.Black);
        private static readonly Brush Blue = new SolidColorBrush(Colors.Blue);
        private static readonly Brush Green = new SolidColorBrush(Colors.Green);
        private static readonly Brush Orange = new SolidColorBrush(Colors.Orange);
        private static readonly Brush Red = new SolidColorBrush(Colors.Red);
        private static readonly Brush Yellow = new SolidColorBrush(Colors.Yellow);
        private static readonly Brush DeepPink = new SolidColorBrush(Colors.DeepPink); // default value

        // ChatColor -> Brush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ChatColor))
                throw new ArgumentException("value not of type ChatColor");
            ChatColor cc = (ChatColor) value;
            switch (cc)
            {
                case ChatColor.Black:
                    return Black;
                case ChatColor.Blue:
                    return Blue;
                case ChatColor.Green:
                    return Green;
                case ChatColor.Orange:
                    return Orange;
                case ChatColor.Red:
                    return Red;
                case ChatColor.Yellow:
                    return Yellow;
                default:
                    return DeepPink; // default value
            }
        }

        // Brush -> ChatColor
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}