using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
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
        }

        #region ICallbackManager
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
                    exceptionFreeCallback.OnPlayerDisconnected += OnPlayerDisconnected;
                }
                return exceptionFreeCallback;
            }
        }

        public string Endpoint
        {
            get
            {
                RemoteEndpointMessageProperty clientEndpoint = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                return clientEndpoint == null ? "???" : clientEndpoint.Address;
            }
        }
        #endregion

        private void OnPlayerDisconnected(object sender, IPlayer player)
        {
            // TODO: inform server
            /*
 *                 {
            Log.WriteLine(actionName + ": " + player.Name + " has disconnected");
            _playerManager.Remove(player);
            // Caution: recursive call
            foreach (Player p in _playerManager.Players)
                p.Callback.OnPublishServerMessage(player.Name + " has disconnected");
        }
*/
            ITetriNETCallback callback = player.Callback;
            if (callback is ExceptionFreeCallback)
            {
                ExceptionFreeCallback tryRemoveResult;
                _callbacks.TryRemove(callback, out tryRemoveResult);
            }
            _playerManager.Remove(player);
        }
    }
}