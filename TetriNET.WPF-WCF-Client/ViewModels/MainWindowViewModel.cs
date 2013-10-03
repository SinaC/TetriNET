using TetriNET.Client.DefaultBoardAndPieces;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
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
        public WinListViewModel WinListViewModel { get; set; }
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; set; }
        public OptionsViewModel OptionsViewModel { get; set; }
        public PartyLineViewModel PartyLineViewModel { get; set; }
        public ConnectionViewModel ConnectionViewModel { get; set; }
        public PlayFieldViewModel PlayFieldViewModel { get; set; }

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
            // Create sub view models
            WinListViewModel = new WinListViewModel();
            ClientStatisticsViewModel = new ClientStatisticsViewModel();
            OptionsViewModel = new OptionsViewModel();
            PartyLineViewModel = new PartyLineViewModel();
            ConnectionViewModel = new ConnectionViewModel();
            PlayFieldViewModel = new PlayFieldViewModel();
            ClientChanged += OnClientChanged;

            // Create client
            Client = new Client.Client(Piece.CreatePiece, () => new Board(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height));
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
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
            }
        }

        private void OnGameOver()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == PlayFieldViewModel.TabIndex)
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
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
        public new WinListViewModelDesignData WinListViewModel { get; set; }
        public new ClientStatisticsViewModelDesignData ClientStatisticsViewModel { get; set; }
        public new OptionsViewModelDesignData OptionsViewModel { get; set; }
        public new PartyLineViewModelDesignData PartyLineViewModel { get; set; }
        public new ConnectionViewModelDesignData ConnectionViewModel { get; set; }
        public new PlayFieldViewModel PlayFieldViewModel { get; set; }
    }
}