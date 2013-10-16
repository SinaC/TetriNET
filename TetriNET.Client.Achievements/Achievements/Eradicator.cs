using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Eradicator: Achievement
    {
        public Eradicator()
        {
            Title = "Eliminator";
            Description = "Clear a total of 100,000 lines";
            OnlyOnce = true;
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            ExtraData += lineCompleted;
            if (ExtraData >= 100000)
                Achieve();
        }
    }
}
