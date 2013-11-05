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

        public void FindAllAchievements()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FindAllAchievements(assembly);
        }

        public void FindAllAchievements(Assembly assembly)
        {
            //
            List<Type> types = assembly.GetTypes().Where( t=> t.IsSubclassOf(typeof(Achievement)) && !t.IsAbstract).ToList();
            foreach (Type type in types)
            {
                try
                {
                    IAchievement achievement = Activator.CreateInstance(type) as IAchievement;
                    if (achievement != null)
                    {
                        IAchievement alreadyExists = Achievements.FirstOrDefault(x => x.Id == achievement.Id);
                        if (alreadyExists != null)
                            Log.WriteLine(Log.LogLevels.Error, "Achievement {0} and {1} share the same id {2}", achievement.Title, alreadyExists.Title, achievement.Id);
                        else
                        {
                            Achievements.Add(achievement);
                            achievement.Achieved += AchievementAchieved;
                        }
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
                achievement.ExtraData = 0;
            }
        }

        private void AchievementAchieved(IAchievement achievement, bool firstTime)
        {
            if (Achieved != null)
                Achieved(achievement, firstTime);

            foreach (IAchievement iter in Achievements.Where(x => x.IsAchievable))
                iter.OnAchievementEarned(achievement, Achievements);
        }

        public void OnGameStarted(GameOptions options)
        {
            _gameStartTime = DateTime.Now;
            foreach (IAchievement achievement in Achievements)
            {
                if (achievement.ResetOnGameStarted)
                    achievement.Reset();
                achievement.OnGameStarted(options);
            }
        }

        public void OnGameFinished()
        {
        }

        public void OnRoundFinished(int deletedRows, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces)
        {
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnRoundFinished(deletedRows, level, moveCount, score, board, collapsedPieces);
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

        public void OnGameOver(int moveCount, int linesCleared, int playingOpponentsInCurrentGame, int playingOpponentsLeftInCurrentGame, IEnumerable<Specials> inventory)
        {
            List<Specials> lst = inventory.ToList();
            TimeSpan timeSpan = DateTime.Now - _gameStartTime;
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnGameLost(timeSpan.TotalSeconds, moveCount, linesCleared, playingOpponentsInCurrentGame, playingOpponentsLeftInCurrentGame, lst);
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