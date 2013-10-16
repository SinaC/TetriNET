using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.CustomSettings
{
    [Serializable]
    public class AchievementSettings
    {
        public string Title;
        public int Count;
        public DateTime FirstTime;
        public DateTime LastTime;
    }

    [Serializable]
    public class AchievementsSettings
    {
        public AchievementSettings[] Achievements;

        // Build settings from achievements
        public void Set(List<IAchievement> achievements)
        {
            if (achievements == null)
                return;
            Achievements = achievements.Select(x => new AchievementSettings
            {
                Title = x.Title,
                Count = x.AchieveCount,
                FirstTime = x.FirstTimeAchieved,
                LastTime = x.LastTimeAchieved
            }).ToArray();
        }

        // Overwrite achievements data with settings
        public void Get(List<IAchievement> achievements)
        {
            if (achievements == null || Achievements == null)
                return;
            foreach (AchievementSettings setting in Achievements)
            {
                IAchievement achievement = achievements.FirstOrDefault(x => x.Title == setting.Title);
                if (achievement != null)
                {
                    achievement.IsAchieved = setting.Count > 0;
                    achievement.AchieveCount = setting.Count;
                    achievement.FirstTimeAchieved = setting.FirstTime;
                    achievement.LastTimeAchieved = setting.LastTime;
                }
            }
        }
    }
}
