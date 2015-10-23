using System.Windows.Controls;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.Views.Achievements
{
    /// <summary>
    /// Interaction logic for AchievementsView.xaml
    /// </summary>
    public partial class AchievementsView : UserControl
    {
        public AchievementsView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
