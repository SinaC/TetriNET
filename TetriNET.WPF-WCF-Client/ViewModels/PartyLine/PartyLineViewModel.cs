using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Properties;
using TetriNET.WPF_WCF_Client.Validators;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public class PartyLineViewModel : ViewModelBase, ITabIndex, IDataErrorInfo
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

        public bool IsUpdateTeamEnabled
        {
            get { return _isRegistered && !_isGameStarted; }
        }

        public bool IsUpdateTeamButtonEnabled
        {
            get
            {
                string error = this["Team"];
                return String.IsNullOrWhiteSpace(error) && _isRegistered && !_isGameStarted;
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
                    UpdateEnabilityAndLabel();
                }
            }
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
            UpdateTeamCommand = new RelayCommand(UpdateTeam);

            Team = Settings.Default.Team;

            ClientChanged += OnClientChanged;
        }

        private void UpdateEnabilityAndLabel()
        {
            OnPropertyChanged("IsStartStopEnabled");
            OnPropertyChanged("IsPauseResumeEnabled");
            OnPropertyChanged("StartStopLabel");
            OnPropertyChanged("PauseResumeLabel");
            OnPropertyChanged("IsUpdateTeamEnabled");
            OnPropertyChanged("IsUpdateTeamButtonEnabled");
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

        private void UpdateTeam()
        {
            Settings.Default.Team = _team;
            Settings.Default.Save();
            Client.ChangeTeam(Team);
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
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.PlayerUnregistered -= OnPlayerUnregistered;
            oldClient.ServerMasterModified -= OnServerMasterModified;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.GamePaused -= OnGamePaused;
            oldClient.GameResumed -= OnGameResumed;
            oldClient.PlayerTeamChanged -= OnPlayerTeamChanged;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.ConnectionLost += OnConnectionLost;
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.PlayerUnregistered += OnPlayerUnregistered;
            newClient.ServerMasterModified += OnServerMasterModified;
            newClient.GameStarted += OnGameStarted;
            newClient.GameFinished += OnGameFinished;
            newClient.GamePaused += OnGamePaused;
            newClient.GameResumed += OnGameResumed;
            newClient.PlayerTeamChanged += OnPlayerTeamChanged;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerTeamChanged(int playerId, string team)
        {
            if (playerId == Client.PlayerId)
                Team = team;
        }

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

        private void OnGameFinished(GameStatistics statistics)
        {
            _isGamePaused = false;
            _isGameStarted = false;
            UpdateEnabilityAndLabel();
        }

        private void OnGameStarted()
        {
            _isGamePaused = false;
            _isGameStarted = true;
            UpdateEnabilityAndLabel();
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            _isServerMaster = Client.IsServerMaster;
            UpdateEnabilityAndLabel();
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, int playerId, bool isServerMaster)
        {
            _isRegistered = Client.IsRegistered;
            _isGameStarted = Client.IsGameStarted;
            _isServerMaster = Client.IsServerMaster;
            _isGamePaused = false;
            UpdateEnabilityAndLabel();

            //Client.ChangeTeam(Team);
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

        public ICommand StartStopCommand { get; private set; }
        public ICommand PauseResumeCommand { get; private set; }
        public ICommand UpdateTeamCommand { get; private set; }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                if (columnName == "Team")
                {
                    StringValidationRule rule = new StringValidationRule
                        {
                            FieldName = columnName,
                            NullAccepted = true
                        };
                    ValidationResult result = rule.Validate(Team, CultureInfo.InvariantCulture);
                    return (string) result.ErrorContent;
                }
                return null;
            }
        }

        public string Error
        {
            get { return String.Empty; }
        }

        #endregion
    }

    public class PartyLineViewModelDesignData : PartyLineViewModel
    {
        public PartyLineViewModelDesignData()
        {
            ChatViewModel = new ChatViewModelDesignData();
            PlayersManagerViewModel = new PlayersManagerViewModelDesignData();
        }
    }
}