using System;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public delegate void OnAchievedHandler(IAchievement achievement, bool firstTime);

    public interface IAchievement
    {
        string Title { get; }
        string Description { get; }
        bool ResetOnGameStarted { get; } // default: true

        int AchieveCount { get; set; }
        bool IsAchieved { get; set; }
        DateTime FirstTimeAchieved { get; set; }
        DateTime LastTimeAchieved { get; set; }

        bool IsAchievable { get; }

        event OnAchievedHandler OnAchieved;

        void Reset();

        // Triggers
        void OnGameWon(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount);
        void OnGameLost(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount);
        void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnUseSpecial(int playerId, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnRoundFinished(int lineCompleted);
    }
}
