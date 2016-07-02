using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.Contracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.SpectatorManager
{
    public class SpectatorManagerArrayBased : ISpectatorManager
    {
        private readonly ISpectator[] _spectators;

        public SpectatorManagerArrayBased(int maxSpectators)
        {
            LockObject = new object();
            MaxSpectators = maxSpectators;
            _spectators = new ISpectator[MaxSpectators];
        }

        #region ISpectatorManager

        public bool Add(ISpectator spectator)
        {
            bool alreadyExists = _spectators.Any(x => x != null && (x == spectator || x.Name == spectator.Name));
            if (!alreadyExists)
            {
                // insert in first empty slot
                for (int i = 0; i < MaxSpectators; i++)
                    if (_spectators[i] == null)
                    {
                        _spectators[i] = spectator;
                        return true;
                    }
            }
            else
                Log.Default.WriteLine(LogLevels.Warning, "{0} already registered", spectator.Name);
            return false;
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

        public int MaxSpectators { get; }

        public int SpectatorCount
        {
            get { return _spectators.Count(x => x != null); }
        }

        public object LockObject { get; }

        public int FirstAvailableId
        {
            get
            {
                for (int i = 0; i < MaxSpectators; i++)
                    if (_spectators[i] == null)
                        return i;
                return -1;
            }
        }

        public IReadOnlyCollection<ISpectator> Spectators
        {
            get { return _spectators.Where(x => x != null).ToList(); }
        }

        public ISpectator this[string name]
        {
            get { return _spectators.FirstOrDefault(x => x != null && x.Name == name); }
        }

        public ISpectator this[int id]
        {
            get { return _spectators.FirstOrDefault(x => x != null && x.Id == id); }
        }

        public ISpectator this[ITetriNETCallback callback]
        {
            get { return _spectators.FirstOrDefault(x => x != null && x.Callback == callback); }
        }

        #endregion
    }
}
