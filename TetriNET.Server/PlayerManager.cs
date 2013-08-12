using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class PlayerManager : IPlayerManager
    {
        private const int MaxPlayerCount = 6;

        private readonly Player[] _players = new Player[MaxPlayerCount];

        #region IPlayerManager

        public event EventHandler<ITetriNETCallback> OnPlayerRemoved;

        public IPlayer Add(string name, ITetriNETCallback callback)
        {
            int emptySlot = GetEmptySlot(name, callback);
            if (emptySlot >= 0)
            {
                Player player = new Player(emptySlot, name, callback);
                _players[emptySlot] = player;
                return player;
            }
            return null;
        }

        public bool Remove(IPlayer player)
        {
            for (int i = 0; i < MaxPlayerCount; i++)
                if (_players[i] != null)
                {
                    ITetriNETCallback callback = _players[i].Callback;
                    _players[i] = null;
                    if (OnPlayerRemoved != null)
                        OnPlayerRemoved(this, callback);
                    return true;
                }
            return false;
        }

        public int PlayerCount
        {
            get
            {
                return _players.Count(x => x != null);
            }
        }

        public IEnumerable<IPlayer> Players
        {
            get
            {
                return _players.Where(x => x != null);
            }
        }

        public IPlayer this[ITetriNETCallback callback]
        {
            get
            {
                return _players.FirstOrDefault(x => x != null && x.Callback == callback);
            }
        }

        public IPlayer this[string name]
        {
            get
            {
                return _players.FirstOrDefault(x => x != null && x.Name == name);
            }
        }

        public IPlayer this[int index]
        {
            get
            {
                if (index >= MaxPlayerCount)
                    return null;
                return _players[index];
            }
        }
        #endregion

        private int GetEmptySlot(string playerName, ITetriNETCallback callback)
        {
            // player already registered
            if (_players.Any(x => x != null && (x.Name == playerName || x.Callback == callback)))
                return -1;
            // get first empty slot
            int emptySlot = -1;
            for (int i = 0; i < MaxPlayerCount; i++)
                if (_players[i] == null)
                {
                    emptySlot = i;
                    break;
                }
            return emptySlot;
        }
    }
}
