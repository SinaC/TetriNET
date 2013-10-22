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
            if (IsAchieved && ExtraData < _achievementsCount)
                IsAchieved = false;
            base.Reset();
        }

        public override void OnAchievementEarned(IAchievement achievement, IEnumerable<IAchievement> achievements)
        {
            if (achievement == this)
                return;
            List<IAchievement> lst = achievements.ToList();
            int count = lst.Count(x => x.Id != Id && x.IsAchieved);
            if (1 + count == lst.Count()) // every achievement except ourself
            {
                ExtraData = _achievementsCount;
                Achieve(); // Achieve calls Reset, so we have to store ExtraData before calling Achieve (or IsAchieved will be reset to false)
            }
        }
    }
}
