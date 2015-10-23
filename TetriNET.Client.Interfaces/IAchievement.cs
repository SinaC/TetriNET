﻿using System;
using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public delegate void AchievedEventHandler(IAchievement achievement, bool firstTime);

    public interface IAchievement
    {
        int Id { get; }
        int Points { get; }
        string Title { get; }
        string Description { get; }
        bool ResetOnGameStarted { get; } // default: true
        bool OnlyOnce { get; } // default: false

        int AchieveCount { get; set; }
        bool IsAchieved { get; set; }
        DateTime FirstTimeAchieved { get; set; }
        DateTime LastTimeAchieved { get; set; }
        int ExtraData { get; set; } // can be used to store data between game session

        bool IsAchievable { get; }
        bool AchievedMoreThanOnce { get; }

        bool IsGoldLevelReached { get; }
        bool IsSilverLevelReached { get; }
        bool IsBronzeLevelReached { get; }

        string Progress { get; }
        bool IsProgressAvailable { get; }

        event AchievedEventHandler Achieved;

        void Reset();

        // Triggers
        void OnGameStarted(GameOptions options);
        void OnAchievementEarned(IAchievement achievement, IReadOnlyCollection<IAchievement> achievements);
        void OnGameWon(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount);
        void OnGameLost(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount, int playerLeft, IReadOnlyCollection<Specials> inventory);
        void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IReadOnlyBoard sourceBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special);
        void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special);
        void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces);
    }
}
