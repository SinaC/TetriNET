using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.WinList
{
    /// <summary>
    /// Interaction logic for WinListView.xaml
    /// </summary>
    public partial class WinListView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("WinListViewClientProperty", typeof(IClient), typeof(WinListView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private bool _isServerMaster;
        public bool IsServerMaster
        {
            get { return _isServerMaster; }
            set
            {
                if (_isServerMaster != value)
                {
                    _isServerMaster = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly ObservableCollection<WinEntry> _winList = new ObservableCollection<WinEntry>();
        public ObservableCollection<WinEntry> WinList
        {
            get { return _winList; }
        }

        public WinListView()
        {
            InitializeComponent();
        }

        public void UpdateWinList(List<WinEntry> winList)
        {
            _winList.Clear();
            foreach(WinEntry entry in winList.OrderByDescending(x => x.Score))
                _winList.Add(entry);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            WinListView _this = sender as WinListView;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnServerMasterModified -= _this.OnServerMasterModified;
                    oldClient.OnWinListModified -= _this.OnWinListModified;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnServerMasterModified += _this.OnServerMasterModified;
                    newClient.OnWinListModified += _this.OnWinListModified;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                }
            }
        }

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            IsServerMaster = false;
        }

        private void OnWinListModified(List<WinEntry> winList)
        {
            ExecuteOnUIThread.Invoke(() => UpdateWinList(winList));
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            IsServerMaster = Client.IsServerMaster;
        }
        #endregion

        #region UI events handler
        private void ResetWinList_OnClick(object sender, RoutedEventArgs e)
        {
            Client.ResetWinList();
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
