using System;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public delegate void AchievedHandler(IAchievement achievement, bool firstTime);

    public interface IAchievement
    {
        string Title { get; }
        string Description { get; }
        bool ResetOnGameStarted { get; } // default: true
        bool OnlyOnce { get; } // default: false

        bool IsAchievable { get; }

        string Progress { get; }
        bool IsProgressAvailable { get; }

        int AchieveCount { get; set; }
        bool IsAchieved { get; set; }
        DateTime FirstTimeAchieved { get; set; }
        DateTime LastTimeAchieved { get; set; }
        int ExtraData { get; set; } // can be used to store data between game session

        event AchievedHandler Achieved;

        void Reset();

        // Triggers
        void OnGameWon(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount);
        void OnGameLost(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount);
        void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnRoundFinished(int lineCompleted, int level, IBoard board);
    }
}
