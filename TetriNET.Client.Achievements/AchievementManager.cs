using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;

namespace TetriNET.Client.Achievements
{
    public class AchievementManager : IAchievementManager
    {
        public AchievementManager()
        {
            //
            Achievements = new List<IAchievement>();
        }

        public void GetAllAchievements()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            GetAllAchievements(assembly);
        }

        public void GetAllAchievements(Assembly assembly)
        {
            //
            List<Type> types = assembly.GetTypes().Where(
                t => String.Equals(t.Namespace, "TetriNET.Client.Achievements.Achievements", StringComparison.Ordinal) 
                    && t.IsSubclassOf(typeof(Achievement))
                    && !t.IsAbstract).ToList();
            foreach (Type type in types)
            {
                try
                {
                    IAchievement achievement = Activator.CreateInstance(type) as IAchievement;
                    if (achievement != null)
                    {
                        Achievements.Add(achievement);
                        achievement.Achieved += AchievementAchieved;
                    }
                    else
                        Log.WriteLine(Log.LogLevels.Warning, "Achievement {0} cannot be instantiated and casted to right IAchievement", type.FullName);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(Log.LogLevels.Warning, "Achievement {0} cannot be instantiated. Exception: {1}", type.FullName, ex.ToString());
                }
            }
        }

        #region IAchievementManager
        public event AchievedHandler Achieved;

        private List<IAchievement> _achievements;
        public List<IAchievement> Achievements
        {
            get
            {
                return _achievements;
            }
            set
            {
                List<IAchievement> oldAchievements = _achievements;
                if (oldAchievements != null && oldAchievements.Any())
                    foreach (IAchievement achievement in oldAchievements)
                        achievement.Achieved -= AchievementAchieved;
                _achievements = value;
                if (_achievements != null && _achievements.Any())
                    foreach (IAchievement achievement in _achievements)
                        achievement.Achieved += AchievementAchieved;
            }
        }

        private DateTime _gameStartTime;

        public void Reset()
        {
            foreach (IAchievement achievement in Achievements)
            {
                achievement.IsAchieved = false;
                achievement.AchieveCount = 0;
            }
        }

        private void AchievementAchieved(IAchievement achievement, bool firstTime)
        {
            if (Achieved != null)
                Achieved(achievement, firstTime);
        }

        public void OnGameStarted()
        {
            _gameStartTime = DateTime.Now;
            foreach (IAchievement achievement in Achievements.Where(x => x.ResetOnGameStarted))
                achievement.Reset();
        }

        public void OnGameFinished()
        {
        }

        public void OnRoundFinished(int deletedRows, int level, IBoard board)
        {
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnRoundFinished(deletedRows, level, board);
        }

        public void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnUseSpecial(playerId, playerTeam, playerBoard, targetId, targetTeam, targetBoard, special);
        }

        public void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnSpecialUsed(playerId, sourceId, sourceTeam, sourceBoard, targetId, targetTeam, targetBoard, special);
        }

        public void OnGameOver(int moveCount, int linesCleared, int playingOpponentsInCurrentGame)
        {
            TimeSpan timeSpan = DateTime.Now - _gameStartTime;
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnGameLost(timeSpan.TotalSeconds, moveCount, linesCleared, playingOpponentsInCurrentGame);
        }

        public void OnGameWon(int moveCount, int linesCleared, int playingOpponentsInCurrentGame)
        {
            TimeSpan timeSpan = DateTime.Now - _gameStartTime;
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnGameWon(timeSpan.TotalSeconds, moveCount, linesCleared, playingOpponentsInCurrentGame);
        }
        #endregion
    }
}