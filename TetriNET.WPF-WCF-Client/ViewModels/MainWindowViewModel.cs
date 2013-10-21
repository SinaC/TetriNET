using TetriNET.Client.Achievements;
using TetriNET.Client.Board;
using TetriNET.Client.Pieces;
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
        public WinListViewModel WinListViewModel { get; private set; }
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; private set; }
        public OptionsViewModel OptionsViewModel { get; private set; }
        public PartyLineViewModel PartyLineViewModel { get; private set; }
        public ConnectionViewModel ConnectionViewModel { get; private set; }
        public PlayFieldViewModel PlayFieldViewModel { get; private set; }
        public AchievementsViewModel AchievementsViewModel { get; private set; }

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

        public MainWindowViewModel()
        {
            //
            AchievementManager manager = new AchievementManager();
            manager.FindAllAchievements();
            Settings.Default.Achievements = Settings.Default.Achievements ?? new AchievementsSettings();
            Settings.Default.Achievements.Load(manager.Achievements);

            // Create sub view models
            WinListViewModel = new WinListViewModel();
            ClientStatisticsViewModel = new ClientStatisticsViewModel();
            OptionsViewModel = new OptionsViewModel();
            PartyLineViewModel = new PartyLineViewModel();
            ConnectionViewModel = new ConnectionViewModel();
            PlayFieldViewModel = new PlayFieldViewModel();
            AchievementsViewModel = new AchievementsViewModel();

            //
            ClientChanged += OnClientChanged;

            // Create client
            Client = new Client.Client(Piece.CreatePiece, () => new BoardWithWallKick(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height), () => manager);
            //Client = new Client.Client(
            //    (pieces, i, arg3, arg4, arg5, arg6) => Piece.CreatePiece(Pieces.TetriminoI, i, arg3, arg4, arg5, arg6),
            //    () => new BoardWithWallKick(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height),
            //    () => manager);
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            WinListViewModel.Client = newClient;
            ClientStatisticsViewModel.Client = newClient;
            OptionsViewModel.Client = newClient;
            PartyLineViewModel.Client = newClient;
            ConnectionViewModel.Client = newClient;
            PlayFieldViewModel.Client = newClient;
            AchievementsViewModel.Client = newClient;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregisted;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregisted;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameOver += OnGameOver;
            newClient.OnConnectionLost += OnConnectionLost;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful && ClientOptionsViewModel.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                if (ActiveTabItemIndex == ConnectionViewModel.TabIndex)
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
        }

        private void OnPlayerUnregisted()
        {
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        private void OnGameStarted()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ActiveTabItemIndex = PlayFieldViewModel.TabIndex;
        }

        private void OnGameFinished()
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
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        #endregion
    }

    public class MainWindowViewModelDesignData : MainWindowViewModel
    {
        public new WinListViewModelDesignData WinListViewModel { get; private set; }
        public new ClientStatisticsViewModelDesignData ClientStatisticsViewModel { get; private set; }
        public new OptionsViewModelDesignData OptionsViewModel { get; private set; }
        public new PartyLineViewModelDesignData PartyLineViewModel { get; private set; }
        public new ConnectionViewModelDesignData ConnectionViewModel { get; private set; }
        public new PlayFieldViewModelDesignData PlayFieldViewModel { get; private set; }
        public new AchievementsViewModelDesignData AchievementsViewModel { get; private set; }

        public MainWindowViewModelDesignData()
        {
            WinListViewModel = new WinListViewModelDesignData();
            ClientStatisticsViewModel = new ClientStatisticsViewModelDesignData();
            OptionsViewModel = new OptionsViewModelDesignData();
            PartyLineViewModel = new PartyLineViewModelDesignData();
            ConnectionViewModel = new ConnectionViewModelDesignData();
            PlayFieldViewModel = new PlayFieldViewModelDesignData();
            AchievementsViewModel = new AchievementsViewModelDesignData();
        }
    }
}