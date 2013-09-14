using System.Windows;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.Views.Statistics
{
    /// <summary>
    /// Interaction logic for ClientStatisticsView.xaml
    /// </summary>
    public partial class ClientStatisticsView : UserControl
    {
        public ClientStatisticsView()
        {
            InitializeComponent();
        }

        #region UI events handler

        private void RefreshStatistics_OnClick(object sender, RoutedEventArgs e)
        {
            //Refresh(); TODO
        }

        #endregion
    }
}