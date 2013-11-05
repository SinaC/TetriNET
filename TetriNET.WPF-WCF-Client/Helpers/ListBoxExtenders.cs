using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace TetriNET.WPF_WCF_Client.Helpers
{
    /// <summary>
    /// This class contains a few useful extenders for the ListBox
    /// </summary>
    public class ListBoxExtenders : DependencyObject
    {
        #region AutoScroll to SelectedItem

        public static readonly DependencyProperty AutoScrollToSelectedItemProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToSelectedItem",
                typeof (bool),
                typeof (ListBoxExtenders),
                new UIPropertyMetadata(default(bool), OnAutoScrollToSelectedItemChanged));

        public static bool GetAutoScrollToSelectedItem(DependencyObject obj)
        {
            return (bool) obj.GetValue(AutoScrollToSelectedItemProperty);
        }

        public static void SetAutoScrollToSelectedItem(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToSelectedItemProperty, value);
        }

        public static void OnAutoScrollToSelectedItemChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var listBox = s as ListBox;
            if (listBox != null)
            {
                var scrollToSelectionHandler = new SelectionChangedEventHandler(
                    (sender, args) => ExecuteOnUIThread.InvokeAsync(() =>
                        {
                            //listBox.UpdateLayout();
                            listBox.Focus(); // set focus to display selected item in blue instead of gray because list is not focused
                            if (listBox.SelectedItem != null)
                                listBox.ScrollIntoView(listBox.SelectedItem);
                        }, DispatcherPriority.ContextIdle));

                if ((bool) e.NewValue)
                    listBox.SelectionChanged += scrollToSelectionHandler;
                else
                    listBox.SelectionChanged -= scrollToSelectionHandler;
            }
        }

        #endregion

        #region AutoScroll to end

        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof (bool),
                typeof (ListBoxExtenders),
                new UIPropertyMetadata(default(bool), OnAutoScrollToEndChanged));

        public static bool GetAutoScrollToEnd(DependencyObject obj)
        {
            return (bool) obj.GetValue(AutoScrollToEndProperty);
        }

        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollToEndProperty, value);
        }

        public static void OnAutoScrollToEndChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            var listBox = s as ListBox;
            if (listBox != null)
            {
                var listBoxItems = listBox.Items;
                var data = listBoxItems.SourceCollection as INotifyCollectionChanged;

                var scrollToEndHandler = new NotifyCollectionChangedEventHandler(
                    (s1, e1) =>
                        {
                            if (listBox.Items.Count > 0)
                            {
                                //object lastItem = listBox.Items[listBox.Items.Count - 1];
                                //listBoxItems.MoveCurrentTo(lastItem);
                                //listBox.ScrollIntoView(lastItem);
                                ExecuteOnUIThread.Invoke(() =>
                                    {
                                        ScrollViewer scrollViewer = VisualTree.GetDescendantByType<ScrollViewer>(listBox);
                                        scrollViewer.ScrollToEnd();
                                    });
                            }
                        });

                if (data != null)
                {
                    if ((bool) e.NewValue)
                        data.CollectionChanged += scrollToEndHandler;
                    else
                        data.CollectionChanged -= scrollToEndHandler;
                }
            }
        }

        #endregion
    }
}
