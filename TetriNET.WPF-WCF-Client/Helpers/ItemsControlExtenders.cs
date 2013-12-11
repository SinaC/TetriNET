using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public class ItemsControlExtenders : DependencyObject
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(ItemsControlExtenders),
                new UIPropertyMetadata(default(bool), OnAutoScrollToEndChanged));

        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollToEndProperty);
        }

        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        public static void OnAutoScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = s as ItemsControl;
            if (itemsControl != null)
            {
                var itemsControlItems = itemsControl.Items;
                var data = itemsControlItems.SourceCollection as INotifyCollectionChanged;

                var scrollToEndHandler = new NotifyCollectionChangedEventHandler(
                    (s1, e1) =>
                    {
                        if (itemsControl.Items.Count > 0)
                        {
                            ExecuteOnUIThread.Invoke(() =>
                            {
                                ScrollViewer scrollViewer = VisualTree.GetDescendantByType<ScrollViewer>(itemsControl);
                                if (scrollViewer != null)
                                    scrollViewer.ScrollToEnd();
                            });
                        }
                    });

                if (data != null)
                {
                    if ((bool)e.NewValue)
                        data.CollectionChanged += scrollToEndHandler;
                    else
                        data.CollectionChanged -= scrollToEndHandler;
                }
            }
        }
    }
}
