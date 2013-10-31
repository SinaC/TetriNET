using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.UserControls
{
    public class KeyBoxControl : TextBox
    {
        public KeyBoxControl()
        {
            GotFocus += KeyBox_GotFocus;
            LostFocus += KeyBox_LostFocus;
            PreviewKeyDown += OnPreviewKeyDown;
            DataContextChanged += OnDataContextChanged;

            //IsReadOnly = true;
            VerticalContentAlignment = VerticalAlignment.Center;
        }

        private void DisplayKey()
        {
            FontStyle = FontStyles.Normal;
            Foreground = new SolidColorBrush(Colors.Black);
            KeySettingViewModel keySetting = DataContext as KeySettingViewModel;
            if (keySetting != null)
                Text = keySetting.KeyDescription;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            DisplayKey(); // Needed because PropertyChanged doesn't work on Key :/
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            keyEventArgs.Handled = true;
            KeySettingViewModel keySetting = DataContext as KeySettingViewModel;
            if (keySetting != null)
                keySetting.Key = keyEventArgs.Key;
            DisplayKey(); // Needed because PropertyChanged doesn't work on Key :/
        }

        private void KeyBox_GotFocus(object sender, RoutedEventArgs e)
        {
            FontStyle = FontStyles.Italic;
            Foreground = new SolidColorBrush(Colors.Gray);
            Text = "Press the new key for this command.";
        }

        private void KeyBox_LostFocus(object sender, RoutedEventArgs e)
        {
            DisplayKey();
        }
    }
}
