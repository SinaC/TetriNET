using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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

        private void AddEntry(ChatEntry entry)
        {
            ExecuteOnUIThread.Invoke(() => ChatEntries.Add(entry));
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
                }
            }
        }

        private void OnPlayerLeft(int playerid, string playerName)
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Green),
                Msg = String.Format("*** {0} has LEFT", playerName)
            });
        }

        private void OnPlayerJoined(int playerid, string playerName)
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Green),
                Msg = String.Format("*** {0} has JOINED", playerName)
            });
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
                AddEntry(new ChatEntry
                {
                    PlayerVisibility = Visibility.Collapsed,
                    Color = new SolidColorBrush(Colors.Green),
                    Msg = "*** You've registered successfully"
                });
            else
                AddEntry(new ChatEntry
                {
                    PlayerVisibility = Visibility.Collapsed,
                    Color = new SolidColorBrush(Colors.Red),
                    Msg = "*** You've FAILED registering !!!"
                });
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Orange),
                Msg = String.Format("*** {0} has WON", playerName)
            });
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Orange),
                Msg = String.Format("*** {0} has LOST", playerName)
            });
        }

        private void OnGameResumed()
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Yellow),
                Msg = "*** The game has been Resumed"
            });
        }

        private void OnGamePaused()
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Yellow),
                Msg = "*** The game has been Paused"
            });
        }

        private void OnGameOver()
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Orange),
                Msg = "*** You have lost"
            });
        }

        private void OnGameFinished()
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Red),
                Msg = "*** The Game has Ended"
            });
        }

        private void OnGameStarted()
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Red),
                Msg = "*** The Game has Started" 
            });
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            AddEntry(new ChatEntry
            {
                PlayerName = playerName,
                PlayerVisibility = Visibility.Visible,
                Color = new SolidColorBrush(Colors.Black),
                Msg = msg
            });
        }

        private void OnServerPublishMessage(string msg)
        {
            AddEntry(new ChatEntry
            {
                PlayerVisibility = Visibility.Collapsed,
                Color = new SolidColorBrush(Colors.Blue),
                Msg = msg
            });
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
