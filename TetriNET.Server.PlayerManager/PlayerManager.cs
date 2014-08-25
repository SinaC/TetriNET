using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.PlayerManager
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

        public bool Add(IPlayer player)
        {
            bool alreadyExists = _players.Any(x => x != null && (x == player || x.Name == player.Name));
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

    // Dictionary based
    //public sealed class PlayerManager : IPlayerManager
    //{
    //    private readonly object _lockObject;
    //    private readonly Dictionary<ITetriNETCallback, IPlayer> _players;
    //    private IPlayer _serverMaster;

    //    public PlayerManager(int maxPlayers)
    //    {
    //        _lockObject = new object();
    //        MaxPlayers = maxPlayers;
    //        _players = new Dictionary<ITetriNETCallback, IPlayer>();
    //        _serverMaster = null;
    //    }

    //    public bool Add(IPlayer player)
    //    {
    //        bool alreadyExists = _players.ContainsKey(player.Callback);

    //        if (!alreadyExists)
    //        {
    //            _players.Add(player.Callback, player);
    //            // ServerMaster
    //            if (_serverMaster == null || _serverMaster.Id > player.Id)
    //                _serverMaster = player;
    //            //
    //            return true;
    //        }
    //        Log.WriteLine(Log.LogLevels.Warning, "{0} already registered", player.Name);
    //        return false;
    //    }

    //    public bool Remove(IPlayer player)
    //    {
    //        bool removed = _players.Remove(player.Callback);
    //        // ServerMaster
    //        if (player == _serverMaster)
    //        {
    //            int min = _players.Select(x => (int?)x.Value.Id).Min() ?? -1;
    //            if (min != -1)
    //            {
    //                KeyValuePair<ITetriNETCallback, IPlayer> kv = _players.FirstOrDefault(x => x.Value.Id == min);
    //                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IPlayer>)))
    //                    _serverMaster = null;
    //                else
    //                    _serverMaster = kv.Value;
    //            }
    //        }
    //        return removed;
    //    }

    //    public void Clear()
    //    {
    //        _players.Clear();
    //    }

    //    public int MaxPlayers { get; private set; }

    //    public int PlayerCount
    //    {
    //        get { return _players.Count; }
    //    }

    //    public object LockObject
    //    {
    //        get { return _lockObject; }
    //    }

    //    public int FirstAvailableId
    //    {
    //        get
    //        {
    //            if (_players.Count == MaxPlayers)
    //                return -1;

    //            IEnumerable<int> ids = _players.Select(x => x.Value.Id);
    //            int min = Enumerable
    //                .Range(0, MaxPlayers)
    //                .Except(ids)
    //                .Min();
    //            return min;
    //        }
    //    }

    //    public List<IPlayer> Players
    //    {
    //        get { return _players.Select(x => x.Value).ToList(); }
    //    }

    //    public IPlayer ServerMaster
    //    {
    //        get { return _serverMaster; }
    //    }

    //    public IPlayer this[string name]
    //    {
    //        get
    //        {
    //            KeyValuePair<ITetriNETCallback, IPlayer> kv = _players.FirstOrDefault(x => x.Value.Name == name);
    //            if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IPlayer>)))
    //                return null;
    //            return kv.Value;
    //        }
    //    }

    //    public IPlayer this[int id]
    //    {
    //        get
    //        {
    //            KeyValuePair<ITetriNETCallback, IPlayer> kv = _players.FirstOrDefault(x => x.Value.Id == id);
    //            if (kv.Equals(default(KeyValuePair<ITetriNETCallback, IPlayer>)))
    //                return null;
    //            return kv.Value;
    //        }
    //    }

    //    public IPlayer this[ITetriNETCallback callback]
    //    {
    //        get
    //        {
    //            IPlayer player;
    //            _players.TryGetValue(callback, out player);
    //            return player;
    //        }
    //    }
    //}
}
