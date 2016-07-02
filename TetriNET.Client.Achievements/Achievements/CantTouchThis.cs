using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class CantTouchThis : AchievementBase
    {
        private readonly int _achievementsCount;

        public CantTouchThis()
        {
            Id = 5;
            Points = 100;
            Title = "Can't Touch This";
            Description = "Earn all achievements";
            OnlyOnce = true;
            BronzeLevel = 1;
            SilverLevel = 2;
            GoldLevel = 3;

            _achievementsCount = Assembly.GetExecutingAssembly().GetTypes().Count(t => t.IsSubclassOf(typeof(AchievementBase)) && !t.IsAbstract);
        }

        public override void Reset()
        {
            if (IsAchieved && ExtraData < _achievementsCount)
                IsAchieved = false;
            base.Reset();
        }

        public override void OnAchievementEarned(IAchievement achievement, IReadOnlyCollection<IAchievement> achievements)
        {
            if (achievement == this)
                return;
            int count = achievements.Count(x => x.Id != Id && x.IsAchieved);
            if (1 + count == achievements.Count()) // every achievement except ourself
            {
                ExtraData = _achievementsCount;
                Achieve(); // Achieve calls Reset, so we have to store ExtraData before calling Achieve (or IsAchieved will be reset to false)
            }
        }
    }
}
