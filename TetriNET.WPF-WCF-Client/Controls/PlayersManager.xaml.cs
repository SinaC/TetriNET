using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Controls
{
    public class PlayerData
    {
        public int RealPlayerId { get; set; }

        public int DisplayPlayerId
        {
            get { return RealPlayerId + 1; }
        }

        public string PlayerName { get; set; }
        public Visibility IsServerMaster { get; set; }
    }

    /// <summary>
    /// Interaction logic for Players.xaml
    /// </summary>
    public partial class PlayersManager : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("WinListViewClientProperty", typeof(IClient), typeof(PlayersManager), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly ObservableCollection<PlayerData> _playerList = new ObservableCollection<PlayerData>();
        public ObservableCollection<PlayerData> PlayerList { get { return _playerList; } }

        public PlayerData SelectedPlayer { get; set; }

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

        public PlayersManager()
        {
            InitializeComponent();
        }

        private void SetServerMaster(int serverMasterId)
        {
            // TODO: how could we update visibility in UI ??? OnPropertyChanged doesn't work
            foreach(PlayerData p in _playerList)
                p.IsServerMaster = p.RealPlayerId == serverMasterId ? Visibility.Visible : Visibility.Hidden;
            //OnPropertyChanged("PlayerList");
        }

        private void AddEntry(int playerId, string playerName)
        {
            // TODO: sort: http://msdn.microsoft.com/en-us/library/ms742542.aspx
            _playerList.Add(new PlayerData
            {
                RealPlayerId = playerId,
                PlayerName = playerName,
                IsServerMaster = Visibility.Hidden,
            });
        }

        private void DeleteEntry(int playerId, string playerName)
        {
            PlayerData p = _playerList.FirstOrDefault(x => x.RealPlayerId == playerId);
            if (p != null)
                _playerList.Remove(p);
            else
                Log.WriteLine(Log.LogLevels.Warning, "Trying to delete unknown player {0}[{1}] from player list", playerId, playerName);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PlayersManager _this = sender as PlayersManager;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnServerMasterModified -= _this.OnServerMasterModified;
                    oldClient.OnPlayerJoined -= _this.OnPlayerJoined;
                    oldClient.OnPlayerLeft -= _this.OnPlayerLeft;
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnServerMasterModified += _this.OnServerMasterModified;
                    newClient.OnPlayerJoined += _this.OnPlayerJoined;
                    newClient.OnPlayerLeft += _this.OnPlayerLeft;
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                }
            }
        }

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ExecuteOnUIThread.Invoke(() => _playerList.Clear());
            IsServerMaster = false;
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            ExecuteOnUIThread.Invoke(() => SetServerMaster(serverMasterId));
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(playerId, playerName));
        }

        private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        {
            ExecuteOnUIThread.Invoke(() => DeleteEntry(playerId, playerName));
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(playerId, Client.Name));
        }
        #endregion

        #region UI events handler
        private void KickPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedPlayer != null && SelectedPlayer.RealPlayerId != Client.PlayerId)
                Client.KickPlayer(SelectedPlayer.RealPlayerId);
        }

        private void BanPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedPlayer != null && SelectedPlayer.RealPlayerId != Client.PlayerId)
                Client.BanPlayer(SelectedPlayer.RealPlayerId);
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
