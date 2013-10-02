﻿using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ConnectionViewModel : ViewModelBase, ITabIndex
    {
        public LoginViewModel ConnectionControlViewModel { get; set; }
        public ServerListViewModel ServerListViewModel { get; set; }

        public ConnectionViewModel()
        {
            ConnectionControlViewModel = new LoginViewModel();
            ServerListViewModel = new ServerListViewModel();
            ServerListViewModel.OnServerSelected += OnServerSelected;

            ClientChanged += OnClientChanged;
        }

        private void OnServerSelected(object sender, string serverAddress)
        {
            ConnectionControlViewModel.ServerAddress = serverAddress;
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 0; }
        }

        #endregion

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            ConnectionControlViewModel.Client = newClient;
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

    public class ConnectionViewModelDesignData : ConnectionViewModel
    {
        public new LoginViewModelDesignData ConnectionControlViewModel { get; private set; }

        public ConnectionViewModelDesignData()
        {
            ConnectionControlViewModel = new LoginViewModelDesignData();
        }
    }
}