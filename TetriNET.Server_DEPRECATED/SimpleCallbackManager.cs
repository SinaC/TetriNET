using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class SimpleCallbackManager : ICallbackManager
    {
        #region ICallbackManager
        public ITetriNETCallback Callback
        {
            get { return OperationContext.Current.GetCallbackChannel<ITetriNETCallback>(); }
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
    }
}