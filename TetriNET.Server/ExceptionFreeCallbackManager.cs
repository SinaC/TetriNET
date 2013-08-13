using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class ExceptionFreeCallbackManager : ICallbackManager
    {
        private readonly ConcurrentDictionary<ITetriNETCallback, ExceptionFreeCallback> _callbacks = new ConcurrentDictionary<ITetriNETCallback, ExceptionFreeCallback>();
        private readonly IPlayerManager _playerManager;

        public ExceptionFreeCallbackManager(IPlayerManager playerManager)
        {
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");
            _playerManager = playerManager;
            _playerManager.OnPlayerRemoved += OnPlayerRemoved;
        }

        public ITetriNETCallback Callback
        {
            get
            {
                ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                ExceptionFreeCallback exceptionFreeCallback;
                bool found = _callbacks.TryGetValue(callback, out exceptionFreeCallback);
                if (!found)
                {
                    exceptionFreeCallback = new ExceptionFreeCallback(callback, _playerManager);
                    _callbacks.TryAdd(callback, exceptionFreeCallback);
                }
                return exceptionFreeCallback;
            }
        }

        private void OnPlayerRemoved(object sender, ITetriNETCallback callback)
        {
            // callback is in fact of type ExceptionFreeTetriNETCallback
            if (callback is ExceptionFreeCallback)
            {
                // Black magic: 
                //  param callback must be casted to ExceptionFreeTetriNETCallback then get real transport callback
                ITetriNETCallback transportCallback = (callback as ExceptionFreeCallback).Callback;
                ExceptionFreeCallback tryRemoveResult;
                _callbacks.TryRemove(transportCallback, out tryRemoveResult);
                Debug.Assert(tryRemoveResult == callback);
            }
        }
    }
}