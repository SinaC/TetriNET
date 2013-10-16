using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Eliminator : Achievement
    {
        public Eliminator()
        {
            Title = "Eliminator";
            Description = "Clear a total of 10,000 lines";
            OnlyOnce = true;
        }

        public override void OnRoundFinished(int lineCompleted, int level, IBoard board)
        {
            ExtraData += lineCompleted;
            if (ExtraData >= 10000)
                Achieve();
        }
    }
}
