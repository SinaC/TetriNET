using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.PlayerManager
{
    // Array based
    public sealed class PlayerManagerArrayBased : IPlayerManager
    {
        private readonly object _lockObject;
        private readonly IPlayer[] _players;

        public PlayerManagerArrayBased(int maxPlayers)
        {
            if (maxPlayers <= 0)
                throw new ArgumentOutOfRangeException("maxPlayers", "maxPlayers must be strictly positive");
            _lockObject = new object();
            MaxPlayers = maxPlayers;
            _players = new IPlayer[MaxPlayers];
        }

        #region IPlayerManager

        public bool Add(IPlayer player)
        {
            if (player == null)
                return false;
            bool alreadyExists = _players.Any(x => x != null && (x == player || x.Name == player.Name || x.Id == player.Id || x.Callback == player.Callback));
            if (!alreadyExists)
            {
                // insert in first empty slot
                for (int i = 0; i < MaxPlayers; i++)
                    if (_players[i] == null)
                    {
                        _players[i] = player;
                        return true;
                    }
                Log.WriteLine(Log.LogLevels.Warning, "No empty slot");
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "{0} already registered", player.Name);
            return false;
        }

        public bool Remove(IPlayer player)
        {
            if (player == null)
                return false;
            for (int i = 0; i < MaxPlayers; i++)
                if (_players[i] == player)
                {
                    _players[i] = null;
                    return true;
                }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < MaxPlayers; i++)
                _players[i] = null;
        }

        public int MaxPlayers { get; private set; }

        public int PlayerCount
        {
            get { return _players.Count(x => x != null); }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public int FirstAvailableId
        {
            get
            {
                for (int i = 0; i < MaxPlayers; i++)
                    if (_players[i] == null)
                        return i;
                return -1;
            }
        }

        public List<IPlayer> Players
        {
            get { return _players.Where(x => x != null).ToList(); }
        }

        public IPlayer ServerMaster
        {
            get { return _players.FirstOrDefault(x => x != null); }
        }

        public IPlayer this[string name]
        {
            get { return _players.FirstOrDefault(x => x != null && x.Name == name); }
        }

        public IPlayer this[int id]
        {
            get { return _players.FirstOrDefault(x => x != null && x.Id == id); }
        }

        public IPlayer this[ITetriNETCallback callback]
        {
            get { return _players.FirstOrDefault(x => x != null && x.Callback == callback); }
        }

        #endregion
    }
}
