using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Connection;

namespace TetriNET.WPF_WCF_Client.Views.Connection
{
    /// <summary>
    /// Interaction logic for ServerListControl.xaml
    /// </summary>
    public partial class ServerListView : UserControl
    {
        public ServerListView()
        {
            InitializeComponent();
        }

        private void ServerList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ServerListViewModel vm = DataContext as ServerListViewModel;
            vm.Do(x => x.SelectServerCommand.Execute(null));
        }

        private void LatestServerList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ServerListViewModel vm = DataContext as ServerListViewModel;
            vm.Do(x => x.SelectLatestServerCommand.Execute(null));
        }
    }
}
