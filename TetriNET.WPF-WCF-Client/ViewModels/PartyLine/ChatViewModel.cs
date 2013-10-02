using System;
using System.Collections.ObjectModel;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public class ChatEntry
    {
        public string PlayerName { get; set; }
        public bool IsPlayerVisible { get; set; } // false if server message
        public string Msg { get; set; }
        public ChatColor Color { get; set; }
    }

    public class ChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 500;

        public ObservableCollection<ChatEntry> ChatEntries { get; private set; }

        private string _inputChat;
        public string InputChat
        {
            get { return _inputChat; }
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

        public ChatViewModel()
        {
            ChatEntries = new ObservableCollection<ChatEntry>();
        }

        private void AddEntry(string msg, ChatColor color, string playerName = null)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    ChatEntries.Add(new ChatEntry
                        {
                            PlayerName = playerName,
                            Msg = msg,
                            IsPlayerVisible = !String.IsNullOrEmpty(playerName),
                            Color = color,
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
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
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
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregistered()
        {
            AddEntry("*** You've unregistered successfully", ChatColor.Green);
            IsRegistered = true;
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            string msg;
            if (reason == ConnectionLostReasons.ServerNotFound)
                msg = "*** Server not found";
            else
                msg = "*** Connection lost";
            AddEntry(msg, ChatColor.Red);
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
            AddEntry(msg, ChatColor.Green);
        }

        private void OnPlayerJoined(int playerid, string playerName)
        {
            AddEntry(String.Format("*** {0} has joined", playerName), ChatColor.Green);
        }

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                AddEntry("*** You've registered successfully", ChatColor.Green);
                IsRegistered = true;
            }
            else
                AddEntry("*** You've FAILED registering !!!", ChatColor.Red);
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            AddEntry(String.Format("*** {0} has WON", playerName), ChatColor.Orange);
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            AddEntry(String.Format("*** {0} has LOST", playerName), ChatColor.Orange);
        }

        private void OnGameResumed()
        {
            AddEntry("*** The game has been Resumed", ChatColor.Yellow);
        }

        private void OnGamePaused()
        {
            AddEntry("*** The game has been Paused", ChatColor.Yellow);
        }

        private void OnGameOver()
        {
            AddEntry("*** You have LOST", ChatColor.Orange);
        }

        private void OnGameFinished()
        {
            AddEntry("*** The Game has Ended", ChatColor.Red);
        }

        private void OnGameStarted()
        {
            AddEntry("*** The Game has Started", ChatColor.Red);
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            AddEntry(msg, ChatColor.Black, playerName);
        }

        private void OnServerPublishMessage(string msg)
        {
            AddEntry(msg, ChatColor.Blue);
        }

        #endregion
    }

    public class ChatViewModelDesignData : ChatViewModel
    {
        public new ObservableCollection<ChatEntry> ChatEntries { get; private set; }

        public ChatViewModelDesignData()
        {
            ChatEntries = new ObservableCollection<ChatEntry>
                {
                    new ChatEntry
                        {
                            PlayerName = "Dummy1",
                            Color = ChatColor.Green,
                            IsPlayerVisible = false,
                            Msg = "Message with player name visible"
                        },
                    new ChatEntry
                        {
                            PlayerName = "Dummy1",
                            Color = ChatColor.Red,
                            IsPlayerVisible = false,
                            Msg = "Message with player name hidden"
                        }
                };
        }
    }
}