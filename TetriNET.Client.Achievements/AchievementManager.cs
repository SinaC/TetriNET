using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Achievements.Achievements;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements
{
    public class AchievementManager : IAchievementManager
    {
        public event OnAchievedHandler OnAchieved;

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
                        achievement.OnAchieved -= AchievementOnAchieved;
                _achievements = value;
                if (_achievements != null && _achievements.Any())
                    foreach (IAchievement achievement in _achievements)
                        achievement.OnAchieved += AchievementOnAchieved;
            }
        }

        private IClient _client;

        public IClient Client
        {
            get { return _client; }
            set
            {
                IClient oldClient = _client;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= OnGameStarted;
                    oldClient.OnGameFinished -= OnGameFinished;
                    oldClient.OnPlayerWon -= OnPlayerWon;
                    oldClient.OnGameOver -= OnGameOver;
                    oldClient.OnSpecialUsed -= SpecialUsed;
                    oldClient.OnRoundFinished -= OnRoundFinished;
                    oldClient.OnUseSpecial -= UseSpecial;
                }
                _client = value;
                if (_client != null)
                {
                    _client.OnGameStarted += OnGameStarted;
                    _client.OnGameFinished += OnGameFinished;
                    _client.OnPlayerWon += OnPlayerWon;
                    _client.OnGameOver += OnGameOver;
                    _client.OnSpecialUsed += SpecialUsed;
                    _client.OnRoundFinished += OnRoundFinished;
                    _client.OnUseSpecial += UseSpecial;
                }
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

        private void AchievementOnAchieved(IAchievement achievement, bool firstTime)
        {
            if (OnAchieved != null)
                OnAchieved(achievement, firstTime);
        }

        private void OnGameStarted()
        {
            _gameStartTime = DateTime.Now;
            foreach (IAchievement achievement in Achievements.Where(x => x.ResetOnGameStarted))
                achievement.Reset();
        }

        private void OnGameFinished()
        {
        }

        private void OnRoundFinished(int deletedRows)
        {
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnRoundFinished(deletedRows);
        }

        private void UseSpecial(int targetId, string targetName, Specials special)
        {
            // Target
            string targetTeam = null;
            IBoard targetBoard = null;
            if (Client.PlayerId == targetId)
            {
                targetTeam = Client.Team;
                targetBoard = Client.Board == null ? null : Client.Board.Clone();
            }
            else
            {
                IOpponent target = Client.Opponents.FirstOrDefault(x => x.PlayerId == targetId);
                if (target != null)
                {
                    targetTeam = target.Team;
                    targetBoard = target.Board == null ? null : target.Board.Clone();
                }
            }
            //
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnUseSpecial(Client.PlayerId, targetId, targetTeam, targetBoard, special);
        }

        private void SpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            // Source
            string sourceTeam = null;
            IBoard sourceBoard = null;
            if (Client.PlayerId == playerId)
            {
                sourceTeam = Client.Team;
                sourceBoard = Client.Board == null ? null : Client.Board.Clone();
            }
            else
            {
                IOpponent source = Client.Opponents.FirstOrDefault(x => x.PlayerId == playerId);
                if (source != null)
                {
                    sourceTeam = source.Team;
                    sourceBoard = source.Board == null ? null : source.Board.Clone();
                }
            }
            // Target
            string targetTeam = null;
            IBoard targetBoard = null;
            if (Client.PlayerId == targetId)
            {
                targetTeam = Client.Team;
                targetBoard = Client.Board == null ? null : Client.Board.Clone();
            }
            else
            {
                IOpponent target = Client.Opponents.FirstOrDefault(x => x.PlayerId == targetId);
                if (target != null)
                {
                    targetTeam = target.Team;
                    targetBoard = target.Board == null ? null : target.Board.Clone();
                }
            }
            //
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnSpecialUsed(Client.PlayerId, playerId, sourceTeam, sourceBoard, targetId, targetTeam, targetBoard, special);
        }

        private void OnGameOver()
        {
            TimeSpan timeSpan = DateTime.Now - _gameStartTime;
            foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                achievement.OnGameLost(timeSpan.TotalSeconds, Client.Statistics.MoveCount, Client.LinesCleared, Client.PlayingOpponentsInCurrentGame);
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            TimeSpan timeSpan = DateTime.Now - _gameStartTime;
            if (playerId == Client.PlayerId)
                foreach (IAchievement achievement in Achievements.Where(x => x.IsAchievable))
                    achievement.OnGameWon(timeSpan.TotalSeconds, Client.Statistics.MoveCount, Client.LinesCleared, Client.PlayingOpponentsInCurrentGame);
        }
    }
}