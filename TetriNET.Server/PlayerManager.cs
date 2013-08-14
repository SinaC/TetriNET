using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class PlayerManager : IPlayerManager
    {
        private readonly IPlayer[] _players;

        public int MaxPlayers { get; private set; }

        public PlayerManager(int maxPlayers)
        {
            MaxPlayers = maxPlayers;
            _players = new IPlayer[MaxPlayers];
        }

        #region IPlayerManager

        public int Add(IPlayer player)
        {
            bool alreadyExists = _players.Any(x => x != null && (x == player || x.Name == player.Name));
            if (!alreadyExists)
            {
                // insert in first empty slot
                for (int i = 0; i < MaxPlayers; i++)
                    if (_players[i] == null)
                    {
                        _players[i] = player;
                        return i;
                    }
            }
            return -1;
        }

        public bool Remove(IPlayer player)
        {
            for (int i = 0; i < MaxPlayers; i++)
                if (_players[i] == player)
                {
                    _players[i] = null;
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

        public int GetId(IPlayer player)
        {
            return Array.IndexOf(_players, player);
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
                if (index >= MaxPlayers)
                    return null;
                return _players[index];
            }
        }

        public IPlayer this[ITetriNETCallback callback]
        {
            get
            {
                return _players.FirstOrDefault(x => x != null && x.Callback == callback);
            }
        }

        #endregion
    }
}
