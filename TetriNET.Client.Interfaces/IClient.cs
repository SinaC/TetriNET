using System;
using System.Collections.Generic;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public enum ConnectionLostReasons
    {
        ServerNotFound,
        Other,
    }

    public delegate void ClientConnectionLostHandler(ConnectionLostReasons reason);

    public delegate void ClientRoundStartedHandler();
    public delegate void ClientRoundFinishedHandler(int deletedRows);
    public delegate void ClientStartGameHandler();
    public delegate void ClientFinishGameHandler();
    public delegate void ClientPauseGameHandler();
    public delegate void ClientResumeGameHandler();
    public delegate void ClientGameOverHandler();
    
    public delegate void ClientRedrawHandler();
    public delegate void ClientRedrawBoardHandler(int playerId, IBoard board);
    public delegate void ClientPieceMovingHandler();
    public delegate void ClientPieceMovedHandler();
    public delegate void ClientNextPieceModifiedHandler();
    public delegate void ClientHoldPieceModifiedHandler();
    public delegate void ClientPlayerRegisteredHandler(RegistrationResults result, int playerId, bool isServerMaster);
    public delegate void ClientPlayerUnregisteredHandler();
    public delegate void ClientWinListModifiedHandler(List<WinEntry> winList);
    public delegate void ClientServerMasterModifiedHandler(int serverMasterId);
    public delegate void ClientPlayerLostHandler(int playerId, string playerName);
    public delegate void ClientPlayerWonHandler(int playerId, string playerName);
    public delegate void ClientPlayerJoinedHandler(int playerId, string playerName);
    public delegate void ClientPlayerLeftHandler(int playerId, string playerName, LeaveReasons reason);
    public delegate void ClientPlayerTeamChangedHandler(int playerId, string team);
    public delegate void ClientPlayerPublishMessageHandler(string playerName, string msg);
    public delegate void ClientServerPublishMessageHandler(string msg);
    public delegate void ClientInventoryChangedHandler();
    public delegate void ClientLinesClearedChangedHandler(int linesCleared);
    public delegate void ClientLevelChangedHandler(int level);
    public delegate void ClientScoreChangedHandler(int score);
    public delegate void ClientSpecialUsedHandler(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special);
    public delegate void ClientUseSpecialHandler(int targetId, string targetName, Specials special);
    public delegate void ClientPlayerAddLinesHandler(int playerId, string playerName, int specialId, int count);
    public delegate void ClientContinuousSpecialToggledHandler(Specials special, bool active, double durationLeftInSeconds);
    public delegate void ClientContinuousSpecialFinishedHandler(int playerId, Specials special);

    public delegate void ClientAchievementEarnedHandler(IAchievement achievement, bool firstTime);
    public delegate void ClientPlayerAchievementEarnedHandler(int playerId, string playerName, int achievementId, string achievementTitle);

    public interface IClient
    {
        string Name { get; }
        string Team { get; }
        int PlayerId { get; }
        int MaxPlayersCount { get; }
        IPiece CurrentPiece { get; }
        IPiece NextPiece { get; }
        IPiece HoldPiece { get; }
        IBoard Board { get; }
        List<Specials> Inventory { get; }
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

        IEnumerable<IOpponent> Opponents { get; }
        IClientStatistics Statistics { get; }
        IEnumerable<IAchievement> Achievements { get; }

        event ClientConnectionLostHandler OnConnectionLost;

        event ClientRoundStartedHandler OnRoundStarted;
        event ClientRoundFinishedHandler OnRoundFinished;
        event ClientStartGameHandler OnGameStarted;
        event ClientFinishGameHandler OnGameFinished;
        event ClientPauseGameHandler OnGamePaused;
        event ClientResumeGameHandler OnGameResumed;
        event ClientGameOverHandler OnGameOver;
        event ClientRedrawHandler OnRedraw;
        event ClientRedrawBoardHandler OnRedrawBoard;
        event ClientPieceMovingHandler OnPieceMoving;
        event ClientPieceMovedHandler OnPieceMoved;
        event ClientNextPieceModifiedHandler OnNextPieceModified;
        event ClientHoldPieceModifiedHandler OnHoldPieceModified;
        event ClientPlayerRegisteredHandler OnPlayerRegistered;
        event ClientPlayerUnregisteredHandler OnPlayerUnregistered;
        event ClientWinListModifiedHandler OnWinListModified;
        event ClientServerMasterModifiedHandler OnServerMasterModified;
        event ClientPlayerLostHandler OnPlayerLost;
        event ClientPlayerWonHandler OnPlayerWon;
        event ClientPlayerJoinedHandler OnPlayerJoined;
        event ClientPlayerLeftHandler OnPlayerLeft;
        event ClientPlayerTeamChangedHandler OnPlayerTeamChanged;
        event ClientPlayerPublishMessageHandler OnPlayerPublishMessage;
        event ClientServerPublishMessageHandler OnServerPublishMessage;
        event ClientInventoryChangedHandler OnInventoryChanged;
        event ClientLinesClearedChangedHandler OnLinesClearedChanged;
        event ClientLevelChangedHandler OnLevelChanged;
        event ClientScoreChangedHandler OnScoreChanged;
        event ClientSpecialUsedHandler OnSpecialUsed;
        event ClientUseSpecialHandler OnUseSpecial;
        event ClientPlayerAddLinesHandler OnPlayerAddLines;
        event ClientContinuousSpecialToggledHandler OnContinuousEffectToggled; // on player
        event ClientContinuousSpecialFinishedHandler OnContinuousSpecialFinished; // on opponent

        event ClientAchievementEarnedHandler OnAchievementEarned;
        event ClientPlayerAchievementEarnedHandler OnPlayerAchievementEarned;

        //bool Connect(Func<ITetriNETCallback, IProxy> createProxyFunc);
        //bool Disconnect();

        bool ConnectAndRegister(Func<ITetriNETCallback, IProxy> createProxyFunc, string name);
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
        bool UseSpecial(int targetId);

        // Achievement
        void ResetAchievements();

        //
        void Dump();
    }
}
