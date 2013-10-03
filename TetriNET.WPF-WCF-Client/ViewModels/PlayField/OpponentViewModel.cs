
using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class OpponentViewModel : ViewModelBase
    {
        public bool IsPlayerIdVisible
        {
            get { return PlayerId != -1; }
        }

        public int DisplayPlayerId
        {
            get { return PlayerId + 1; }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _playerId;
        public int PlayerId
        {
            get { return _playerId; }
            set
            {
                if (_playerId != value)
                {
                    _playerId = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayPlayerId");
                    OnPropertyChanged("IsPlayerIdVisible");
                }
            }
        }

        private bool _hasLost;
        public bool HasLost
        {
            get { return _hasLost; }
            set
            {
                if (_hasLost != value)
                {
                    _hasLost = value;
                    OnPropertyChanged();
                }
            }
        }

        public OpponentViewModel()
        {
            PlayerId = -1;
            PlayerName = "Not playing";
            HasLost = false;
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerLost -= OnPlayerLost;
            oldClient.OnGameStarted -= OnGameStarted;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerLost += OnPlayerLost;
            newClient.OnGameStarted += OnGameStarted;
        }
        #endregion

        #region IClient events handler

        private void OnGameStarted()
        {
            HasLost = false;
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            if (playerId == PlayerId)
                HasLost = true;
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            PlayerId = -1;
            PlayerName = "Not playing";
        }

        private void OnPlayerUnregistered()
        {
            PlayerId = -1;
            PlayerName = "Not playing";
        }

        #endregion
    }
}
