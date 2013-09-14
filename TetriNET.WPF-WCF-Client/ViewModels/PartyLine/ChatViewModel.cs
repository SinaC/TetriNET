using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public class ChatEntry // TODO: remove Brush and Visibility and use style
    {
        public string PlayerName { get; set; }
        public Visibility PlayerVisibility { get; set; } // false if server message
        public string Msg { get; set; }
        public Brush Color { get; set; }
    }

    public class ChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 500;

        private readonly ObservableCollection<ChatEntry> _chatEntries = new ObservableCollection<ChatEntry>();
        public ObservableCollection<ChatEntry> ChatEntries
        {
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

        private bool _isRegistered;
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set
            {
                if (_isRegistered != value)
                {
                    _isRegistered = value;
                    OnPropertyChanged();
                }
            }
        }

        private void AddEntry(string msg, Color color, string playerName = null)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                ChatEntries.Add(new ChatEntry
                {
                    PlayerName = playerName,
                    Msg = msg,
                    PlayerVisibility = String.IsNullOrEmpty(playerName) ? Visibility.Collapsed : Visibility.Visible,
                    Color = new SolidColorBrush(color),
                });
                if (ChatEntries.Count > MaxEntries)
                    ChatEntries.RemoveAt(0);
            });
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerPublishMessage -= OnPlayerPublishMessage;
            oldClient.OnServerPublishMessage -= OnServerPublishMessage;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnGamePaused -= OnGamePaused;
            oldClient.OnGameResumed -= OnGameResumed;
            oldClient.OnPlayerLost -= OnPlayerLost;
            oldClient.OnPlayerWon -= OnPlayerWon;
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerJoined -= OnPlayerJoined;
            oldClient.OnPlayerLeft -= OnPlayerLeft;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerPublishMessage += OnPlayerPublishMessage;
            newClient.OnServerPublishMessage += OnServerPublishMessage;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameOver += OnGameOver;
            newClient.OnGamePaused += OnGamePaused;
            newClient.OnGameResumed += OnGameResumed;
            newClient.OnPlayerLost += OnPlayerLost;
            newClient.OnPlayerWon += OnPlayerWon;
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerJoined += OnPlayerJoined;
            newClient.OnPlayerLeft += OnPlayerLeft;
            newClient.OnConnectionLost += OnConnectionLost;
        }
        #endregion

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            string msg;
            if (reason == ConnectionLostReasons.ServerNotFound)
                msg = "*** Server not found";
            else
                msg = "*** Connection lost";
            AddEntry(msg, Colors.Red);
            IsRegistered = false;
        }

        private void OnPlayerLeft(int playerid, string playerName, LeaveReasons reason)
        {
            string msg;
            switch (reason)
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
            AddEntry(msg, Colors.Green);
        }

        private void OnPlayerJoined(int playerid, string playerName)
        {
            AddEntry(String.Format("*** {0} has joined", playerName), Colors.Green);
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                AddEntry("*** You've registered successfully", Colors.Green);
                IsRegistered = true;
            }
            else
                AddEntry("*** You've FAILED registering !!!", Colors.Red);
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            AddEntry(String.Format("*** {0} has WON", playerName), Colors.Orange);
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            AddEntry(String.Format("*** {0} has LOST", playerName), Colors.Orange);
        }

        private void OnGameResumed()
        {
            AddEntry("*** The game has been Resumed", Colors.Yellow);
        }

        private void OnGamePaused()
        {
            AddEntry("*** The game has been Paused", Colors.Yellow);
        }

        private void OnGameOver()
        {
            AddEntry("*** You have LOST", Colors.Orange);
        }

        private void OnGameFinished()
        {
            AddEntry("*** The Game has Ended", Colors.Red);
        }

        private void OnGameStarted()
        {
            AddEntry("*** The Game has Started", Colors.Red);
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            AddEntry(msg, Colors.Black, playerName);
        }

        private void OnServerPublishMessage(string msg)
        {
            AddEntry(msg, Colors.Blue);
        }

        #endregion
    }
}
