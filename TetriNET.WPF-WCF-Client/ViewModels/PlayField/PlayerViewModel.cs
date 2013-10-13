using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class PlayerViewModel : ViewModelBase
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

        public string PlayerName
        {
            get { return Client == null || !Client.IsRegistered ? "Not registered" : Client.Name; }
        }

        public bool IsColorSchemeUsed
        {
            get { return ClientOptionsViewModel.Instance == null || ClientOptionsViewModel.Instance.IsColorSchemeUsed; } // true if no instance (aka in designer mode)
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
                    OnPropertyChanged("PlayerName");
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

        public PlayerViewModel()
        {
            PlayerId = -1;
            HasLost = false;
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnPlayerTeamChanged -= OnPlayerTeamChanged;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnGameOver += OnGameOver;
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

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
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
}