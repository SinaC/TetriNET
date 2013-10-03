using System;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.WCFHost
{
    internal class IPFilterServiceBehavior : IDispatchMessageInspector, IServiceBehavior
    {
        private static readonly object HttpAccessDenied = new object();
        private static readonly object AccessDenied = new object();
        private readonly IBanManager _verifier;
        private readonly IPlayerManager _playerManager;

        public IPFilterServiceBehavior(IBanManager verifier, IPlayerManager playerManager)
        {
            if (verifier == null)
                throw new ArgumentNullException("verifier");
            if (playerManager == null)
                throw new ArgumentNullException("playerManager");

            _verifier = verifier;
            _playerManager = playerManager;
        }

        /// <summary>
        /// Called after an inbound message has been received but before the message is dispatched to the intended operation.
        /// </summary>
        /// <param name="request">The request message.</param>
        /// <param name="channel">The incoming channel.</param>
        /// <param name="instanceContext">The current service instance.</param>
        /// <returns>
        /// The object used to correlate state. This object is passed back in the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.BeforeSendReply(System.ServiceModel.Channels.Message@,System.Object)"/> method.
        /// </returns>
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            // RemoteEndpointMessageProperty new in 3.5 allows us to get the remote endpoint address.
            RemoteEndpointMessageProperty remoteEndpoint = request.Properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            if (remoteEndpoint != null)
            {
                // The address is a string so we have to parse to get as a number
                IPAddress address = IPAddress.Parse(remoteEndpoint.Address);

                // If ip address is denied clear the request mesage so service method does not get execute
                if (_verifier.IsBanned(address))
                {
                    Log.WriteLine(Log.LogLevels.Warning, "Banned player {0} tried to connect", address);

                    request = null;
                    object result = (channel.LocalAddress.Uri.Scheme.Equals(Uri.UriSchemeHttp) ||
                                     channel.LocalAddress.Uri.Scheme.Equals(Uri.UriSchemeHttps)) ?
                        HttpAccessDenied : AccessDenied;
                    return result;
                }
                // TODO
                //else
                //{
                //    // Check SPAM
                //    ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
                //    IPlayer player = _playerManager[callback];
                //    if (player != null)
                //    {
                //        TimeSpan timeSpan = DateTime.Now - player.LastActionFromClient;
                //        //Log.WriteLine(Log.LogLevels.Debug, "DELAY BETWEEN LAST MSG AND NOW:{0} | {1}", timeSpan.TotalMilliseconds, player.LastActionFromClient);
                //    }
                //}
            }

            return null;
        }

        /// <summary>
        /// Called after the operation has returned but before the reply message is sent.
        /// </summary>
        /// <param name="reply">The reply message. This value is null if the operation is one way.</param>
        /// <param name="correlationState">The correlation object returned from the <see cref="M:System.ServiceModel.Dispatcher.IDispatchMessageInspector.AfterReceiveRequest(System.ServiceModel.Channels.Message@,System.ServiceModel.IClientChannel,System.ServiceModel.InstanceContext)"/> method.</param>
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            if (reply != null && correlationState == HttpAccessDenied)
            {
                HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty
                {
                    StatusCode = (HttpStatusCode)401
                };
                reply.Properties["httpResponse"] = responseProperty;
            }
            // TODO: else send something :)
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {

        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            //foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            //{
            //    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
            //    {
            //        endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
            //    }
            //}  
            foreach (EndpointDispatcher endpointDispatcher in serviceHostBase.ChannelDispatchers.Cast<ChannelDispatcher>().SelectMany(channelDispatcher => channelDispatcher.Endpoints))
            {
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
            }
        }
        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm that the service can run successfully.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {

        }

    }
}
