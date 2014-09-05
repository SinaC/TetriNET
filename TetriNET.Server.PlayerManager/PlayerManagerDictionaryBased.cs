using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.PlayerManager
{
    // Dictionary based
    public sealed class PlayerManagerDictionaryBased : IPlayerManager
    {
        private readonly object _lockObject;
        private readonly Dictionary<ITetriNETCallback, IPlayer> _players;
        private IPlayer _serverMaster;

        public PlayerManagerDictionaryBased(int maxPlayers)
        {
            if (maxPlayers <= 0)
                throw new ArgumentOutOfRangeException("maxPlayers", "maxPlayers must be strictly positive");
            _lockObject = new object();
            MaxPlayers = maxPlayers;
            _players = new Dictionary<ITetriNETCallback, IPlayer>();
            _serverMaster = null;
        }

        #region IPlayerManager

        public bool Add(IPlayer player)
        {
            if (player == null)
                return false;
            if (_players.Count >= MaxPlayers)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many players");
                return false;
            }

            if (_players.ContainsKey(player.Callback))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already registered", player.Name);
                return false;
            }

            if (_players.Any(x => x.Value.Name == player.Name || x.Value.Id == player.Id))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already registered", player.Name);
                return false;
            }

            //
            _players.Add(player.Callback, player);
            // ServerMaster
            if (_serverMaster == null || _serverMaster.Id > player.Id)
                _serverMaster = player;
            //
            return true;
        }

        public bool Remove(IPlayer player)
        {
            if (player == null)
                return false;
            bool removed = _players.Remove(player.Callback);
            // ServerMaster
            if (player == _serverMaster)
            {
                int min = _players.Select(x => (int?)x.Value.Id).Min() ?? -1;
                if (min != -1)
                {
                    KeyValuePair<ITetriNETCallback, IPlayer> kv = _players.FirstOrDefault(x => x.Value.Id == min);
                    if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IPlayer>)))
                        _serverMaster = null;
                    else
                        _serverMaster = kv.Value;
                }
                else
                    _serverMaster = null;
            }
            return removed;
        }

        public void Clear()
        {
            _serverMaster = null;
            _players.Clear();
        }

        public int MaxPlayers { get; private set; }

        public int PlayerCount
        {
            get { return _players.Count; }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public int FirstAvailableId
        {
            get
            {
                if (_players.Count == MaxPlayers)
                    return -1;

                IEnumerable<int> ids = _players.Select(x => x.Value.Id);
                int min = Enumerable
                    .Range(0, MaxPlayers)
                    .Except(ids)
                    .Min();
                return min;
            }
        }

        public List<IPlayer> Players
        {
            get { return _players.Select(x => x.Value).ToList(); }
        }

        public IPlayer ServerMaster
        {
            get { return _serverMaster; }
        }

        public IPlayer this[string name]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, IPlayer> kv = _players.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IPlayer>)))
                    return null;
                return kv.Value;
            }
        }

        public IPlayer this[int id]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, IPlayer> kv = _players.FirstOrDefault(x => x.Value.Id == id);
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IPlayer>)))
                    return null;
                return kv.Value;
            }
        }

        public IPlayer this[ITetriNETCallback callback]
        {
            get
            {
                IPlayer player;
                _players.TryGetValue(callback, out player);
                return player;
            }
        }

        #endregion
    }
}
