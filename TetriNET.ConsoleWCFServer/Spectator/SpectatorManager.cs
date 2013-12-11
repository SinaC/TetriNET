using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.ConsoleWCFServer.Spectator
{
    public class SpectatorManager : ISpectatorManager
    {
        private readonly object _lockObject;
        private readonly ISpectator[] _spectators;

        public SpectatorManager(int maxSpectators)
        {
            _lockObject = new object();
            MaxSpectators = maxSpectators;
            _spectators = new ISpectator[MaxSpectators];
        }

        #region ISpectatorManager

        public int Add(ISpectator spectator)
        {
            bool alreadyExists = _spectators.Any(x => x != null && (x == spectator || x.Name == spectator.Name));
            if (!alreadyExists)
            {
                // insert in first empty slot
                for (int i = 0; i < MaxSpectators; i++)
                    if (_spectators[i] == null)
                    {
                        _spectators[i] = spectator;
                        return i;
                    }
            }
            else
                Log.WriteLine(Log.LogLevels.Warning, "{0} already registered", spectator.Name);
            return -1;
        }

        public bool Remove(ISpectator spectator)
        {
            for (int i = 0; i < MaxSpectators; i++)
                if (_spectators[i] == spectator)
                {
                    _spectators[i] = null;
                    return true;
                }
            return false;
        }

        public void Clear()
        {
            for (int i = 0; i < MaxSpectators; i++)
                _spectators[i] = null;
        }

        public int MaxSpectators { get; private set; }

        public int SpectatorCount
        {
            get
            {
                return _spectators.Count(x => x != null);
            }
        }

        public object LockObject
        {
            get
            {
                return _lockObject;
            }
        }

        public IEnumerable<ISpectator> Spectators
        {
            get
            {
                return _spectators.Where(x => x != null);
            }
        }

        public int GetId(ISpectator spectator)
        {
            return spectator == null ? -1 : Array.IndexOf(_spectators, spectator);
        }

        public ISpectator this[string name]
        {
            get
            {
                return _spectators.FirstOrDefault(x => x != null && x.Name == name);
            }
        }

        public ISpectator this[int index]
        {
            get
            {
                if (index >= MaxSpectators)
                    return null;
                return _spectators[index];
            }
        }

        public ISpectator this[ITetriNETCallback callback]
        {
            get
            {
                return _spectators.FirstOrDefault(x => x != null && x.Callback == callback);
            }
        }

        #endregion
    }
}
