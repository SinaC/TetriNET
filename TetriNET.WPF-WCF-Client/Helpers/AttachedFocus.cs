using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    //http://stackoverflow.com/questions/1356045/set-focus-on-textbox-in-wpf-from-view-model-c-wpf
    //http://zamjad.wordpress.com/2011/01/24/difference-between-coercevaluecallback-and-propertychangedcallback/
    //use CoerceValueCallback to ensure event is raised even if value is not modified
    public static class AttachedFocus
    {
        public static bool GetIsFocused(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFocusedProperty, value);
        }

        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached("IsFocused", typeof(bool), typeof(AttachedFocus), new PropertyMetadata(false, null, OnIsFocusedPropertyChanged));

        private static object OnIsFocusedPropertyChanged(DependencyObject d, object value)
        {
            var uie = (UIElement)d;
            if ((bool) value)
            {
                //uie.Focus(); // Don't care about false values.
                //Keyboard.Focus(uie);
                //FocusManager.SetFocusedElement(d, uie);
                ExecuteOnUIThread.InvokeAsync(() => Keyboard.Focus(uie), DispatcherPriority.ContextIdle);
            }
            return value;
        }
    }
}
