using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Controls
{
    public class PlayerData
    {
        private int _playerId;
        public int PlayerId
        {
            get { return _playerId + 1; }
            set { _playerId = value; }
        }

        public string PlayerName { get; set; }
        public Visibility IsServerMaster { get; set; }
    }

    /// <summary>
    /// Interaction logic for Players.xaml
    /// </summary>
    public partial class PlayersManager : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("WinListViewClientProperty", typeof(IClient), typeof(PlayersManager), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly ObservableCollection<PlayerData> _playerList = new ObservableCollection<PlayerData>();
        public ObservableCollection<PlayerData> PlayerList { get { return _playerList; } }

        public PlayersManager()
        {
            InitializeComponent();
        }

        private void SetServerMaster(bool isServerMaster)
        {
            // TODO
        }

        private void AddEntry(int playerId, string playerName)
        {
            _playerList.Add(new PlayerData
            {
                PlayerId = playerId,
                PlayerName = playerName,
                IsServerMaster = Visibility.Hidden,
            });
        }

        private void DeleteEntry(int playerId, string playerName)
        {
            PlayerData p = _playerList.FirstOrDefault(x => x.PlayerId == playerId);
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
                }
            }
        }

        private void OnServerMasterModified(bool isServerMaster)
        {
            ExecuteOnUIThread.Invoke(() => SetServerMaster(isServerMaster)); // TODO: impossible to do
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(playerId, playerName));
        }

        private void OnPlayerLeft(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => DeleteEntry(playerId, playerName));
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(playerId, Client.Name));
        }

        private void KickPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void BanPlayer_OnClick(object sender, RoutedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
