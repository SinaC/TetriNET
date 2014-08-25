using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class StatisticsViewModel : ViewModelBase, ITabIndex
    {
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; set; }
        public GameStatisticsViewModel GameStatisticsViewModel { get; set; }

        public bool IsPlayer
        {
            get { return Client != null && Client.IsRegistered && !Client.IsSpectator; }
        }

        public bool IsSpectator
        {
            get { return Client != null && Client.IsRegistered && Client.IsSpectator; }
        }

        public StatisticsViewModel()
        {
            ClientStatisticsViewModel = new ClientStatisticsViewModel();
            GameStatisticsViewModel = new GameStatisticsViewModel();
            ClientChanged += OnClientChanged;
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 6; }
        }

        #endregion

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            ClientStatisticsViewModel.Client = newClient;
            GameStatisticsViewModel.Client = newClient;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.PlayerUnregistered -= OnPlayerUnregistered;
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.RegisteredAsSpectator -= RegisteredAsSpectator;
            oldClient.ConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.PlayerUnregistered += OnPlayerUnregistered;
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.RegisteredAsSpectator += RegisteredAsSpectator;
            newClient.ConnectionLost += OnConnectionLost;
        }


        #endregion

        #region IClient events handler
        
        private void OnPlayerUnregistered()
        {
            RefreshMode();
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, int playerId, bool isServerMaster)
        {
            RefreshMode();
        }

        private void RegisteredAsSpectator(RegistrationResults result, int spectatorId)
        {
            RefreshMode();
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            RefreshMode();
        }

        #endregion

        private void RefreshMode()
        {
            OnPropertyChanged("IsPlayer");
            OnPropertyChanged("IsSpectator");
        }
    }

    public class StatisticsViewModelDesignData : StatisticsViewModel
    {
        public new bool IsPlayer
        {
            get { return true; }
        }

        public new bool IsSpectator
        {
            get { return false; }
        }

        public StatisticsViewModelDesignData()
        {
            ClientStatisticsViewModel = new ClientStatisticsViewModelDesignData();
            GameStatisticsViewModel = new GameStatisticsViewModelDesignData();
        }
    }
}
