using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
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
        private static readonly Brush Magenta = new SolidColorBrush(Colors.Magenta);
        private static readonly Brush Cyan = new SolidColorBrush(Colors.Cyan);
        private static readonly Brush White = new SolidColorBrush(Colors.White);
        private static readonly Brush PaleVioletRed = new SolidColorBrush(Colors.PaleVioletRed);
        private static readonly Brush LightSeaGreen = new SolidColorBrush(Colors.LightSeaGreen);
        private static readonly Brush Gold = new SolidColorBrush(Colors.Gold);
        private static readonly Brush MediumSlateBlue = new SolidColorBrush(Colors.MediumSlateBlue);
        private static readonly Brush LightGray = new SolidColorBrush(Colors.LightGray);
        private static readonly Brush DodgerBlue = new SolidColorBrush(Colors.DodgerBlue);
        private static readonly Brush DeepPink = new SolidColorBrush(Colors.DeepPink); // default value

        private static bool ApplicationIsInDesignMode
        {
            get { return (bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue); }
        }

        // ChatColor -> Brush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (ApplicationIsInDesignMode)
                return DeepPink;
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
                case ChatColor.Magenta:
                    return Magenta;
                case ChatColor.Cyan:
                    return Cyan;
                case ChatColor.White:
                    return White;
                case ChatColor.PaleVioletRed:
                    return PaleVioletRed;
                case ChatColor.LightSeaGreen:
                    return LightSeaGreen;
                case ChatColor.Gold:
                    return Gold;
                case ChatColor.MediumSlateBlue:
                    return MediumSlateBlue;
                case ChatColor.LightGray:
                    return LightGray;
                case ChatColor.DodgerBlue:
                    return DodgerBlue;
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