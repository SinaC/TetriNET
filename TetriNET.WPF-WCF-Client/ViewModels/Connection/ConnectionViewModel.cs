using System;
using System.Reflection;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

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
            ServerListViewModel.OnServerSelected += OnServerSelected;

            ClientChanged += OnClientChanged;
        }

        private void OnServerSelected(object sender, string serverAddress)
        {
            LoginViewModel.ServerAddress = serverAddress;
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
        public new LoginViewModelDesignData LoginViewModel { get; private set; }

        public ConnectionViewModelDesignData()
        {
            LoginViewModel = new LoginViewModelDesignData();
        }
    }
}