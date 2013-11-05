using System.Windows;
using System.Windows.Media;

//http://stackoverflow.com/questions/10293236/accessing-the-scrollviewer-of-a-listbox-from-c-sharp

namespace TetriNET.WPF_WCF_Client.Helpers
{
    public static class VisualTree
    {
        public static T GetDescendantByType<T>(Visual element) where T : class
        {
            if (element == null)
                return default(T);
            if (element.GetType() == typeof (T))
                return element as T;
            T foundElement = null;
            if (element is FrameworkElement)
                (element as FrameworkElement).ApplyTemplate();
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = GetDescendantByType<T>(visual);
                if (foundElement != null)
                    break;
            }
            return foundElement;
        }

        public static TAncestor FindAncestor<TAncestor>(DependencyObject current) where TAncestor : DependencyObject
        {
            do
            {
                if (current is TAncestor)
                    return current as TAncestor;
                current = VisualTreeHelper.GetParent(current);
            } while (current != null);
            return null;
        }
    }
}