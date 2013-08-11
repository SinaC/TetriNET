using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Net;
using System.Configuration;
using System.ServiceModel.Configuration;
using IPFiltering.Configuration;

namespace IPFiltering
{
    public class IPFilterServiceBehavior : IDispatchMessageInspector, IServiceBehavior
    {
        private static readonly object _httpAccessDenied = new object();
        private static readonly object _accessDenied = new object();
        private IPFilter _verifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="IPFilterServiceBehavior"/> class.
        /// </summary>
        /// <param name="filterName">Name of the filter.</param>
        public IPFilterServiceBehavior(string filterName)
        {
            _verifier = Configuration.FilterFactory.Create(IPFilterConfiguration.Default.Filters[filterName]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IPFilterServiceBehavior"/> class.
        /// </summary>
        /// <param name="filter">IP filter.</param>
        public IPFilterServiceBehavior(IPFilter filter)
        {
            _verifier = filter;
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
                if (_verifier.CheckAddress(address) == IPFilterType.Deny)
                {
                    request = null;
                    object result = (channel.LocalAddress.Uri.Scheme.Equals(Uri.UriSchemeHttp) ||
                                     channel.LocalAddress.Uri.Scheme.Equals(Uri.UriSchemeHttps)) ?
                        _httpAccessDenied : _accessDenied;
                    return result;
                }
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
            if (correlationState == _httpAccessDenied)
            {
                HttpResponseMessageProperty responseProperty = new HttpResponseMessageProperty();
                responseProperty.StatusCode = (HttpStatusCode)401;
                reply.Properties["httpResponse"] = responseProperty;                
            }
        }

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation.
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, System.Collections.ObjectModel.Collection<ServiceEndpoint> endpoints, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {

        }
        
        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, message or parameter interceptors, security extensions, and other custom extension objects.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                {
                    endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
                }
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

    public class IPFilterBehaviorExtension : BehaviorExtensionElement
    {
        /// <summary>
        /// Gets or sets the name of the filter.
        /// </summary>
        /// <value>The name of the filter.</value>
        [ConfigurationProperty("filterName", IsRequired = true)]
        public virtual string FilterName
        {
            get
            {
                return this["filterName"] as string;
            }
            set
            {

                this["providerName"] = value;
            }
        }
        /// <summary>
        /// Gets the type of behavior.
        /// </summary>
        /// <value></value>
        /// <returns>A <see cref="T:System.Type"/>.</returns>
        public override Type BehaviorType
        {
            get
            {
                return typeof(IPFilterServiceBehavior);
            }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// </summary>
        /// <returns>The behavior extension.</returns>
        protected override object CreateBehavior()
        {
            return new IPFilterServiceBehavior(this.FilterName);
        }
    }
}
