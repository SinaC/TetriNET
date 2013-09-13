using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for Connection.xaml
    /// </summary>
    public partial class Connection : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("ConnectionClientProperty", typeof(IClient), typeof(Connection), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

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

        public Connection()
        {
            InitializeComponent();

            //Username = "Joel";
            //ServerAddress = ConfigurationManager.AppSettings["address"];
            Username = Settings.Default.Username;
            ServerAddress = Settings.Default.Server;
        }

        private void SetConnectionResultMessage(string msg, Color color)
        {
            ConnectionResult = msg;
            ConnectionResultColor = new SolidColorBrush(color);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            Connection _this = sender as Connection;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnPlayerUnregistered -= _this.OnPlayerUnregistered;
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerUnregistered += _this.OnPlayerUnregistered;
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                }
            }
        }

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            string msg;
            if (reason == ConnectionLostReasons.ServerNotFound)
                msg = "Server not found";
            else
                msg = "Connection lost";

            ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage(msg, Colors.Red));
            _isRegistered = false;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
                ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage(String.Format("Registered as player {0}", playerId + 1), Colors.Green));
            else
            {
                Client.Disconnect();
                ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage("Registration failed", Colors.Red));
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

        #region UI events handler
        private void ConnectDisconnect_OnClick(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                if (!Client.IsRegistered)
                {
                    if (String.IsNullOrEmpty(ServerAddress))
                    {
                        ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage("Missing server address", Colors.Red));
                        return;
                    }
                    if (String.IsNullOrEmpty(Username))
                    {
                        ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage("Missing username", Colors.Red));
                        return;
                    }
                    bool connected = Client.Connect(callback => new WCFProxy.WCFProxy(callback, ServerAddress));
                    if (!connected)
                    {
                        ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage("Connection failed", Colors.Red));
                    }
                    else
                        Client.Register(Username);
                }
                else
                {
                    Client.Unregister();
                    bool disconnected = Client.Disconnect();
                    if (disconnected)
                        ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage("Disconnected", Colors.Red));
                    else
                        ExecuteOnUIThread.Invoke(() => SetConnectionResultMessage("Disconnection failed", Colors.Red));
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
