using System;
using System.Collections.ObjectModel;
using System.Linq;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public abstract class ChatEntry
    {
        public ChatColor Color { get; set; }
    }

    public class PlayerMessageEntry : ChatEntry
    {
        public string Message { get; set; }
        public string PlayerName { get; set; }
    }

    public class ServerMessageEntry : ChatEntry
    {
        public string Message { get; set; }
    }

    public abstract class AchievementEntry : ChatEntry
    {
        public IClient Client { get; set; }
        public IAchievement Achievement { get; set; }

        public bool IsEarned
        {
            get
            {
                if (Achievement != null && Client != null && Client.Achievements != null)
                    return Client.Achievements.Any(x => x.Id == Achievement.Id && x.IsAchieved);
                return false;
            }
        }

        public DateTime FirstTimeAchieved
        {
            get
            {
                if (Achievement != null && Client != null && Client.Achievements != null)
                {
                    IAchievement achievement = Client.Achievements.FirstOrDefault(x => x.Id == Achievement.Id && x.IsAchieved);
                    if (achievement != null)
                        return achievement.FirstTimeAchieved;
                }
                return DateTime.MinValue;
            }
        }
    }

    public class SelfAchievementEntry : AchievementEntry
    {
    }

    public class OtherAchievementEntry : AchievementEntry
    {
        public string PlayerName { get; set; }
    }

    public class InvalidAchievementEntry : ChatEntry
    {
        public string PlayerName { get; set; }
        public string AchievementTitle { get; set; }
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
            get { return _inputChat; }
            set
            {
                if (value != null)
                {
                    Client.Do(x => x.PublishMessage(value));
                    _inputChat = ""; // delete msg
                    OnPropertyChanged();
                }
            }
        }

        private bool _isRegistered;
        public bool IsRegistered
        {
            get { return _isRegistered; }
            set { Set(() => IsRegistered, ref _isRegistered, value); }
        }

        private bool _isInputFocused;
        public bool IsInputFocused
        {
            get { return _isInputFocused; }
            set { Set(() => IsInputFocused, ref _isInputFocused, value); }
        }

        protected void AddEntry(ChatEntry entry)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                ChatEntries.Add(entry);
                if (ChatEntries.Count > MaxEntries)
                    ChatEntries.RemoveAt(0);
            });
        }

        protected void AddServerMessage(ChatColor color, string format, params object[] args)
        {
            AddEntry(new ServerMessageEntry
            {
                Message = String.Format(format, args),
                Color = color,
            });
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.PlayerAchievementEarned -= OnPlayerAchievementEarned;
            oldClient.AchievementEarned -= OnAchievementEarned;
            oldClient.PlayerPublishMessage -= OnPlayerPublishMessage;
            oldClient.ServerPublishMessage -= OnServerPublishMessage;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.GameOver -= OnGameOver;
            oldClient.GamePaused -= OnGamePaused;
            oldClient.GameResumed -= OnGameResumed;
            oldClient.PlayerLost -= OnPlayerLost;
            oldClient.PlayerWon -= OnPlayerWon;
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.RegisteredAsSpectator -= OnRegisteredAsSpectator;
            oldClient.PlayerJoined -= OnPlayerJoined;
            oldClient.PlayerLeft -= OnPlayerLeft;
            oldClient.SpectatorJoined -= OnSpectatorJoined;
            oldClient.SpectatorLeft -= OnSpectatorLeft;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.PlayerUnregistered -= OnPlayerUnregistered;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.PlayerAchievementEarned += OnPlayerAchievementEarned;
            newClient.AchievementEarned += OnAchievementEarned;
            newClient.PlayerPublishMessage += OnPlayerPublishMessage;
            newClient.ServerPublishMessage += OnServerPublishMessage;
            newClient.GameStarted += OnGameStarted;
            newClient.GameFinished += OnGameFinished;
            newClient.GameOver += OnGameOver;
            newClient.GamePaused += OnGamePaused;
            newClient.GameResumed += OnGameResumed;
            newClient.PlayerLost += OnPlayerLost;
            newClient.PlayerWon += OnPlayerWon;
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.RegisteredAsSpectator += OnRegisteredAsSpectator;
            newClient.PlayerJoined += OnPlayerJoined;
            newClient.PlayerLeft += OnPlayerLeft;
            newClient.SpectatorJoined += OnSpectatorJoined;
            newClient.SpectatorLeft += OnSpectatorLeft;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.PlayerUnregistered += OnPlayerUnregistered;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregistered()
        {
            AddServerMessage(ChatColor.Green, "*** You've unregistered successfully");
            IsRegistered = true;
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            string msg;
            if (reason == ConnectionLostReasons.ServerNotFound)
                msg = "*** Server not found";
            else
                msg = "*** Connection lost";
            AddServerMessage(ChatColor.Red, msg);
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
            AddServerMessage(ChatColor.Green, msg);
        }

        private void OnPlayerJoined(int playerid, string playerName, string team)
        {
            if (!String.IsNullOrWhiteSpace(team))
                AddServerMessage(ChatColor.Green, "*** {0} [{1}] has joined", playerName, team);
            else
                AddServerMessage(ChatColor.Green, "*** {0} has joined", playerName);
        }

        private void OnSpectatorLeft(int spectatorId, string spectatorName, LeaveReasons reason)
        {
            OnPlayerLeft(spectatorId, spectatorName, reason);
        }

        private void OnSpectatorJoined(int spectatorId, string spectatorName)
        {
            AddServerMessage(ChatColor.Green, "*** {0} has joined as spectator", spectatorName);
        }

        private void DisplayRegistrationFailMessage(RegistrationResults result, Versioning serverVersion)
        {
            string message = "*** You've FAILED registering !!!"; // defaulting
            switch (result)
            {
                case RegistrationResults.IncompatibleVersion:
                    if (serverVersion != null)
                        message = String.Format("*** You've FAILED registering !!! Application incompatible with server version {0}.{1}", serverVersion.Major, serverVersion.Minor);
                    else
                        message = String.Format("*** You've FAILED registering !!! Application incompatible with unknown server version");
                    break;
                case RegistrationResults.RegistrationFailedInvalidName:
                    message = "*** You've FAILED registering !!! Your name is not valid";
                    break;
                case RegistrationResults.RegistrationFailedPlayerAlreadyExists:
                    message = "*** You've FAILED registering !!! A player with the same name is already registered";
                    break;
                case RegistrationResults.RegistrationFailedTooManyPlayers:
                    message = "*** You've FAILED registering !!! Too many players already connected";
                    break;
                case RegistrationResults.RegistrationFailedInvalidId:
                    message = "*** You've FAILED registering !!! Internal server problem";
                    break;
            }
            AddServerMessage(ChatColor.Red, message);
        }

        private void DisplayRegistrationSuccessfullMessage(Versioning serverVersion)
        {
            AddServerMessage(ChatColor.Green, "*** You've registered successfully as {0} ({1})", Client.Name, Client.IsSpectator ? "Spectator" : "Player");
            if (serverVersion == null)
                AddServerMessage(ChatColor.Red, "Unknown server version. Use at your own risk!!!");
            else
                AddServerMessage(ChatColor.Red, "Server version: {0}.{1}", serverVersion.Major, serverVersion.Minor);
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                DisplayRegistrationSuccessfullMessage(serverVersion);
                IsRegistered = true;
            }
            else
                DisplayRegistrationFailMessage(result, serverVersion);
        }

        private void OnRegisteredAsSpectator(RegistrationResults result, Versioning serverVersion, int spectatorId)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                DisplayRegistrationSuccessfullMessage(serverVersion);
                IsRegistered = true;
            }
            else
                DisplayRegistrationFailMessage(result, serverVersion);
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            AddServerMessage(ChatColor.Orange, "*** {0} has WON", playerName);
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            AddServerMessage(ChatColor.Orange, "*** {0} has LOST", playerName);
        }

        private void OnGameResumed()
        {
            AddServerMessage(ChatColor.Yellow, "*** The game has been Resumed");
        }

        private void OnGamePaused()
        {
            AddServerMessage(ChatColor.Yellow, "*** The game has been Paused");
        }

        private void OnGameOver()
        {
            AddServerMessage(ChatColor.Orange, "*** You have LOST");
        }

        private void OnGameFinished(GameStatistics statistics)
        {
            AddServerMessage(ChatColor.Red, "*** The Game has Ended");
        }

        private void OnGameStarted()
        {
            AddServerMessage(ChatColor.Red, "*** The Game has Started");
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            AddEntry(new PlayerMessageEntry
                {
                    Color = ChatColor.Black,
                    PlayerName = playerName,
                    Message = msg
                });
        }

        private void OnServerPublishMessage(string msg)
        {
            AddServerMessage(ChatColor.Blue, msg);
        }

        private void OnAchievementEarned(IAchievement achievement, bool firstTime)
        {
            if (firstTime)
                AddEntry(new SelfAchievementEntry
                    {
                        Color = ChatColor.Blue,
                        Client = Client,
                        Achievement = achievement
                    });
        }

        private void OnPlayerAchievementEarned(int playerId, string playerName, int achievementId, string achievementTitle)
        {
            IAchievement achievement = Client.Achievements == null ? null : Client.Achievements.FirstOrDefault(x => x.Id == achievementId);

            if (achievement == null)
                AddEntry(new InvalidAchievementEntry
                    {
                        Color = ChatColor.Blue,
                        PlayerName = playerName,
                        AchievementTitle = achievementTitle
                    });
            else
                AddEntry(new OtherAchievementEntry
                    {
                        Color = ChatColor.Blue, 
                        Client = Client,
                        PlayerName = playerName, 
                        Achievement = achievement
                    });
        }

        #endregion
    }

    public class ChatViewModelDesignData : ChatViewModel
    {
        public ChatViewModelDesignData()
        {
            ChatEntries.Add(new PlayerMessageEntry
            {
                PlayerName = "Dummy1",
                Color = ChatColor.Green,
                Message = "Message with player name visible",
            });
            ChatEntries.Add(new ServerMessageEntry
            {
                Color = ChatColor.Red,
                Message = "Message without player name",
            });
            ChatEntries.Add(new InvalidAchievementEntry
            {
                Color = ChatColor.Blue,
                AchievementTitle = "Too good for you",
                PlayerName = "Dummy3",
            });
            for(int i = 0; i < 50; i++)
                ChatEntries.Add(new PlayerMessageEntry
                    {
                        Color = ChatColor.Orange,
                        PlayerName = "Player1",
                        Message = "A dummy message"
                    });
        }
    }
}