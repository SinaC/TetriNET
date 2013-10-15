using System.Collections.Generic;

namespace TetriNET.Client.Interfaces
{
    public interface IAchievementManager
    {
        List<IAchievement> Achievements { get; }
        IClient Client { get; set; }

        event OnAchievedHandler OnAchieved;

        void Reset();
    }
}
