using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.SpectatorManager
{
    // Dictionary based
    public sealed class SpectatorManagerDictionaryBased : ISpectatorManager
    {
        private readonly object _lockObject;
        private readonly Dictionary<ITetriNETCallback, ISpectator> _spectators;

        public SpectatorManagerDictionaryBased(int maxSpectators)
        {
            _lockObject = new object();
            MaxSpectators = maxSpectators;
            _spectators = new Dictionary<ITetriNETCallback, ISpectator>();
        }

        public bool Add(ISpectator spectator)
        {
            if (_spectators.Count >= MaxSpectators)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Too many spectators");
                return false;
            }

            if (_spectators.ContainsKey(spectator.Callback))
            {
                Log.Default.WriteLine(LogLevels.Warning, "{0} already registered", spectator.Name);
                return false;
            }
            //
            _spectators.Add(spectator.Callback, spectator);
            //
            return true;
        }

        public bool Remove(ISpectator spectator)
        {
            return _spectators.Remove(spectator.Callback);
        }

        public void Clear()
        {
            _spectators.Clear();
        }

        public int MaxSpectators { get; private set; }

        public int SpectatorCount
        {
            get { return _spectators.Count; }
        }

        public object LockObject
        {
            get { return _lockObject; }
        }

        public int FirstAvailableId
        {
            get
            {
                if (_spectators.Count == MaxSpectators)
                    return -1;

                IEnumerable<int> ids = _spectators.Select(x => x.Value.Id);
                int min = Enumerable
                    .Range(0, MaxSpectators)
                    .Except(ids)
                    .Min();
                return min;
            }
        }

        public IReadOnlyCollection<ISpectator> Spectators
        {
            get { return _spectators.Select(x => x.Value).ToList(); }
        }

        public ISpectator this[string name]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, ISpectator> kv = _spectators.FirstOrDefault(x => x.Value.Name == name);
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, ISpectator>)))
                    return null;
                return kv.Value;
            }
        }

        public ISpectator this[int id]
        {
            get
            {
                KeyValuePair<ITetriNETCallback, ISpectator> kv = _spectators.FirstOrDefault(x => x.Value.Id == id);
                if (kv.Equals(default(KeyValuePair<ITetriNETCallback, ISpectator>)))
                    return null;
                return kv.Value;
            }
        }

        public ISpectator this[ITetriNETCallback callback]
        {
            get
            {
                ISpectator spectator;
                _spectators.TryGetValue(callback, out spectator);
                return spectator;
            }
        }
    }
}
