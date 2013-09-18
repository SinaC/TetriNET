using System.Linq;
using TetriNET.Client;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Properties;
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
        public int ActiveTabItemIndex {
            get
            {
                return _activeTabItemIndex;
            }
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
            Client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(Models.Options.Width, Models.Options.Height));

            // Get saved or default options
            Models.Options.OptionsSingleton.Instance.ServerOptions = Settings.Default.GameOptions ?? Client.Options;
            // TODO: fix this bug  ---- Workaround: remove duplicate key
            Models.Options.OptionsSingleton.Instance.ServerOptions.SpecialOccurancies = Models.Options.OptionsSingleton.Instance.ServerOptions.SpecialOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
            Models.Options.OptionsSingleton.Instance.ServerOptions.TetriminoOccurancies = Models.Options.OptionsSingleton.Instance.ServerOptions.TetriminoOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
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
        private void OnPlayerRegistered(RegistrationResults result, int playerId)
        {
            if (result == RegistrationResults.RegistrationSuccessful && Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                if (ActiveTabItemIndex == ConnectionViewModel.TabIndex)
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
            if (result == RegistrationResults.RegistrationSuccessful && Client.IsServerMaster)
                Client.ChangeOptions(Models.Options.OptionsSingleton.Instance.ServerOptions);
        }

        private void OnPlayerUnregisted()
        {
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        private void OnGameStarted()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ActiveTabItemIndex = PlayFieldViewModel.TabIndex;
        }

        private void OnGameFinished()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == PlayFieldViewModel.TabIndex)
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
            }
        }

        private void OnGameOver()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
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
}
