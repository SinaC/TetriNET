﻿using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using TetriNET.Client;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Connection
{
    public class ConnectionControlViewModel : ViewModelBase
    {
        private bool _isRegistered;

        private string _username;
        public string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                    Settings.Default.Username = _username;
                    Settings.Default.Save();
                }
            }
        }

        private string _serverAddress;
        public string ServerAddress
        {
            get
            {
                return _serverAddress;
            }
            set
            {
                if (_serverAddress != value)
                {
                    _serverAddress = value;
                    OnPropertyChanged();
                    Settings.Default.Server = _serverAddress;
                    Settings.Default.Save();
                }
            }
        }

        public string ConnectDisconnectLabel
        {
            get { return _isRegistered ? "Disconnect" : "Connect"; }
        }

        private string _connectionResult;
        public string ConnectionResult
        {
            get
            {
                return _connectionResult;
            }
            set
            {
                if (_connectionResult != value)
                {
                    _connectionResult = value;
                    OnPropertyChanged();
                }
            }
        }

        private Brush _connectionResultColor;
        public Brush ConnectionResultColor
        {
            get { return _connectionResultColor; }
            set
            {
                if (!Equals(_connectionResultColor, value))
                {
                    _connectionResultColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public ConnectionControlViewModel()
        {
            Username = Settings.Default.Username;
            ServerAddress = Settings.Default.Server;

            ConnectDisconnectCommand = new RelayCommand(ConnectDisconnect);
        }

        private void SetConnectionResultMessage(string msg, Color color)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                ConnectionResult = msg;
                ConnectionResultColor = new SolidColorBrush(color);
            });
        }

        private void ConnectDisconnect()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (!Client.IsRegistered)
                {
                    if (String.IsNullOrEmpty(ServerAddress))
                    {
                        SetConnectionResultMessage("Missing server address", Colors.Red);
                        return;
                    }
                    if (String.IsNullOrEmpty(Username))
                    {
                        SetConnectionResultMessage("Missing username", Colors.Red);
                        return;
                    }
                    bool connected = Client.Connect(callback => new WCFProxy.WCFProxy(callback, ServerAddress));
                    if (!connected)
                    {
                        SetConnectionResultMessage("Connection failed", Colors.Red);
                    }
                    else
                        Client.Register(Username);
                }
                else
                {
                    Client.Unregister();
                    bool disconnected = Client.Disconnect();
                    if (disconnected)
                        SetConnectionResultMessage("Disconnected", Colors.Red);
                    else
                        SetConnectionResultMessage("Disconnection failed", Colors.Red);
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
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

            SetConnectionResultMessage(msg, Colors.Red);
            _isRegistered = false;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        private void OnPlayerRegistered(RegistrationResults result, int playerId)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
               SetConnectionResultMessage(String.Format("Registered as player {0}", playerId + 1), Colors.Green);
            else
            {
                DescriptionAttribute attribute = EnumHelper.GetAttribute<DescriptionAttribute>(result);
                Client.Disconnect();
                SetConnectionResultMessage(String.Format("Registration failed {0}", attribute == null ? result.ToString() : attribute.Description), Colors.Red);
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
        public ICommand ConnectDisconnectCommand { get; set; }
        #endregion
    }

}
