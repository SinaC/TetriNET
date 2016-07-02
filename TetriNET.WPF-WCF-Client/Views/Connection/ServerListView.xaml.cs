﻿using System.Windows.Controls;
using System.Windows.Input;
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
            vm?.SelectServerCommand.Execute(null);
        }

        private void LatestServerList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            ServerListViewModel vm = DataContext as ServerListViewModel;
            vm?.SelectLatestServerCommand.Execute(null);
        }
    }
}
