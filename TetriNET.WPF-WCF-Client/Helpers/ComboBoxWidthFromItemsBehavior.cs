using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    //http://stackoverflow.com/questions/1034505/how-can-i-make-a-wpf-combo-box-have-the-width-of-its-widest-element-in-xaml
    public static class ComboBoxWidthFromItemsBehavior
    {
        public static readonly DependencyProperty ComboBoxWidthFromItemsProperty =
            DependencyProperty.RegisterAttached
            (
                "ComboBoxWidthFromItems",
                typeof(bool),
                typeof(ComboBoxWidthFromItemsBehavior),
                new UIPropertyMetadata(false, OnComboBoxWidthFromItemsPropertyChanged)
            );
        public static bool GetComboBoxWidthFromItems(DependencyObject obj)
        {
            return (bool)obj.GetValue(ComboBoxWidthFromItemsProperty);
        }
        public static void SetComboBoxWidthFromItems(DependencyObject obj, bool value)
        {
            obj.SetValue(ComboBoxWidthFromItemsProperty, value);
        }
        private static void OnComboBoxWidthFromItemsPropertyChanged(DependencyObject dpo,
                                                                    DependencyPropertyChangedEventArgs e)
        {
            ComboBox comboBox = dpo as ComboBox;
            if (comboBox != null)
            {
                if ((bool)e.NewValue)
                {
                    comboBox.Loaded += OnComboBoxLoaded;
                }
                else
                {
                    comboBox.Loaded -= OnComboBoxLoaded;
                }
            }
        }
        private static void OnComboBoxLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox != null)
            {
                Action action = comboBox.SetWidthFromItems;
                comboBox.Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
            }
        }
    }
}
