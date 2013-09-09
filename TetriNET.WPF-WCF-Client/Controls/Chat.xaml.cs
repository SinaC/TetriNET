using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Controls
{
    public class ChatEntry
    {
        public string PlayerName { get; set; }
        public Visibility PlayerVisibility { get; set; } // false if server message
        public string Msg { get; set; }
        public Brush Color { get; set; }
    }

    /// <summary>
    /// Interaction logic for PartyLine.xaml
    /// </summary>
    public partial class Chat : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("ChatClientProperty", typeof(IClient), typeof(Chat), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly ObservableCollection<ChatEntry> _chatEntries = new ObservableCollection<ChatEntry>();
        public ObservableCollection<ChatEntry> ChatEntries {
            get { return _chatEntries; }
        }

        private string _inputChat;
        public string InputChat
        {
            get
            {
                return _inputChat;
            }
            set
            {
                if (value != null)
                {
                    if (Client != null)
                        Client.PublishMessage(value);
                    _inputChat = ""; // delete msg
                    OnPropertyChanged();
                }
            }
        }

        public Chat()
        {
            InitializeComponent();
        }

        private void AddEntry(string msg, Color color, string playerName = null)
        {
            ChatEntries.Add(new ChatEntry
                {
                    PlayerName = playerName,
                    Msg = msg,
                    PlayerVisibility = String.IsNullOrEmpty(playerName) ? Visibility.Collapsed : Visibility.Visible,
                    Color = new SolidColorBrush(color),
                });
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            Chat _this = sender as Chat;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnPlayerPublishMessage -= _this.OnPlayerPublishMessage;
                    oldClient.OnServerPublishMessage -= _this.OnServerPublishMessage;
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnGameFinished -= _this.OnGameFinished;
                    oldClient.OnGameOver -= _this.OnGameOver;
                    oldClient.OnGamePaused -= _this.OnGamePaused;
                    oldClient.OnGameResumed -= _this.OnGameResumed;
                    oldClient.OnPlayerLost -= _this.OnPlayerLost;
                    oldClient.OnPlayerWon -= _this.OnPlayerWon;
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnPlayerJoined -= _this.OnPlayerJoined;
                    oldClient.OnPlayerLeft -= _this.OnPlayerLeft;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerPublishMessage += _this.OnPlayerPublishMessage;
                    newClient.OnServerPublishMessage += _this.OnServerPublishMessage;
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnGameFinished += _this.OnGameFinished;
                    newClient.OnGameOver += _this.OnGameOver;
                    newClient.OnGamePaused += _this.OnGamePaused;
                    newClient.OnGameResumed += _this.OnGameResumed;
                    newClient.OnPlayerLost += _this.OnPlayerLost;
                    newClient.OnPlayerWon += _this.OnPlayerWon;
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnPlayerJoined += _this.OnPlayerJoined;
                    newClient.OnPlayerLeft += _this.OnPlayerLeft;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                }
            }
        }

        private void OnConnectionLost()
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(String.Format("*** Connection LOST"), Colors.Red));
        }

        private void OnPlayerLeft(int playerid, string playerName, LeaveReasons reason)
        {
            string msg;
            switch(reason)
            {
                case LeaveReasons.Spam:
                    msg = String.Format("*** {0} has been BANNED [SPAM]", playerName);
                    break;
                case LeaveReasons.Disconnected:
                    msg = String.Format("*** {0} has disconnected", playerName);
                    break;
                case LeaveReasons.ConnectionLost:
                    msg = String.Format("*** {0} has left [connection lost]", playerName);
                    break;
                case LeaveReasons.Timeout:
                    msg = String.Format("*** {0} has left [timeout]", playerName);
                    break;
                case LeaveReasons.Ban:
                    msg = String.Format("*** {0} has been BANNED", playerName);
                    break;
                case LeaveReasons.Kick:
                    msg = String.Format("*** {0} has been kicked", playerName);
                    break;
                default:
                    msg = String.Format("*** {0} has left {1}", playerName, reason);
                    break;
            }
            ExecuteOnUIThread.Invoke(() => AddEntry(msg, Colors.Green));
        }

        private void OnPlayerJoined(int playerid, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(String.Format("*** {0} has joined", playerName), Colors.Green));
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
                ExecuteOnUIThread.Invoke(() => AddEntry("*** You've registered successfully", Colors.Green));
            else
                ExecuteOnUIThread.Invoke(() => AddEntry("*** You've FAILED registering !!!", Colors.Red));
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(String.Format("*** {0} has WON", playerName), Colors.Orange));
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(String.Format("*** {0} has LOST", playerName), Colors.Orange));
        }

        private void OnGameResumed()
        {
            ExecuteOnUIThread.Invoke(() => AddEntry("*** The game has been Resumed", Colors.Yellow));
        }

        private void OnGamePaused()
        {
            ExecuteOnUIThread.Invoke(() => AddEntry("*** The game has been Paused", Colors.Yellow));
        }

        private void OnGameOver()
        {
            ExecuteOnUIThread.Invoke(() => AddEntry("*** You have LOST", Colors.Orange));
        }

        private void OnGameFinished()
        {
            ExecuteOnUIThread.Invoke(() => AddEntry("*** The Game has Ended", Colors.Red));
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() => AddEntry("*** The Game has Started", Colors.Red));
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(msg, Colors.Black, playerName));
        }

        private void OnServerPublishMessage(string msg)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(msg, Colors.Blue));
        }

        private void InputChat_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = this.TxtInputChat.GetBindingExpression(TextBox.TextProperty);
                if (exp != null)
                    exp.UpdateSource();
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
