using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public enum ConnectionLostReasons
    {
        ServerNotFound,
        Other,
    }

    public delegate void ClientConnectionLostEventHandler(ConnectionLostReasons reason);

    public delegate void ClientRoundStartedEventHandler();
    public delegate void ClientRoundFinishedEventHandler(int deletedRows);
    public delegate void ClientStartGameEventHandler();
    public delegate void ClientFinishGameEventHandler(GameStatistics statistics);
    public delegate void ClientPauseGameEventHandler();
    public delegate void ClientResumeGameEventHandler();
    public delegate void ClientGameOverEventHandler();
    
    public delegate void ClientRedrawEventHandler();
    public delegate void ClientRedrawBoardEventHandler(int playerId, IBoard board);
    public delegate void ClientPieceMovingEventHandler();
    public delegate void ClientPieceMovedEventHandler();
    public delegate void ClientNextPieceModifiedEventHandler();
    public delegate void ClientHoldPieceModifiedEventHandler();
    public delegate void ClientRegisteredAsPlayerEventHandler(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster);
    public delegate void ClientPlayerUnregisteredEventHandler();
    public delegate void ClientWinListModifiedEventHandler(List<WinEntry> winList);
    public delegate void ClientServerMasterModifiedEventHandler(int serverMasterId);
    public delegate void ClientPlayerLostEventHandler(int playerId, string playerName);
    public delegate void ClientPlayerWonEventHandler(int playerId, string playerName);
    public delegate void ClientPlayerJoinedEventHandler(int playerId, string playerName, string team);
    public delegate void ClientPlayerLeftEventHandler(int playerId, string playerName, LeaveReasons reason);
    public delegate void ClientPlayerTeamChangedEventHandler(int playerId, string team);
    public delegate void ClientPlayerPublishMessageEventHandler(string playerName, string msg);
    public delegate void ClientServerPublishMessageEventHandler(string msg);
    public delegate void ClientInventoryChangedEventHandler();
    public delegate void ClientLinesClearedChangedEventHandler(int linesCleared);
    public delegate void ClientLevelChangedEventHandler(int level);
    public delegate void ClientScoreChangedEventHandler(int score);
    public delegate void ClientSpecialUsedEventHandler(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special);
    public delegate void ClientUseSpecialEventHandler(int targetId, string targetName, Specials special);
    public delegate void ClientPlayerAddLinesEventHandler(int playerId, string playerName, int specialId, int count);
    public delegate void ClientContinuousSpecialToggledEventHandler(Specials special, bool active, double durationLeftInSeconds);
    public delegate void ClientContinuousSpecialFinishedEventHandler(int playerId, Specials special);
    public delegate void ClientAchievementEarnedEventHandler(IAchievement achievement, bool firstTime);
    public delegate void ClientPlayerAchievementEarnedEventHandler(int playerId, string playerName, int achievementId, string achievementTitle);
    public delegate void ClientOptionsChangedEventHandler();
    public delegate void ClientRegisteredAsSpectatorEventHandler(RegistrationResults result, Versioning serverVersion, int spectatorId);
    public delegate void ClientSpectatorJoinedEventHandler(int spectatorId, string spectatorName);
    public delegate void ClientSpectatorLeftEventHandler(int spectatorId, string spectatorName, LeaveReasons reason);

    public interface IClient
    {
        string Name { get; }
        string Team { get; }
        bool IsSpectator { get; }
        int PlayerId { get; }
        int MaxPlayersCount { get; }
        IPiece CurrentPiece { get; }
        IPiece NextPiece { get; }
        IPiece HoldPiece { get; }
        IBoard Board { get; }
        IReadOnlyCollection<Specials> Inventory { get; }
        int LinesCleared { get; }
        int Level { get; }
        int Score { get; }
        bool IsRegistered { get; }
        bool IsGamePaused { get; } // Server-state
        bool IsGameStarted { get; } // Server-state
        bool IsPlaying { get; }
        int InventorySize { get; }
        GameOptions Options { get; }
        bool IsServerMaster { get; }
        int PlayingOpponentsInCurrentGame { get; }

        Versioning Version { get; }

        IReadOnlyCollection<IOpponent> Opponents { get; }
        IClientStatistics Statistics { get; }
        IReadOnlyCollection<IAchievement> Achievements { get; }

        event ClientConnectionLostEventHandler ConnectionLost;

        event ClientRoundStartedEventHandler RoundStarted;
        event ClientRoundFinishedEventHandler RoundFinished;
        event ClientStartGameEventHandler GameStarted;
        event ClientFinishGameEventHandler GameFinished;
        event ClientPauseGameEventHandler GamePaused;
        event ClientResumeGameEventHandler GameResumed;
        event ClientGameOverEventHandler GameOver;
        event ClientRedrawEventHandler Redraw;
        event ClientRedrawBoardEventHandler RedrawBoard;
        event ClientPieceMovingEventHandler PieceMoving;
        event ClientPieceMovedEventHandler PieceMoved;
        event ClientNextPieceModifiedEventHandler NextPieceModified;
        event ClientHoldPieceModifiedEventHandler HoldPieceModified;
        event ClientRegisteredAsPlayerEventHandler RegisteredAsPlayer;
        event ClientPlayerUnregisteredEventHandler PlayerUnregistered;
        event ClientWinListModifiedEventHandler WinListModified;
        event ClientServerMasterModifiedEventHandler ServerMasterModified;
        event ClientPlayerLostEventHandler PlayerLost;
        event ClientPlayerWonEventHandler PlayerWon;
        event ClientPlayerJoinedEventHandler PlayerJoined;
        event ClientPlayerLeftEventHandler PlayerLeft;
        event ClientPlayerTeamChangedEventHandler PlayerTeamChanged;
        event ClientPlayerPublishMessageEventHandler PlayerPublishMessage;
        event ClientServerPublishMessageEventHandler ServerPublishMessage;
        event ClientInventoryChangedEventHandler InventoryChanged;
        event ClientLinesClearedChangedEventHandler LinesClearedChanged;
        event ClientLevelChangedEventHandler LevelChanged;
        event ClientScoreChangedEventHandler ScoreChanged;
        event ClientSpecialUsedEventHandler SpecialUsed;
        event ClientUseSpecialEventHandler UseSpecial;
        event ClientPlayerAddLinesEventHandler PlayerAddLines;
        event ClientContinuousSpecialToggledEventHandler ContinuousEffectToggled; // on player
        event ClientContinuousSpecialFinishedEventHandler ContinuousSpecialFinished; // on opponent
        event ClientAchievementEarnedEventHandler AchievementEarned;
        event ClientPlayerAchievementEarnedEventHandler PlayerAchievementEarned;
        event ClientOptionsChangedEventHandler OptionsChanged;
        event ClientRegisteredAsSpectatorEventHandler RegisteredAsSpectator;
        event ClientSpectatorJoinedEventHandler SpectatorJoined;
        event ClientSpectatorLeftEventHandler SpectatorLeft;

        //bool Connect(Func<ITetriNETCallback, IProxy> createProxyFunc);
        //bool Disconnect();

        void SetVersion(int major, int minor);

        bool ConnectAndRegisterAsPlayer(string address, string name, string team);
        bool ConnectAndRegisterAsSpectator(string address, string name);
        bool UnregisterAndDisconnect();

        // Client->Server command
        //void Register(string name);
        //void Unregister();
        void StartGame();
        void StopGame();
        void PauseGame();
        void ResumeGame();
        void ResetWinList();
        void ChangeTeam(string team);
        void ChangeOptions(GameOptions options);
        void KickPlayer(int playerId);
        void BanPlayer(int playerId);
        void PublishMessage(string msg);

        // Game controller
        void Hold();
        void Drop();
        void MoveDown(bool automatic = false);
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();
        void DiscardFirstSpecial();
        bool UseFirstSpecial(int targetId);

        // Achievement
        void ResetAchievements();

        //
        void Dump();
    }
}
