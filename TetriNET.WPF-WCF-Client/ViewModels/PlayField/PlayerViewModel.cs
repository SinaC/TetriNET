using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;

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

        public string PlayerName
        {
            get { return Client == null || !Client.IsRegistered ? "Not registered" : Client.Name; }
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
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnGameOver += OnGameOver;
            newClient.OnGameStarted += OnGameStarted;
        }

        #endregion

        #region IClient events handler

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
        }

        #endregion
    }
}