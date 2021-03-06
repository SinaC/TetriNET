﻿using TetriNET.Common.BlockingActionQueue;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.CustomSettings;
using TetriNET.WPF_WCF_Client.Properties;
using TetriNET.WPF_WCF_Client.ViewModels.Achievements;
using TetriNET.WPF_WCF_Client.ViewModels.Connection;
using TetriNET.WPF_WCF_Client.ViewModels.Options;
using TetriNET.WPF_WCF_Client.ViewModels.PartyLine;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;
using TetriNET.WPF_WCF_Client.ViewModels.Statistics;
using TetriNET.WPF_WCF_Client.ViewModels.WinList;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public WinListViewModel WinListViewModel { get; protected set; }
        public StatisticsViewModel StatisticsViewModel { get; protected set; }
        public OptionsViewModel OptionsViewModel { get; protected set; }
        public PartyLineViewModel PartyLineViewModel { get; protected set; }
        public ConnectionViewModel ConnectionViewModel { get; protected set; }
        public PlayFieldViewModelBase PlayFieldViewModel { get; protected set; }
        public AchievementsViewModel AchievementsViewModel { get; protected set; }

        protected PlayFieldViewModel PlayFieldPlayerViewModel { get; set; }
        protected PlayFieldSpectatorViewModel PlayFieldSpectatorViewModel { get; set; }

        private int _activeTabItemIndex;
        public int ActiveTabItemIndex
        {
            get { return _activeTabItemIndex; }
            set { Set(() => ActiveTabItemIndex, ref _activeTabItemIndex, value); }
        }

        private bool _isRegistered;
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set { Set(() => IsRegistered, ref _isRegistered, value); }
        }

        public MainWindowViewModel()
        {
            //
            IFactory factory = new Factory();

            //
            IActionQueue actionQueue = new BlockingActionQueue();

            //
            IAchievementManager manager = null;
            if (!IsInDesignMode)
            {
                //
                manager = factory.CreateAchievementManager();
                manager.FindAllAchievements();
                Settings.Default.Achievements = Settings.Default.Achievements ?? new AchievementsSettings();
                Settings.Default.Achievements.Load(manager.Achievements);
            }

            // Create sub view models
            WinListViewModel = new WinListViewModel();
            StatisticsViewModel = new StatisticsViewModel();
            OptionsViewModel = new OptionsViewModel();
            PartyLineViewModel = new PartyLineViewModel();
            ConnectionViewModel = new ConnectionViewModel();
            PlayFieldPlayerViewModel = new PlayFieldViewModel();
            PlayFieldSpectatorViewModel = new PlayFieldSpectatorViewModel();
            AchievementsViewModel = new AchievementsViewModel();
            PlayFieldViewModel = PlayFieldPlayerViewModel; // by default, player view

            //
            ConnectionViewModel.LoginViewModel.OnConnect += OnConnect;

            //
            ClientChanged += OnClientChanged;

            if (!IsInDesignMode)
            {
                // Create client
                Client = new Client.Client(factory, actionQueue, manager);
            }
        }

        private void OnConnect(ConnectEventArgs e)
        {
            if (e.IsSpectator)
                e.Success = Client.ConnectAndRegisterAsSpectator(ConnectionViewModel.LoginViewModel.ServerCompleteSpectatorAddress, ConnectionViewModel.LoginViewModel.Username);
            else
                e.Success = Client.ConnectAndRegisterAsPlayer(ConnectionViewModel.LoginViewModel.ServerCompletePlayerAddress, ConnectionViewModel.LoginViewModel.Username, PartyLineViewModel.Team);
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            WinListViewModel.Client = newClient;
            StatisticsViewModel.Client = newClient;
            OptionsViewModel.Client = newClient;
            PartyLineViewModel.Client = newClient;
            ConnectionViewModel.Client = newClient;
            PlayFieldPlayerViewModel.Client = newClient;
            PlayFieldSpectatorViewModel.Client = newClient;
            AchievementsViewModel.Client = newClient;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.RegisteredAsSpectator -= OnRegisteredAsSpectator;
            oldClient.PlayerUnregistered -= OnPlayerUnregisted;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.GameOver -= OnGameOver;
            oldClient.ConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.RegisteredAsSpectator += OnRegisteredAsSpectator;
            newClient.PlayerUnregistered += OnPlayerUnregisted;
            newClient.GameStarted += OnGameStarted;
            newClient.GameFinished += OnGameFinished;
            newClient.GameOver += OnGameOver;
            newClient.ConnectionLost += OnConnectionLost;
        }

        #endregion

        #region IClient events handler

        private void OnRegisteredAsPlayer(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                IsRegistered = true;
                PlayFieldViewModel = PlayFieldPlayerViewModel;
                OnPropertyChanged("PlayFieldViewModel");
                if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                    if (ActiveTabItemIndex == ConnectionViewModel.TabIndex)
                        ActiveTabItemIndex = PartyLineViewModel.TabIndex;
            }
        }

        private void OnRegisteredAsSpectator(RegistrationResults result, Versioning serverVersion, int spectatorId)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                IsRegistered = true;
                PlayFieldViewModel = PlayFieldSpectatorViewModel;
                OnPropertyChanged("PlayFieldViewModel");
                if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                    if (ActiveTabItemIndex == ConnectionViewModel.TabIndex)
                        ActiveTabItemIndex = PartyLineViewModel.TabIndex;
            }
        }

        private void OnPlayerUnregisted()
        {
            IsRegistered = false;
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        private void OnGameStarted()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ActiveTabItemIndex = PlayFieldViewModel.TabIndex;
        }

        private void OnGameFinished(GameStatistics statistics)
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == PlayFieldViewModel.TabIndex)
                {
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
                    PartyLineViewModel.ChatViewModel.IsInputFocused = true;
                }
            }
        }

        private void OnGameOver()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == PlayFieldViewModel.TabIndex)
                {
                    PartyLineViewModel.ChatViewModel.IsInputFocused = true;
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
                }
            }
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            IsRegistered = false;
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        #endregion
    }

    public class MainWindowViewModelDesignData : MainWindowViewModel
    {
        public MainWindowViewModelDesignData()
        {
            WinListViewModel = new WinListViewModelDesignData();
            StatisticsViewModel = new StatisticsViewModelDesignData();
            OptionsViewModel = new OptionsViewModelDesignData();
            PartyLineViewModel = new PartyLineViewModelDesignData();
            ConnectionViewModel = new ConnectionViewModelDesignData();
            PlayFieldPlayerViewModel = new PlayFieldViewModelDesignData();
            PlayFieldSpectatorViewModel = new PlayFieldSpectatorViewModelDesignData();
            AchievementsViewModel = new AchievementsViewModelDesignData();

            PlayFieldViewModel = PlayFieldPlayerViewModel;
            IsRegistered = true;
        }
    }
}