using System;
using System.Reflection;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Messages;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ConnectionViewModel : ViewModelBase, ITabIndex
    {
        public LoginViewModel LoginViewModel { get; set; }
        public ServerListViewModel ServerListViewModel { get; set; }

        public string AssemblyVersion
        {
            get
            {
                Assembly asm = AssemblyHelper.GetEntryAssembly();
                Version version = asm.GetName().Version;
                string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(asm, typeof(AssemblyCompanyAttribute), false)).Company;
                return String.Format("TetriNET {0}.{1} by {2}", version.Major, version.Minor, company);
            }
        }

        public ConnectionViewModel()
        {
            LoginViewModel = new LoginViewModel();
            ServerListViewModel = new ServerListViewModel();

            ClientChanged += OnClientChanged;

            Mediator.Register<ServerSelectedMessage>(this, OnServerSelected);
        }

        private void OnServerSelected(ServerSelectedMessage msg)
        {
            LoginViewModel.SetAddress(msg.ServerAddress);
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
            LoginViewModel.Client = newClient;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.RegisteredAsSpectator -= OnRegisteredAsSpectator;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.RegisteredAsSpectator += OnRegisteredAsSpectator;
        }

        #endregion

        #region IClient events handler

        private void OnRegisteredAsSpectator(RegistrationResults result, Versioning serverVersion, int spectatorId)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
                AddServerToLatest();
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
                AddServerToLatest();
        }

        #endregion

        private void AddServerToLatest()
        {
            string serverAddress = LoginViewModel.ServerCompletePlayerAddress;
            ServerListViewModel.AddServerToLatest(serverAddress);
        }
    }

    public class ConnectionViewModelDesignData : ConnectionViewModel
    {
        public ConnectionViewModelDesignData()
        {
            LoginViewModel = new LoginViewModelDesignData();
        }
    }
}