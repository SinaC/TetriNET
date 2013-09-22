using System;
using System.Collections.Generic;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Interfaces
{
    public enum ConnectionLostReasons
    {
        ServerNotFound,
        Other,
    }

    public delegate void ClientConnectionLostHandler(ConnectionLostReasons reason);

    public delegate void ClientRoundStartedHandler();
    public delegate void ClientRoundFinishedHandler();
    public delegate void ClientStartGameHandler();
    public delegate void ClientFinishGameHandler();
    public delegate void ClientPauseGameHandler();
    public delegate void ClientResumeGameHandler();
    public delegate void ClientGameOverHandler();
    
    public delegate void ClientRedrawHandler();
    public delegate void ClientRedrawBoardHandler(int playerId, IBoard board);
    public delegate void ClientPieceMovingHandler();
    public delegate void ClientPieceMovedHandler();
    public delegate void ClientPlayerRegisteredHandler(RegistrationResults result, int playerId);
    public delegate void ClientPlayerUnregisteredHandler();
    public delegate void ClientWinListModifiedHandler(List<WinEntry> winList);
    public delegate void ClientServerMasterModifiedHandler(int serverMasterId);
    public delegate void ClientPlayerLostHandler(int playerId, string playerName);
    public delegate void ClientPlayerWonHandler(int playerId, string playerName);
    public delegate void ClientPlayerJoinedHandler(int playerId, string playerName);
    public delegate void ClientPlayerLeftHandler(int playerId, string playerName, LeaveReasons reason);
    public delegate void ClientPlayerPublishMessageHandler(string playerName, string msg);
    public delegate void ClientServerPublishMessageHandler(string msg);
    public delegate void ClientInventoryChangedHandler();
    public delegate void ClientLinesClearedChangedHandler();
    public delegate void ClientLevelChangedHandler();
    public delegate void ClientSpecialUsedHandler(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special);
    public delegate void ClientPlayerAddLines(string playerName, int specialId, int count);
    public delegate void ClientToggleDarkness(bool active);
    public delegate void ClientToggleConfusion(bool active);
    public delegate void ClientToggleImmunity(bool active);
    public delegate void ClientContinuousSpecialFinishedHandler(int playerId, Specials special);

    public interface IClient
    {
        string Name { get; }
        int PlayerId { get; }
        int MaxPlayersCount { get; }
        IPiece CurrentPiece { get; }
        IPiece NextPiece { get; }
        IBoard Board { get; }
        List<Specials> Inventory { get; }
        int LinesCleared { get; }
        int Level { get; }
        bool IsRegistered { get; }
        bool IsGamePaused { get; } // Server-state
        bool IsGameStarted { get; } // Server-state
        bool IsPlaying { get; }
        int InventorySize { get; }
        GameOptions Options { get; }
        bool IsServerMaster { get; }

        //IBoard GetBoard(int playerId);
        //bool IsPlaying(int playerId);
        IEnumerable<IOpponent> Opponents { get; }
        IClientStatistics Statistics { get; }

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
        event ClientPlayerRegisteredHandler OnPlayerRegistered;
        event ClientPlayerUnregisteredHandler OnPlayerUnregistered;
        event ClientWinListModifiedHandler OnWinListModified;
        event ClientServerMasterModifiedHandler OnServerMasterModified;
        event ClientPlayerLostHandler OnPlayerLost;
        event ClientPlayerWonHandler OnPlayerWon;
        event ClientPlayerJoinedHandler OnPlayerJoined;
        event ClientPlayerLeftHandler OnPlayerLeft;
        event ClientPlayerPublishMessageHandler OnPlayerPublishMessage;
        event ClientServerPublishMessageHandler OnServerPublishMessage;
        event ClientInventoryChangedHandler OnInventoryChanged;
        event ClientLinesClearedChangedHandler OnLinesClearedChanged;
        event ClientLevelChangedHandler OnLevelChanged;
        event ClientSpecialUsedHandler OnSpecialUsed;
        event ClientPlayerAddLines OnPlayerAddLines;
        event ClientToggleDarkness OnDarknessToggled;
        event ClientToggleConfusion OnConfusionToggled;
        event ClientToggleImmunity OnImmunityToggled;
        event ClientContinuousSpecialFinishedHandler OnContinuousSpecialFinished;

        bool Connect(Func<ITetriNETCallback, IProxy> createProxyFunc);
        bool Disconnect();

        // Client->Server command
        void Register(string name);
        void Unregister();
        void StartGame();
        void StopGame();
        void PauseGame();
        void ResumeGame();
        void ResetWinList();
        void ChangeOptions(GameOptions options);
        void KickPlayer(int playerId);
        void BanPlayer(int playerId);
        void PublishMessage(string msg);

        // Game controller
        void Drop();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();
        void DiscardFirstSpecial();
        bool UseSpecial(int targetId);

        //
        void Dump();
    }
}
