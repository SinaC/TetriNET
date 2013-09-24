using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ServerListViewModel
    {
        private readonly ObservableCollection<string> _servers = new ObservableCollection<string>();
        public ObservableCollection<string> Servers
        {
            get { return _servers; }
        }

        public string SelectedServer { get; set; }

        public event EventHandler<string> OnServerSelected;

        public ServerListViewModel()
        {
            ScanForServerCommand = new AsyncRelayCommand(async _ =>
                {
                    await Task.Run(() => ScanForServer());
                });
            SelectServerCommand = new RelayCommand(SelectServer);
        }

        private void ScanForServer()
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                List<string> servers = WCFProxy.WCFProxy.DiscoverHosts();
                ExecuteOnUIThread.Invoke(() =>
                    {
                        Servers.Clear();
                        if (servers == null || !servers.Any())
                            Servers.Add("No server found");
                        else
                            foreach (string s in servers)
                                Servers.Add(s);
                    });
            }
            catch
            {
                ExecuteOnUIThread.Invoke(() => Servers.Add("Error while scanning"));
            }
            finally
            {
                //Mouse.OverrideCursor = null;
            }
        }

        private void SelectServer()
        {
            if (!String.IsNullOrEmpty(SelectedServer) && OnServerSelected != null)
                OnServerSelected(this, SelectedServer);
        }

        #region Commands

        public ICommand ScanForServerCommand { get; set; }
        public ICommand SelectServerCommand { get; set; }

        #endregion
    }
}