using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class StatisticsViewModel : ViewModelBase, ITabIndex
    {
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; set; }
        public GameStatisticsViewModel GameStatisticsViewModel { get; set; }

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
            // NOP
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            // NOP
        }

        #endregion
    }

    public class StatisticsViewModelDesignData : StatisticsViewModel
    {
        public StatisticsViewModelDesignData()
        {
            ClientStatisticsViewModel = new ClientStatisticsViewModelDesignData();
            GameStatisticsViewModel = new GameStatisticsViewModelDesignData();
        }
    }
}
