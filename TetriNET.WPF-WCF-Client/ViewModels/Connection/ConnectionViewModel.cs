using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ConnectionViewModel : ViewModelBase
    {
        public ConnectionControlViewModel ConnectionControlViewModel { get; set; }
        public ServerListViewModel ServerListViewModel { get; set; }

        public ConnectionViewModel()
        {
            ConnectionControlViewModel = new ConnectionControlViewModel();
            ServerListViewModel = new ServerListViewModel();
            ServerListViewModel.OnServerSelected += OnServerSelected;
        }

        private void OnServerSelected(object sender, string serverAddress)
        {
            ConnectionControlViewModel.ServerAddress = serverAddress;
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            // NOP
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            // NOP
        }

        public override void OnClientAssigned(IClient newClient)
        {
            ConnectionControlViewModel.Client = newClient;
        }

        #endregion
    }
}
