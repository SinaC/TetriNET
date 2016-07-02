using System;
using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements
{
    public abstract class AchievementBase : IAchievement
    {
        private bool AlreadyAchievedThisGame { get; set; }

        protected bool IsFailed { get; set; }

        protected int BronzeLevel { get; set; }
        protected int SilverLevel { get; set; }
        protected int GoldLevel { get; set; }

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

        //public string LevelColor // Color is UI business
        //{
        //    get { return AchieveCount >= GoldLevel ? "Gold" 
        //        : AchieveCount >= SilverLevel ? "Silver"
        //        : AchieveCount >= BronzeLevel ? "#FFD3712D" 
        //        : "Transparent"; }
        //}

        public bool IsGoldLevelReached => AchieveCount >= GoldLevel;

        public bool IsSilverLevelReached => AchieveCount >= SilverLevel;

        public bool IsBronzeLevelReached => AchieveCount >= BronzeLevel;

        public bool AchievedMoreThanOnce => AchieveCount > 1;

        public bool IsAchievable => (OnlyOnce && !IsAchieved) || (!OnlyOnce && !IsFailed && !AlreadyAchievedThisGame);

        public virtual string Progress => String.Empty;

        public virtual bool IsProgressAvailable => !String.IsNullOrWhiteSpace(Progress);

        public event AchievedEventHandler Achieved;

        protected AchievementBase()
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

            Achieved?.Invoke(this, firstTime);
        }

        // Triggers
        public virtual void OnGameStarted(GameOptions options)
        {
        }

        public virtual void OnAchievementEarned(IAchievement achievement, IReadOnlyCollection<IAchievement> achievements)
        {
        }

        public virtual void OnGameWon(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount)
        {
        }

        public virtual void OnGameLost(double playTime /*in seconds*/, int moveCount, int lineCount, int playerCount, int playerLeft, IReadOnlyCollection<Specials> inventory)
        {
        }

        public virtual void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IReadOnlyBoard sourceBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
        }

        public virtual void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
        }

        public virtual void OnRoundFinished(int lineCompleted, int level, int moveCount, int score, IReadOnlyBoard board, IReadOnlyCollection<Pieces> collapsedPieces)
        {
        }
    }
}
