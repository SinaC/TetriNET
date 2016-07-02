using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class OpponentViewModel : ViewModelBase
    {
        public bool IsPlayerIdVisible => PlayerId != -1;

        public int DisplayPlayerId => PlayerId + 1;

        public bool IsPlayerInTeam => !string.IsNullOrWhiteSpace(Team);

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set { Set(() => PlayerName, ref _playerName, value); }
        }

        private string _team;
        public string Team
        {
            get { return _team; }
            set
            {
                if (Set(() => Team, ref _team, value))
                    OnPropertyChanged("IsPlayerInTeam");
            }
        }

        private int _playerId;
        public int PlayerId
        {
            get { return _playerId; }
            set
            {
                if (Set(() => PlayerId, ref _playerId, value))
                {
                    OnPropertyChanged("DisplayPlayerId");
                    OnPropertyChanged("IsPlayerIdVisible");
                }
            }
        }

        private bool _hasLost;
        public bool HasLost
        {
            get { return _hasLost; }
            set { Set(() => HasLost, ref _hasLost, value); }
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
