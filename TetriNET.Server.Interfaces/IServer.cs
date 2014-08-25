using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Server.Interfaces
{
    public enum ServerStates
    {
        WaitingStartServer, // -> StartingServer
        StartingServer, // -> WaitingStartGame
        WaitingStartGame, // -> StartingGame
        StartingGame, // -> GameStarted
        GameStarted, // -> GameFinished | GamePaused
        GameFinished, // -> WaitingStartGame
        StoppingServer, // -> WaitingStartServer

        GamePaused, // -> GameStarted
    }

    public interface IServer
    {
        ServerStates State { get; }
        int SpecialId { get; }
        List<WinEntry> WinList { get; }
        Dictionary<string, GameStatisticsByPlayer> PlayerStatistics { get; } // By player (cannot be stored in IPlayer because IPlayer is lost when a player is disconnected during a game)
        GameOptions Options { get; }

        void StartServer();
        void StopServer();

        void StartGame();
        void StopGame();
        void PauseGame();
        void ResumeGame();

        void ResetWinList();
    }
}
