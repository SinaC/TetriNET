using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class ExceptionFreeTetriNETCallbackManager : ITetriNETCallbackManager
    {
        private readonly ConcurrentDictionary<ITetriNETCallback, ExceptionFreeTetriNETCallback> _callbacks = new ConcurrentDictionary<ITetriNETCallback, ExceptionFreeTetriNETCallback>();
        private readonly IPlayerManager _playerManager;

        public ExceptionFreeTetriNETCallbackManager(IPlayerManager playerManager)
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
                ExceptionFreeTetriNETCallback exceptionFreeCallback;
                bool found = _callbacks.TryGetValue(callback, out exceptionFreeCallback);
                if (!found)
                {
                    exceptionFreeCallback = new ExceptionFreeTetriNETCallback(callback, _playerManager);
                    _callbacks.TryAdd(callback, exceptionFreeCallback);
                }
                return exceptionFreeCallback;
            }
        }

        private void OnPlayerRemoved(object sender, ITetriNETCallback callback)
        {
            // callback is in fact of type ExceptionFreeTetriNETCallback
            if (callback is ExceptionFreeTetriNETCallback)
            {
                // Black magic: 
                //  param callback must be casted to ExceptionFreeTetriNETCallback then get real transport callback
                ITetriNETCallback transportCallback = (callback as ExceptionFreeTetriNETCallback).Callback;
                ExceptionFreeTetriNETCallback tryRemoveResult;
                _callbacks.TryRemove(transportCallback, out tryRemoveResult);
                Debug.Assert(tryRemoveResult == callback);
            }
        }
    }
}