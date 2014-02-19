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
    public class ConnectEventArgs : EventArgs
    {
        public bool IsSpectator { get; set; }
        public bool Success { get; set; }
    }

    public class LoginViewModel : ViewModelBase, IDataErrorInfo
    {
        private bool _isRegistered;

        public bool IsNotRegistered { get { return !_isRegistered; } }

        public bool IsConnectDisconnectEnabled
        {
            get
            {
                string error = this["Username"];
                return String.IsNullOrWhiteSpace(error);
            }
        }

        public event Action<ConnectEventArgs> OnConnect;

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

        private bool _isSpectatorModeChecked;
        public bool IsSpectatorModeChecked
        {
            get { return _isSpectatorModeChecked; }
            set
            {
                if (_isSpectatorModeChecked != value)
                {
                    _isSpectatorModeChecked = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                    OnPropertyChanged("ServerCompletePlayerAddress");
                    OnPropertyChanged("ServerCompleteSpectatorAddress");
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
                    OnPropertyChanged("ServerCompletePlayerAddress");
                    OnPropertyChanged("ServerCompleteSpectatorAddress");
                }
            }
        }

        public string ServerCompletePlayerAddress
        {
            get { return "net.tcp://" + ServerAddress + ":" + ServerPort + "/TetriNET"; }
        }

        public string ServerCompleteSpectatorAddress
        {
            get { return "net.tcp://" + ServerAddress + ":" + ServerPort + "/TetriNETSpectator"; }
        }

        public void SetAddress(string address)
        {
            // Split
            Uri uri = new Uri(address);
            ServerAddress = uri.Host;
            ServerPort = uri.Port;
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
            //ServerAddress = Settings.Default.Server;
            string server = Settings.Default.Server;
            if (String.IsNullOrWhiteSpace(Settings.Default.Server))
                server = "net.tcp://localhost:8765/TetriNET"; // defaulting, doesn't seem to be set automatically
            SetAddress(server);

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

        private bool CheckError(string field)
        {
            string error = this[field];
            if (!String.IsNullOrWhiteSpace(error))
            {
                SetConnectionResultMessage(error, ChatColor.Red);
                return false;
            }
            return true;
        }

        private void ConnectDisconnect()
        {
            try
            {
                IsProgressBarVisible = true;
                if (!Client.IsRegistered)
                {
                    //if (String.IsNullOrEmpty(ServerAddress))
                    //{
                    //    SetConnectionResultMessage("Missing server address", ChatColor.Red);
                    //    return;
                    //}
                    //if (Settings.Default.Server != _serverAddress)
                    //{
                    //    Settings.Default.Server = _serverAddress;
                    //    Settings.Default.Save();
                    //}
                    if (!CheckError("ServerAddress"))
                        return;
                    if (!CheckError("ServerPort"))
                        return;
                    if (Settings.Default.Server != ServerCompletePlayerAddress)
                    {
                        Settings.Default.Server = ServerCompletePlayerAddress;
                        Settings.Default.Save();
                    }
                    if (!CheckError("Username"))
                        return;
                    if (Settings.Default.Username != _username)
                    {
                        Settings.Default.Username = _username;
                        Settings.Default.Save();
                    }

                    //bool connected;
                    if (OnConnect != null)
                    {
                        ConnectEventArgs args = new ConnectEventArgs
                            {
                                IsSpectator = IsSpectatorModeChecked
                            };
                        //if (IsSpectatorModeChecked)
                            //connected = Client.ConnectAndRegisterAsSpectator(callback => new WCFSpectatorProxy(callback, ServerCompleteSpectatorAddress), Username);
                        //else
                            //connected = Client.ConnectAndRegisterAsPlayer(callback => new WCFProxy(callback, ServerCompletePlayerAddress), Username);
                        OnConnect(args);
                        //if (!connected)
                        if (!args.Success)
                        {
                            SetConnectionResultMessage("Connection failed", ChatColor.Red);
                        }
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
            }
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnRegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.OnRegisteredAsSpectator -= OnRegisteredAsSpectator;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnRegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.OnRegisteredAsSpectator += OnRegisteredAsSpectator;
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
            OnPropertyChanged("IsNotRegistered");
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, int playerId, bool isServerMaster)
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
            OnPropertyChanged("IsNotRegistered");
        }

        private void OnRegisteredAsSpectator(RegistrationResults result, int spectatorId)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
                SetConnectionResultMessage(String.Format("Registered as spectator {0}", spectatorId + 1), ChatColor.Green);
            else
            {
                DescriptionAttribute attribute = EnumHelper.GetAttribute<DescriptionAttribute>(result);
                Client.UnregisterAndDisconnect();
                SetConnectionResultMessage(String.Format("Registration failed: {0}", attribute == null ? result.ToString() : attribute.Description), ChatColor.Red);
            }
            _isRegistered = Client.IsRegistered;
            OnPropertyChanged("ConnectDisconnectLabel");
            OnPropertyChanged("IsNotRegistered");
        }

        private void OnPlayerUnregistered()
        {
            _isRegistered = Client.IsRegistered;
            OnPropertyChanged("ConnectDisconnectLabel");
            OnPropertyChanged("IsNotRegistered");
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
                if (columnName == "ServerAddress")
                {
                    return String.IsNullOrWhiteSpace(ServerAddress) ? "Server address cannot be empty" : null;
                }
                if (columnName == "ServerPort")
                {
                    NumericValidationRule rule = new NumericValidationRule
                        {
                            Minimum = 1024,
                            Maximum = 65535,
                            PositiveOnly = true
                        };
                    ValidationResult result = rule.Validate(ServerPort, CultureInfo.InvariantCulture);
                    return (string)result.ErrorContent;
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
        public LoginViewModelDesignData()
        {
            ConnectionResultColor = ChatColor.Red;
            Username = "SinaC";
            ServerAddress = "localhost";
            ServerPort = 8765;
        }
    }
}