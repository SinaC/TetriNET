using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class OptionsViewModel : ViewModelBase, ITabIndex
    {
        public ClientOptionsViewModel ClientOptionsViewModel { get; set; }
        public ServerOptionsViewModel ServerOptionsViewModel { get; set; }

        public OptionsViewModel()
        {
            ClientOptionsViewModel = new ClientOptionsViewModel();
            ServerOptionsViewModel = new ServerOptionsViewModel();

            ClientChanged += OnClientChanged;
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            ClientOptionsViewModel.Client = newClient;
            ServerOptionsViewModel.Client = newClient;
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

        #region ITabIndex

        public int TabIndex
        {
            get { return 1; }
        }

        #endregion
    }

    public class OptionsViewModelDesignData : OptionsViewModel
    {
        public new ClientOptionsViewModelDesignData ClientOptionsViewModel { get; private set; }
        public new ServerOptionsViewModelDesignData ServerOptionsViewModel { get; private set; }

        public OptionsViewModelDesignData()
        {
            ClientOptionsViewModel = new ClientOptionsViewModelDesignData();
            ServerOptionsViewModel = new ServerOptionsViewModelDesignData();
        }
    }
}