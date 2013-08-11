using System;
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
            LastAction = DateTime.Now;
        }

        #region IPlayer
        public int Id { get; private set; }
        public string Name { get; private set; }
        public ITetriNETCallback Callback { get; private set; }
        public int TetriminoIndex { get; set; }
        public DateTime LastAction { get; set; }
        #endregion

    }
}
