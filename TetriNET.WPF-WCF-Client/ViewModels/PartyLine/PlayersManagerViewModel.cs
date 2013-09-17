using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public class PlayerData : INotifyPropertyChanged
    {
        public int RealPlayerId { get; set; }

        public int DisplayPlayerId
        {
            get { return RealPlayerId + 1; }
        }

        public string PlayerName { get; set; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class PlayersManagerViewModel : ViewModelBase
    {
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

        public PlayersManagerViewModel()
        {
            KickPlayerCommand = new RelayCommand(KickPlayer);
            BanPlayerCommand = new RelayCommand(BanPlayer);
        }

        private void SetServerMaster(int serverMasterId)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                // TODO: how could we update visibility in UI ??? OnPropertyChanged doesn't work
                foreach (PlayerData p in PlayerList)
                    p.IsServerMaster = p.RealPlayerId == serverMasterId;
                //OnPropertyChanged("PlayerList");
            });
        }

        private void ClearEntries()
        {
            ExecuteOnUIThread.Invoke(() => PlayerList.Clear());
        }

        private void AddEntry(int playerId, string playerName)
        {
            // TODO: sort: http://msdn.microsoft.com/en-us/library/ms742542.aspx
            ExecuteOnUIThread.Invoke(() => PlayerList.Add(new PlayerData
            {
                RealPlayerId = playerId,
                PlayerName = playerName,
                IsServerMaster = false,
            }));
        }

        private void DeleteEntry(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                PlayerData p = PlayerList.FirstOrDefault(x => x.RealPlayerId == playerId);
                if (p != null)
                    PlayerList.Remove(p);
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Trying to delete unknown player {0}[{1}] from player list", playerId, playerName);
            });
        }

        private void KickPlayer()
        {
            if (SelectedPlayer != null && SelectedPlayer.RealPlayerId != Client.PlayerId)
                Client.KickPlayer(SelectedPlayer.RealPlayerId);
        }

        private void BanPlayer()
        {
            if (SelectedPlayer != null && SelectedPlayer.RealPlayerId != Client.PlayerId)
                Client.BanPlayer(SelectedPlayer.RealPlayerId);
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnPlayerJoined -= OnPlayerJoined;
            oldClient.OnPlayerLeft -= OnPlayerLeft;
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnPlayerJoined += OnPlayerJoined;
            newClient.OnPlayerLeft += OnPlayerLeft;
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnConnectionLost += OnConnectionLost;
        }
        #endregion

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ClearEntries();
            IsServerMaster = false;
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            SetServerMaster(serverMasterId);
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            AddEntry(playerId, playerName);
        }

        private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        {
            DeleteEntry(playerId, playerName);
        }

        private void OnPlayerUnregistered()
        {
            ClearEntries();
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            ClearEntries();
            AddEntry(playerId, Client.Name);
            IsServerMaster = Client.IsServerMaster;
        }
        #endregion

        #region Commands
        public ICommand KickPlayerCommand { get; set; }
        public ICommand BanPlayerCommand { get; set; }
        #endregion
    }
}
