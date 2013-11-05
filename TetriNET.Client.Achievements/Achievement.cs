using System;
using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements
{
    internal abstract class Achievement : IAchievement
    {
        private bool AlreadyAchievedThisGame { get; set; }

        protected bool IsFailed { get; set; }

        public int Id { get; protected set; }
        public int Points { get; protected set; }
        public string Title { get; protected set; }
        public string Description { get; protected set; }
        public bool ResetOnGameStarted { get; protected set; } // default: true
        public bool OnlyOnce { get; protected set; } // default: false

        public int AchieveCount { get; set; }
        public bool IsAchieved { get; set; }
        public DateTime FirstTimeAchieved { get; set; }
        public DateTime LastTimeAchieved { get; set; }
        public int ExtraData { get; set; } // can be used to store data between game session
       
        public bool AchievedMoreThanOnce
        {
            get { return AchieveCount > 1; }
        }

        public bool IsAchievable {
            get
            {
                return (OnlyOnce && !IsAchieved) || (!OnlyOnce && !IsFailed && !AlreadyAchievedThisGame);
            }
        }

        public virtual string Progress
        {
            get { return String.Empty; }
        }

        public virtual bool IsProgressAvailable
        {
            get { return !String.IsNullOrWhiteSpace(Progress); }
        }

        public event AchievedHandler Achieved;

        protected Achievement()
        {
            IsFailed = false;
            AlreadyAchievedThisGame = false;
            ResetOnGameStarted = true; // default: true
            OnlyOnce = false; // default: false
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
        public virtual void OnGameStarted(GameOptions options)
        {
        }

        public virtual void OnAchievementEarned(IAchievement achievement, IEnumerable<IAchievement> achievements)
        {
        }

        public virtual void OnGameWon(double playTime/*in seconds*/, int moveCount, int lineCount, int playerCount)
        {
        }

        public virtual void OnGameLost(double playTime/*in seconds*/, int moveCount, int lineCount, int playerCount, int playerLeft, List<Specials> inventory)
        {
        }

        public virtual void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
        }

        public virtual void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
        }

        public virtual void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
        {
        }
    }
}
