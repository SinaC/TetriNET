using System.ComponentModel;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.Views.Statistics
{
    /// <summary>
    /// Interaction logic for GameStatisticsView.xaml
    /// </summary>
    public partial class GameStatisticsView : UserControl
    {
        public GameStatisticsView()
        {
            InitializeComponent();
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            e.Column.Header = ((PropertyDescriptor)e.PropertyDescriptor).DisplayName;
        }
    }
}
