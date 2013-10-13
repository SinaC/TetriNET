
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

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

        public bool IsPlayerInTeam
        {
            get { return !string.IsNullOrWhiteSpace(Team); }
        }

        public bool IsColorSchemeUsed
        {
            get { return ClientOptionsViewModel.Instance == null || ClientOptionsViewModel.Instance.IsColorSchemeUsed; } // true if no instance (aka in designer mode)
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

        private string _team;
        public string Team
        {
            get { return _team; }
            set
            {
                if (_team != value)
                {
                    _team = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsPlayerInTeam");
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
            Team = "";
            HasLost = false;
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerLost -= OnPlayerLost;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnPlayerTeamChanged -= OnPlayerTeamChanged;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerLost += OnPlayerLost;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnPlayerTeamChanged += OnPlayerTeamChanged;
        }
        #endregion

        #region IClient events handler

        private void OnPlayerTeamChanged(int playerId, string team)
        {
            if (PlayerId == playerId)
                Team = team;
        }

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
            Team = "";
        }

        private void OnPlayerUnregistered()
        {
            PlayerId = -1;
            PlayerName = "Not playing";
            Team = "";
        }

        #endregion
    }
}
