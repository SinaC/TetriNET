using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ServerListViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<string> _servers = new ObservableCollection<string>();
        public ObservableCollection<string> Servers
        {
            get { return _servers; }
        }

        private bool _isProgressBarVisible;
        public bool IsProgressBarVisible
        {
            get { return _isProgressBarVisible; }
            set
            {
                if (_isProgressBarVisible != value)
                {
                    _isProgressBarVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedServer { get; set; }

        public event EventHandler<string> OnServerSelected;

        public ServerListViewModel()
        {
            IsProgressBarVisible = false;

            ScanForServerCommand = new AsyncRelayCommand(ScanForServer);
            SelectServerCommand = new RelayCommand(SelectServer);
        }

        private void ScanForServer()
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                IsProgressBarVisible = true;
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
                IsProgressBarVisible = false;
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}