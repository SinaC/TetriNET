using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Views.PartyLine
{
    /// <summary>
    /// Interaction logic for PartyLine.xaml
    /// </summary>
    public partial class PartyLineView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("PartyLineClientProperty", typeof(IClient), typeof(PartyLineView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private bool _isRegistered;
        private bool _isServerMaster;
        private bool _isGameStarted;
        private bool _isGamePaused;

        public bool IsStartStopEnabled
        {
            get { return _isRegistered && _isServerMaster; }
        }

        public bool IsPauseResumeEnabled
        {
            get { return _isRegistered && _isServerMaster && (_isGameStarted || _isGamePaused); }
        }

        public string StartStopLabel
        {
            get { return _isGameStarted || _isGamePaused ? "Stop game" : "Start game"; }
        }

        public string PauseResumeLabel
        {
            get { return _isGamePaused ? "Resume game" : "Pause game"; }
        }

        public PartyLineView()
        {
            InitializeComponent();

            _isServerMaster = false;
            _isGameStarted = false;
            _isGamePaused = false;
        }

        private void UpdateEnability()
        {
            OnPropertyChanged("IsStartStopEnabled");
            OnPropertyChanged("IsPauseResumeEnabled");
            OnPropertyChanged("StartStopLabel");
            OnPropertyChanged("PauseResumeLabel");
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PartyLineView _this = sender as PartyLineView;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnPlayerUnregistered -= _this.OnPlayerUnregistered;
                    oldClient.OnServerMasterModified -= _this.OnServerMasterModified;
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnGameFinished -= _this.OnGameFinished;
                    oldClient.OnGamePaused -= _this.OnGamePaused;
                    oldClient.OnGameResumed -= _this.OnGameResumed;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                _this.Chat.Client = newClient;
                _this.PlayersManager.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnPlayerUnregistered += _this.OnPlayerUnregistered;
                    newClient.OnServerMasterModified += _this.OnServerMasterModified;
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnGameFinished += _this.OnGameFinished;
                    newClient.OnGamePaused += _this.OnGamePaused;
                    newClient.OnGameResumed += _this.OnGameResumed;
                }
            }
        }

        #region IClient events handler
        private void OnGameResumed()
        {
            _isGamePaused = false;
            UpdateEnability();
        }

        private void OnGamePaused()
        {
            _isGamePaused = true;
            UpdateEnability();
        }

        private void OnGameFinished()
        {
            _isGameStarted = false;
            UpdateEnability();
        }

        private void OnGameStarted()
        {
            _isGameStarted = true;
            UpdateEnability();
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            _isServerMaster = Client.IsServerMaster;
            UpdateEnability();
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            _isRegistered = Client.IsRegistered;
            _isGameStarted = Client.IsGameStarted;
            _isServerMaster = Client.IsServerMaster;
            _isGamePaused = false;
            UpdateEnability();
        }

        private void OnPlayerUnregistered()
        {
            _isRegistered = Client.IsRegistered;
            _isGameStarted = Client.IsGameStarted;
            _isServerMaster = Client.IsServerMaster;
            _isGamePaused = false;
            UpdateEnability();
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            _isRegistered = false;
            _isServerMaster = false;
            UpdateEnability();
        }
        #endregion

        #region UI events handler
        private void StartStopGame_OnClick(object sender, RoutedEventArgs e)
        {
            if (Client.IsGameStarted)
                Client.StopGame();
            else
                Client.StartGame();
        }

        private void PauseResumeGame_OnClick(object sender, RoutedEventArgs e)
        {
            if (Client.IsGamePaused)
                Client.ResumeGame();
            else
                Client.PauseGame();
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
