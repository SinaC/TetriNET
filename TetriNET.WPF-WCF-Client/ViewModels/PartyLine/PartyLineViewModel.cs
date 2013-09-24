using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public class PartyLineViewModel : ViewModelBase, ITabIndex
    {
        public ChatViewModel ChatViewModel { get; set; }
        public PlayersManagerViewModel PlayersManagerViewModel { get; set; }

        private bool _isRegistered;
        private bool _isServerMaster;
        private bool _isGameStarted;
        private bool _isGamePaused;

        public bool IsStartStopEnabled
        {
            get { return _isRegistered && _isServerMaster; }
        }

        public bool IsPauseResumeEnabled
        {
            get { return _isRegistered && _isServerMaster && (_isGameStarted || _isGamePaused); }
        }

        public string StartStopLabel
        {
            get { return _isGameStarted || _isGamePaused ? "Stop game" : "Start game"; }
        }

        public string PauseResumeLabel
        {
            get { return _isGamePaused ? "Resume game" : "Pause game"; }
        }

        public PartyLineViewModel()
        {
            _isServerMaster = false;
            _isGameStarted = false;
            _isGamePaused = false;

            ChatViewModel = new ChatViewModel();
            PlayersManagerViewModel = new PlayersManagerViewModel();

            StartStopCommand = new RelayCommand(StartStop);
            PauseResumeCommand = new RelayCommand(PauseResume);

            ClientChanged += OnClientChanged;
        }

        private void UpdateEnabilityAndLabel()
        {
            OnPropertyChanged("IsStartStopEnabled");
            OnPropertyChanged("IsPauseResumeEnabled");
            OnPropertyChanged("StartStopLabel");
            OnPropertyChanged("PauseResumeLabel");
        }

        private void StartStop()
        {
            if (Client.IsGameStarted)
                Client.StopGame();
            else
                Client.StartGame();
            UpdateEnabilityAndLabel();
        }

        private void PauseResume()
        {
            if (Client.IsGamePaused)
                Client.ResumeGame();
            else
                Client.PauseGame();
            UpdateEnabilityAndLabel();
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 3; }
        }

        #endregion

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            ChatViewModel.Client = newClient;
            PlayersManagerViewModel.Client = newClient;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGamePaused -= OnGamePaused;
            oldClient.OnGameResumed -= OnGameResumed;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGamePaused += OnGamePaused;
            newClient.OnGameResumed += OnGameResumed;
        }

        #endregion

        #region IClient events handler

        private void OnGameResumed()
        {
            _isGamePaused = false;
            UpdateEnabilityAndLabel();
        }

        private void OnGamePaused()
        {
            _isGamePaused = true;
            UpdateEnabilityAndLabel();
        }

        private void OnGameFinished()
        {
            _isGameStarted = false;
            UpdateEnabilityAndLabel();
        }

        private void OnGameStarted()
        {
            _isGameStarted = true;
            UpdateEnabilityAndLabel();
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            _isServerMaster = Client.IsServerMaster;
            UpdateEnabilityAndLabel();
        }

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            _isRegistered = Client.IsRegistered;
            _isGameStarted = Client.IsGameStarted;
            _isServerMaster = Client.IsServerMaster;
            _isGamePaused = false;
            UpdateEnabilityAndLabel();
        }

        private void OnPlayerUnregistered()
        {
            _isRegistered = Client.IsRegistered;
            _isGameStarted = Client.IsGameStarted;
            _isServerMaster = Client.IsServerMaster;
            _isGamePaused = false;
            UpdateEnabilityAndLabel();
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            _isRegistered = false;
            _isServerMaster = false;
            UpdateEnabilityAndLabel();
        }

        #endregion

        #region Commands

        public ICommand StartStopCommand { get; set; }
        public ICommand PauseResumeCommand { get; set; }

        #endregion
    }
}