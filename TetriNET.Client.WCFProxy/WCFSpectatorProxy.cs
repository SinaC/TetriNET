using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;

namespace TetriNET.Client.WCFProxy
{
    public class WCFSpectatorProxy : ISpectatorProxy
    {
        private DuplexChannelFactory<IWCFTetriNETSpectator> _factory;
        private readonly IWCFTetriNETSpectator _proxy;

        public WCFSpectatorProxy(ITetriNETCallback callback, string address)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (address == null)
                throw new ArgumentNullException("address");

            LastActionToServer = DateTime.Now;

            EndpointAddress endpointAddress = new EndpointAddress(address);

            // Create WCF proxy from endpoint
            Log.WriteLine(Log.LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callback);
            _factory = new DuplexChannelFactory<IWCFTetriNETSpectator>(instanceContext, binding, endpointAddress);
            _proxy = _factory.CreateChannel(instanceContext);
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                LastActionToServer = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                if (OnConnectionLost != null)
                    OnConnectionLost();
                if (_factory != null)
                    _factory.Abort();
            }
        }

        #region ISpectatorProxy

        public DateTime LastActionToServer { get; private set; }

        public event ProxySpectatorConnectionLostHandler OnConnectionLost;

        public bool Disconnect()
        {
            if (_factory == null)
                return false; // should connect first
            try
            {
                _factory.Close();
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Warning, "Exception:{0}", ex);
                _factory.Abort();
            }
            _factory = null;
            return true;
        }

        #endregion

        #region ITetriNETSpectator

        public void RegisterSpectator(ITetriNETCallback callback, string spectatorName)
        {
            ExceptionFreeAction(() => _proxy.RegisterSpectator(spectatorName), "RegisterSpectator");
        }

        public void UnregisterSpectator(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.UnregisterSpectator, "UnregisterSpectator");
        }

        public void HeartbeatSpectator(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.HeartbeatSpectator, "Heartbeat");
        }

        public void PublishSpectatorMessage(ITetriNETCallback callback, string msg)
        {
            ExceptionFreeAction(() => _proxy.PublishSpectatorMessage(msg), "PublishSpectatorMessage");
        }

        #endregion
    }
}
