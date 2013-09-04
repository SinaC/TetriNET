using System.Collections.Generic;
using System.ServiceModel;
using TetriNET.Common.GameDatas;

namespace TetriNET.Common.Contracts
{
    public interface ITetriNETCallback
    {
        // Player connexion/deconnexion
        [OperationContract(IsOneWay = true)]
        void OnHeartbeatReceived();

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted); // Player Number + In-Game

        [OperationContract(IsOneWay = true)] // Player Joined
        void OnPlayerJoined(int playerId, string name);

        [OperationContract(IsOneWay = true)] // Player Left
        void OnPlayerLeft(int playerId, string name, LeaveReasons reason);

        // Chat
        [OperationContract(IsOneWay = true)] // Partyline Chat Message
        void OnPublishPlayerMessage(string playerName, string msg);

        [OperationContract(IsOneWay = true)] // Game Chat Message
        void OnPublishServerMessage(string msg);

        // In-game
        [OperationContract(IsOneWay = true)] // Player Lost
        void OnPlayerLost(int playerId);

        [OperationContract(IsOneWay = true)] // Player Won
        void OnPlayerWon(int playerId);

        [OperationContract(IsOneWay = true)] // New Game
        void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, Tetriminos thirdTetrimino, GameOptions options);

        [OperationContract(IsOneWay = true)] // End Game
        void OnGameFinished();

        [OperationContract(IsOneWay = true)] // Pause Game
        void OnGamePaused();

        [OperationContract(IsOneWay = true)] // Resume Game
        void OnGameResumed();

        [OperationContract(IsOneWay = true)]
        void OnServerAddLines(int lineCount);

        [OperationContract(IsOneWay = true)] // Classic-Style Add-Lines
        void OnPlayerAddLines(int specialId, int playerId, int lineCount);

        [OperationContract(IsOneWay = true)]
        void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special); // Special Used

        [OperationContract(IsOneWay = true)]
        void OnNextTetrimino(int index, Tetriminos tetrimino); // TODO: send 3 next tetriminoes

        [OperationContract(IsOneWay = true)] // Field Update
        void OnGridModified(int playerId, byte[] grid);

        // Server master command
        [OperationContract(IsOneWay = true)]
        void OnServerMasterChanged(int playerId);

        [OperationContract(IsOneWay = true)] // Win list
        void OnWinListModified(List<WinEntry> winList);
    }
}
