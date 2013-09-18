using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TetriNET.WPF_WCF_Client.Models;

namespace TetriNET.WPF_WCF_Client.Views.Options
{
    public class KeyBox : TextBox
    {
        #region Fields/Properties
        public static readonly DependencyProperty KeySettingProperty = DependencyProperty.Register("KeySetting", typeof(KeySetting), typeof(KeyBox), new UIPropertyMetadata(KeySetting_Changed));

        public KeySetting KeySetting
        {
            get { return (KeySetting)GetValue(KeySettingProperty); }
            set { SetValue(KeySettingProperty, value); }
        }
        #endregion

        public KeyBox()
        {
            GotFocus += KeyBox_GotFocus;
            LostFocus += KeyBox_LostFocus;
            KeyDown += KeyBox_KeyDown;
            #region Set properties to specify the look of the control
            IsReadOnly = true;
            VerticalContentAlignment = VerticalAlignment.Center;
            #endregion
        }

        private static void KeySetting_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = (KeyBox)sender;
            __this.FontStyle = FontStyles.Normal;
            __this.Foreground = new SolidColorBrush(Colors.Black);
            __this.Text = __this.KeySetting.Key.ToString();
        }

        private void KeyBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.FontStyle = FontStyles.Italic;
            this.Foreground = new SolidColorBrush(Colors.Gray);
            this.Text = "Press the new key for this command.";
        }

        private void KeyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            this.FontStyle = FontStyles.Normal;
            this.Foreground = new SolidColorBrush(Colors.Black);
            this.Text = KeySetting.Key.ToString();
        }

        private void KeyBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //Changing the Key seems not to trigger the changed event, also every Property involved executes PropertyChanged...
            KeySetting = new KeySetting(e.Key, KeySetting.Command);
        }
    }
}
