using System.ComponentModel;
using System.Windows;
using TetriNET.Client.Achievements;
using TetriNET.Client.Board;
using TetriNET.Client.Pieces;
using TetriNET.Client.WCFProxy;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
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
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; protected set; }
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
            set
            {
                if (_activeTabItemIndex != value)
                {
                    _activeTabItemIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRegistered;
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            AchievementManager manager = null;
            bool isDesignMode = DesignerProperties.GetIsInDesignMode(new DependencyObject());
            if (!isDesignMode)
            {
                //
                manager = new AchievementManager();
                manager.FindAllAchievements();
                Settings.Default.Achievements = Settings.Default.Achievements ?? new AchievementsSettings();
                Settings.Default.Achievements.Load(manager.Achievements);
            }

            // Create sub view models
            WinListViewModel = new WinListViewModel();
            ClientStatisticsViewModel = new ClientStatisticsViewModel();
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

            if (!isDesignMode)
            {
                // Create client
                Client = new Client.Client(Piece.CreatePiece, () => new BoardWithWallKick(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height), () => manager);
                //Client = new Client.Client(
                //    (pieces, i, arg3, arg4, arg5, arg6) => Piece.CreatePiece(Pieces.TetriminoI, i, arg3, arg4, arg5, arg6),
                //    () => new BoardWithWallKick(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height),
                //    () => manager);
            }
        }

        private void OnConnect(ConnectEventArgs e)
        {
            if (e.IsSpectator)
                e.Success = Client.ConnectAndRegisterAsSpectator(callback => new WCFSpectatorProxy(callback, ConnectionViewModel.LoginViewModel.ServerCompleteSpectatorAddress), ConnectionViewModel.LoginViewModel.Username);
            else
                e.Success = Client.ConnectAndRegisterAsPlayer(callback => new WCFProxy(callback, ConnectionViewModel.LoginViewModel.ServerCompletePlayerAddress), ConnectionViewModel.LoginViewModel.Username, PartyLineViewModel.Team);
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            WinListViewModel.Client = newClient;
            ClientStatisticsViewModel.Client = newClient;
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

        private void OnRegisteredAsPlayer(RegistrationResults result, int playerId, bool isServerMaster)
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

        private void OnRegisteredAsSpectator(RegistrationResults result, int spectatorId)
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
        //public new WinListViewModelDesignData WinListViewModel { get; private set; }
        //public new ClientStatisticsViewModelDesignData ClientStatisticsViewModel { get; private set; }
        //public new OptionsViewModelDesignData OptionsViewModel { get; private set; }
        //public new PartyLineViewModelDesignData PartyLineViewModel { get; private set; }
        //public new ConnectionViewModelDesignData ConnectionViewModel { get; private set; }
        //public new PlayFieldViewModelBase PlayFieldViewModel { get; private set; }
        //public new AchievementsViewModelDesignData AchievementsViewModel { get; private set; }

        //protected new PlayFieldViewModelDesignData PlayFieldPlayerViewModel { get; set; }
        //protected new PlayFieldSpectatorViewModelDesignData PlayFieldSpectatorViewModel { get; set; }

        public MainWindowViewModelDesignData()
        {
            WinListViewModel = new WinListViewModelDesignData();
            ClientStatisticsViewModel = new ClientStatisticsViewModelDesignData();
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