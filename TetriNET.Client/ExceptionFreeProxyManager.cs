using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Common.WCF;

namespace TetriNET.Client
{
    public class ExceptionFreeProxyManager : IProxyManager
    {
        private readonly string _baseAddress;

        public ExceptionFreeProxyManager(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public IWCFTetriNET CreateProxy(ITetriNETCallback callback, IClient client)
        {
            EndpointAddress address = null;
            if (String.IsNullOrEmpty(_baseAddress) || _baseAddress.ToLower() == "auto")
            {
                Log.WriteLine("Searching IWCFTetriNET server");
                List<EndpointAddress> addresses = DiscoveryHelper.DiscoverAddresses<IWCFTetriNET>();
                if (addresses != null && addresses.Any())
                {
                    foreach (EndpointAddress endpoint in addresses)
                        Log.WriteLine("{0}:\t{1}", addresses.IndexOf(endpoint), endpoint.Uri);
                    Log.WriteLine("Selecting first server");
                    address = addresses[0];
                }
                else
                {
                    Log.WriteLine("No server found");
                }
            }
            else
                address = new EndpointAddress(_baseAddress);

            if (address != null)
            {
                Log.WriteLine("Connecting to server:{0}", address.Uri);
                Binding binding = new NetTcpBinding(SecurityMode.None);
                InstanceContext instanceContext = new InstanceContext(callback);
                IWCFTetriNET proxy = DuplexChannelFactory<IWCFTetriNET>.CreateChannel(instanceContext, binding, address);
                if (proxy != null)
                    return new ExceptionFreeProxy(proxy, client);
            }
            return null;
        }
    }
}
