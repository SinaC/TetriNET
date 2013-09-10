using System;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

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

            Username = "Joel";
            ServerAddress = ConfigurationManager.AppSettings["address"];
        }

        private void Success(string msg)
        {
            ConnectionResult = msg;
            ConnectionResultColor = new SolidColorBrush(Colors.Green);
        }

        private void Fail(string msg)
        {
            ConnectionResult = msg;
            ConnectionResultColor = new SolidColorBrush(Colors.Red);
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
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                }
            }
        }

        private void OnConnectionLost()
        {
            ExecuteOnUIThread.Invoke(() => Fail("Connection lost or Registration failed"));
            _isRegistered = false;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
                ExecuteOnUIThread.Invoke(() => Success(String.Format("Registered as player {0}", playerId+1)));
            else
                ExecuteOnUIThread.Invoke(() => Fail("Registration failed"));
            _isRegistered = Client.IsRegistered;
            OnPropertyChanged("ConnectDisconnectLabel");
        }

        private void ConnectDisconnect_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Client.IsRegistered)
            {
                if (String.IsNullOrEmpty(ServerAddress))
                {
                    ExecuteOnUIThread.Invoke(() => Fail("Missing server address"));
                    return;
                }
                if (String.IsNullOrEmpty(Username))
                {
                    ExecuteOnUIThread.Invoke(() => Fail("Missing username"));
                    return;
                }
                bool connected = Client.Connect(callback => new WCFProxy.WCFProxy(callback, ServerAddress));
                if (!connected)
                {
                    ExecuteOnUIThread.Invoke(() => Fail("Connection failed"));
                }
                else
                    Client.Register(Username);
            }
            else
            {
                Client.Unregister();
                bool disconnected = Client.Disconnect();
                if (disconnected)
                    ExecuteOnUIThread.Invoke(() => Success("Disconnected"));
                else
                    ExecuteOnUIThread.Invoke(() => Fail("Disconnection failed"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
