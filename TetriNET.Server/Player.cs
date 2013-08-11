using TetriNET.Common;

namespace TetriNET.Server
{
    public class Player : IPlayer
    {
        public Player(int id, string name, ITetriNETCallback callback)
        {
            Id = id;
            Name = name;
            Callback = callback;
            TetriminoIndex = 0;
        }

        #region IPlayer
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ITetriNETCallback Callback { get; private set; }
        public int TetriminoIndex { get; set; }
        #endregion

    }
}
