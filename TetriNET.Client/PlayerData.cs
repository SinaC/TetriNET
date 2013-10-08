using TetriNET.Client.Interfaces;

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

        #region IOpponent

        public int PlayerId { get; set; }
        public bool IsImmune { get; set; }
        public IBoard Board { get; set; }
        public string Team { get; set; }

        #endregion

        public string Name { get; set; }
        public States State { get; set; }
    }
}
