using System;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements
{
    internal abstract class Achievement : IAchievement
    {
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public bool ResetOnGameStarted { get; protected set; } // default: true

        public int AchieveCount { get; set; }
        public bool IsAchieved { get; set; }
        public DateTime FirstTimeAchieved { get; set; }
        public DateTime LastTimeAchieved { get; set; }

        public bool IsFailed { get; protected set; }
        public bool AlreadyAchievedThisGame { get; protected set; }

        public bool IsAchievable {
            get
            {
                return !IsFailed && !AlreadyAchievedThisGame;
            }
        }
        
        public event AchievedHandler Achieved;

        protected Achievement()
        {
            IsFailed = false;
            ResetOnGameStarted = true;
        }

        public virtual void Reset()
        {
            IsFailed = false;
            AlreadyAchievedThisGame = false;
        }

        public virtual void Achieve()
        {
            bool firstTime = false;
            DateTime now = DateTime.Now;
            if (!IsAchieved)
            {
                FirstTimeAchieved = now;
                firstTime = true;
            }
            LastTimeAchieved = now;
            IsAchieved = true;
            AchieveCount++;

            Reset();

            AlreadyAchievedThisGame = true;

            if (Achieved != null)
                Achieved(this, firstTime);
        }

        // Triggers
        public virtual void OnGameWon(double playTime/*in seconds*/, int moveCount, int lineCount, int playerCount)
        {
        }

        public virtual void OnGameLost(double playTime/*in seconds*/, int moveCount, int lineCount, int playerCount)
        {
        }

        public virtual void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
        }

        public virtual void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
        }

        public virtual void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
        }
    }
}
