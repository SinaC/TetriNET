using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for ServerList.xaml
    /// </summary>
    public partial class ServerList : UserControl
    {
        private readonly ObservableCollection<string> _servers = new ObservableCollection<string>();
        public ObservableCollection<string> Servers { get { return _servers; }}

        public event EventHandler<string> OnServerSelected;

        public ServerList()
        {
            InitializeComponent();
        }

        private void ScanForServers_OnClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Servers.Clear();
                List<string> servers = WCFProxy.WCFProxy.DiscoverHosts();
                if (servers == null || !servers.Any())
                    Servers.Add("No server found");
                else
                    foreach (string s in servers)
                        Servers.Add(s);
            }
            catch
            {
                Servers.Add("Error while scanning");
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void Server_OnDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;
            if (item != null)
            {
                string serverAddress = item.DataContext as string;
                if (!String.IsNullOrEmpty(serverAddress) && OnServerSelected != null)
                    OnServerSelected(this, serverAddress);
            }
        }
    }
}
