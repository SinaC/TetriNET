using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;
using TetriNET.Client.WCFProxy;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Messages;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ServerListViewModel : ObservableObject
    {
        private const int MaxLatestServerCount = 5;

        private readonly ObservableCollection<string> _servers = new ObservableCollection<string>();
        public ObservableCollection<string> Servers
        {
            get { return _servers; }
        }

        private readonly ObservableCollection<string> _latestServers = new ObservableCollection<string>();
        public ObservableCollection<string> LatestServers
        {
            get { return _latestServers; }
        }

        private bool _isScanForServerEnabled;
        public bool IsScanForServerEnabled
        {
            get { return _isScanForServerEnabled; }
            set { Set(() => IsScanForServerEnabled, ref _isScanForServerEnabled, value); }
        }

        private bool _isProgressBarVisible;
        public bool IsProgressBarVisible
        {
            get { return _isProgressBarVisible; }
            set { Set(() => IsProgressBarVisible, ref _isProgressBarVisible, value); }
        }

        private string _selectedServer;
        public string SelectedServer
        {
            get { return _selectedServer; }
            set { Set(() => SelectedServer, ref _selectedServer, value); }
        }

        private string _selectedLatestServer;
        public string SelectedLatestServer
        {
            get { return _selectedLatestServer; }
            set { Set(() => SelectedLatestServer, ref _selectedLatestServer, value); }
        }

        public ServerListViewModel()
        {
            IsProgressBarVisible = false;
            IsScanForServerEnabled = true;

            ScanForServerCommand = new AsyncRelayCommand(ScanForServer);
            //ScanForServerCommand = new AsyncRelayCommand2( async _ => await Task.Run(() => ScanForServer()));
            SelectServerCommand = new RelayCommand(SelectServer);
            SelectLatestServerCommand = new RelayCommand(SelectLatestServer);

            StringCollection latestServers = Settings.Default.LatestServers;
            if (latestServers != null)
            {
                _latestServers.Clear();
                foreach (string s in latestServers)
                    _latestServers.Add(s);
            }
        }

        public void AddServerToLatest(string address)
        {
            ExecuteOnUIThread.Invoke(() => AddServerToLatestInner(address));
        }

        private void AddServerToLatestInner(string address)
        {
            if (_latestServers.Any(x => x == address))
                return;
            _latestServers.Insert(0, address);
            if (_latestServers.Count > MaxLatestServerCount) // No more than 5 servers in list
                _latestServers.RemoveAt(MaxLatestServerCount);
            StringCollection latestServers = new StringCollection();
            latestServers.AddRange(_latestServers.ToArray());
            Settings.Default.LatestServers = latestServers;
            Settings.Default.Save();
        }

        private void ScanForServer()
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                IsScanForServerEnabled = false;
                IsProgressBarVisible = true;
                List<string> servers = WCFProxy.DiscoverHosts();
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
                IsProgressBarVisible = false;
                IsScanForServerEnabled = true;
                //Mouse.OverrideCursor = null;
            }
        }

        private void SelectServer()
        {
            if (!String.IsNullOrWhiteSpace(SelectedServer))
                Mediator.Send(new ServerSelectedMessage
                    {
                        ServerAddress = SelectedServer
                    });
        }

        private void SelectLatestServer()
        {
            if (!String.IsNullOrWhiteSpace(SelectedLatestServer))
                Mediator.Send(new ServerSelectedMessage
                {
                    ServerAddress = SelectedLatestServer
                });
        }

        #region Commands

        public ICommand ScanForServerCommand { get; private set; }
        public ICommand SelectServerCommand { get; private set; }
        public ICommand SelectLatestServerCommand { get; private set; }

        #endregion
    }

    public class ServerListViewModelDesignData : ServerListViewModel
    {
        public ServerListViewModelDesignData()
        {
        }
    }
}