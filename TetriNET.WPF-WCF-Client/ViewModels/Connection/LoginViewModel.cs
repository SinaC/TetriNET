using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Client.WCFProxy;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;
using TetriNET.WPF_WCF_Client.Properties;
using TetriNET.WPF_WCF_Client.Validators;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class LoginViewModel : ViewModelBase, IDataErrorInfo
    {
        private bool _isRegistered;

        public bool IsConnectDisconnectEnabled
        {
            get
            {
                string error = this["Username"];
                return String.IsNullOrWhiteSpace(error);
            }
        }

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsConnectDisconnectEnabled");
                }
            }
        }

        private string _serverAddress;
        public string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                if (_serverAddress != value)
                {
                    _serverAddress = value;
                    if (String.IsNullOrWhiteSpace(_serverAddress))
                        _serverAddress = "localhost:8765";
                    // net.tcp://[ip|machine name]:[port]/TetriNET
                    if (!_serverAddress.StartsWith("net.tcp://"))
                        _serverAddress = "net.tcp://" + _serverAddress;
                    //if (ServerAddress.EndsWith("/TetriNETv2"))
                    //    _serverAddress = _serverAddress.Replace("/TetriNETv2", ""); // remove old endpoint
                    if (!_serverAddress.EndsWith("/TetriNET"))
                        _serverAddress = _serverAddress + "/TetriNET";

                    OnPropertyChanged();
                    OnPropertyChanged("ServerCompleteAddress");
                }
            }
        }

        private int _serverPort;
        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (_serverPort != value)
                {
                    _serverPort = value;
                    OnPropertyChanged();
                    OnPropertyChanged("ServerCompleteAddress");
                }
            }
        }

        public string ServerCompleteAddress
        {
            get { return "net.tcp://" + ServerAddress + ":" + ServerPort + "/TetriNET"; }
        }

        public string ConnectDisconnectLabel
        {
            get { return _isRegistered ? "Disconnect" : "Connect"; }
        }

        private string _connectionResult;
        public string ConnectionResult
        {
            get { return _connectionResult; }
            set
            {
                if (_connectionResult != value)
                {
                    _connectionResult = value;
                    OnPropertyChanged();
                }
            }
        }

        private ChatColor _connectionResultColor;
        public ChatColor ConnectionResultColor
        {
            get { return _connectionResultColor; }
            set
            {
                if (_connectionResultColor != value)
                {
                    _connectionResultColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isProgressBarVisible;
        public bool IsProgressBarVisible
        {
            get { return _isProgressBarVisible; }
            set
            {
                if (_isProgressBarVisible != value)
                {
                    _isProgressBarVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public LoginViewModel()
        {
            ConnectionResultColor = ChatColor.Black;
            IsProgressBarVisible = false;
            Username = Settings.Default.Username;
            ServerAddress = Settings.Default.Server;

            ConnectDisconnectCommand = new AsyncRelayCommand(ConnectDisconnect);
        }

        private void SetConnectionResultMessage(string msg, ChatColor color)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    ConnectionResult = msg;
                    ConnectionResultColor = color;
                });
        }

        private void ConnectDisconnect()
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                IsProgressBarVisible = true;
                if (!Client.IsRegistered)
                {
                    if (String.IsNullOrEmpty(ServerAddress))
                    {
                        SetConnectionResultMessage("Missing server address", ChatColor.Red);
                        return;
                    }
                    if (Settings.Default.Server != _serverAddress)
                    {
                        Settings.Default.Server = _serverAddress;
                        Settings.Default.Save();
                    }

                    string error = this["Username"];
                    if (!String.IsNullOrWhiteSpace(error))
                    {
                        SetConnectionResultMessage(error, ChatColor.Red);
                        return;
                    }
                    if (Settings.Default.Username != _username)
                    {
                        Settings.Default.Username = _username;
                        Settings.Default.Save();
                    }

                    bool connected = Client.ConnectAndRegister(callback => new WCFProxy(callback, ServerAddress), Username);
                    if (!connected)
                    {
                        SetConnectionResultMessage("Connection failed", ChatColor.Red);
                    }
                }
                else
                {
                    bool disconnected = Client.UnregisterAndDisconnect();
                    if (disconnected)
                        SetConnectionResultMessage("Disconnected", ChatColor.Red);
                    else
                        SetConnectionResultMessage("Disconnection failed", ChatColor.Red);
                }
            }
            finally
            {
                IsProgressBarVisible = false;
                //Mouse.OverrideCursor = null;
            }
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnConnectionLost += OnConnectionLost;
        }

        #endregion

        #region IClient events handler

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            string msg;
            if (reason == ConnectionLostReasons.ServerNotFound)
                msg = "Server not found";
            else
                msg = "Connection lost";

            SetConnectionResultMessage(msg, ChatColor.Red);
            _isRegistered = false;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
                SetConnectionResultMessage(String.Format("Registered as player {0}", playerId + 1), ChatColor.Green);
            else
            {
                DescriptionAttribute attribute = EnumHelper.GetAttribute<DescriptionAttribute>(result);
                Client.UnregisterAndDisconnect();
                SetConnectionResultMessage(String.Format("Registration failed: {0}", attribute == null ? result.ToString() : attribute.Description), ChatColor.Red);
            }
            _isRegistered = Client.IsRegistered;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        private void OnPlayerUnregistered()
        {
            _isRegistered = Client.IsRegistered;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        #endregion

        #region Commands

        public ICommand ConnectDisconnectCommand { get; private set; }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                if (columnName == "Username")
                {
                    StringValidationRule rule = new StringValidationRule
                        {
                            FieldName = columnName
                        };
                    ValidationResult result = rule.Validate(Username, CultureInfo.InvariantCulture);
                    return (string) result.ErrorContent;
                }
                return null;
            }
        }
        
        public string Error
        {
            get { return String.Empty; }
        }

        #endregion
    }

    public class LoginViewModelDesignData : LoginViewModel
    {
        public new ChatColor ConnectionResultColor { get; private set; }

        public LoginViewModelDesignData()
        {
            ConnectionResultColor = ChatColor.Red;
        }
    }
}