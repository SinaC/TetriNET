using TetriNET.Common.Interfaces;

namespace TetriNET.Client
{
    internal sealed class PlayerData : IOpponent
    {
        public enum States
        {
            Joined,
            Playing,
            Lost,
        }

        public int PlayerId { get; set; }
        public string Name { get; set; }
        public IBoard Board { get; set; }
        public States State { get; set; }
    }
}
