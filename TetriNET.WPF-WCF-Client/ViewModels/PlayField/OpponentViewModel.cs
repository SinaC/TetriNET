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

        public bool IsPlayerInTeam
        {
            get { return !string.IsNullOrWhiteSpace(Team); }
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
            oldClient.PlayerUnregistered -= OnPlayerUnregistered;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.PlayerLost -= OnPlayerLost;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.PlayerTeamChanged -= OnPlayerTeamChanged;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.PlayerUnregistered += OnPlayerUnregistered;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.PlayerLost += OnPlayerLost;
            newClient.GameStarted += OnGameStarted;
            newClient.PlayerTeamChanged += OnPlayerTeamChanged;
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

    public class OpponentViewModelDesignData : OpponentViewModel
    {
        public OpponentViewModelDesignData()
        {
            PlayerId = 2;
            Team = "LMA";
            HasLost = true;
        }
    }
}
