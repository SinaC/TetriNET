using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class CantTouchThis : Achievement
    {
        private readonly int _achievementsCount;

        public CantTouchThis()
        {
            Id = 5;
            Points = 100;
            Title = "Can't Touch This";
            Description = "Earn all achievements";
            OnlyOnce = true;

            _achievementsCount = Assembly.GetExecutingAssembly().GetTypes().Count(t => t.IsSubclassOf(typeof(Achievement)) && !t.IsAbstract);
        }

        public override void Reset()
        {
            if (IsAchieved && ExtraData != _achievementsCount)
                IsAchieved = false;
            base.Reset();
        }

        public override void OnAchievementEarned(IAchievement achievement, IEnumerable<IAchievement> achievements)
        {
            List<IAchievement> lst = achievements.ToList();
            int count = lst.Where(x => x.Id != achievement.Id).Count(x => x.IsAchieved);
            if (count == lst.Count())
            {
                Achieve();
                ExtraData = _achievementsCount;
            }
        }
    }
}
