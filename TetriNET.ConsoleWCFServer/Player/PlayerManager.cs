using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.ConsoleWCFServer.Player
{
    public sealed class PlayerManager : IPlayerManager
    {
        private readonly object _lockObject;
        private readonly IPlayer[] _players;

        public PlayerManager(int maxPlayers)
        {
            _lockObject = new object();
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
            else
                Log.WriteLine(Log.LogLevels.Warning, "{0} already registered", player.Name);
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

        public void Clear()
        {
            for (int i = 0; i < MaxPlayers; i++)
                _players[i] = null;
        }

        public int MaxPlayers { get; private set; }

        public int PlayerCount
        {
            get
            {
                return _players.Count(x => x != null);
            }
        }

        public object LockObject
        {
            get
            {
                return _lockObject;
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
            return player == null ? -1 : Array.IndexOf(_players, player);
        }

        public IPlayer ServerMaster
        {
            get
            {
                return _players.FirstOrDefault(x => x != null);
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
