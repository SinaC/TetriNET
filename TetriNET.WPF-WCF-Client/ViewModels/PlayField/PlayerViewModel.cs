using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class PlayerViewModel : ViewModelBase
    {
        public bool IsPlayerIdVisible => PlayerId != -1;

        public int DisplayPlayerId => PlayerId + 1;

        public bool IsPlayerInTeam => !string.IsNullOrWhiteSpace(Team);

        public string PlayerName => Client == null || !Client.IsRegistered ? "Not registered" : Client.Name;

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
                    OnPropertyChanged("PlayerName");
                }
            }
        }

        private bool _hasLost;
        public bool HasLost
        {
            get { return _hasLost; }
            set { Set(() => HasLost, ref _hasLost, value); }
        }

        public PlayerViewModel()
        {
            PlayerId = -1;
            HasLost = false;
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.PlayerUnregistered -= OnPlayerUnregistered;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.GameOver -= OnGameOver;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.PlayerTeamChanged -= OnPlayerTeamChanged;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.PlayerUnregistered += OnPlayerUnregistered;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.GameOver += OnGameOver;
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

        private void OnGameOver()
        {
            HasLost = true;
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            PlayerId = -1;
            HasLost = false;
            Team = "";
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
                PlayerId = playerId;
            HasLost = false;
        }

        private void OnPlayerUnregistered()
        {
            PlayerId = -1;
            HasLost = false;
            Team = "";
        }

        #endregion
    }

    public class PlayerViewModelDesignData : PlayerViewModel
    {
        public PlayerViewModelDesignData()
        {
            PlayerId = 5;
            Team = "LMA";
            HasLost = true;
        }
    }
}