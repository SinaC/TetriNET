using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using TetriNET.WPF_WCF_Client.Models;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.Converters
{
    [ValueConversion(typeof(int), typeof(Brush))]
    public class PlayerIdToBrushConverter : IValueConverter
    {
        private readonly ChatColorBrushConverter _chatChatColorBrushConverter;

        public PlayerIdToBrushConverter()
        {
            _chatChatColorBrushConverter = new ChatColorBrushConverter();
        }

        private static bool ApplicationIsInDesignMode
        {
            get { return (bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue); }
        }

        // int -> Brush
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is int))
                throw new ArgumentException("value not of type int");
            int playerId = (int) value;
            ChatColor cc;
            if (ApplicationIsInDesignMode)
                cc = (ChatColor)(playerId+1); // no black
            else if (playerId == -1)
                cc = ChatColor.White;
            else if (playerId < 0 || playerId >= 6)
                throw new ArgumentException("value must be in [0,5]");
            else
                cc = ClientOptionsViewModel.Instance.PlayerColors[playerId];
            return _chatChatColorBrushConverter.Convert(cc, targetType, null, null);
        }

        // Brush -> int
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
