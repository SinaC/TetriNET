using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Client
{
    public class SimpleTetriNETProxyManager : ITetriNETProxyManager
    {
        private readonly string _baseAddress;

        public SimpleTetriNETProxyManager(string baseAddress)
        {
            _baseAddress = baseAddress;
        }

        public ITetriNET CreateProxy(ITetriNETCallback callback, IClient client)
        {
            EndpointAddress address = null;
            if (String.IsNullOrEmpty(_baseAddress) || _baseAddress.ToLower() == "auto")
            {
                Log.WriteLine("Searching ITetriNET server");
                List<EndpointAddress> addresses = DiscoveryHelper.DiscoverAddresses<ITetriNET>();
                if (addresses != null && addresses.Any())
                {
                    foreach (EndpointAddress endpoint in addresses)
                        Log.WriteLine("{0}:\t{1}", addresses.IndexOf(endpoint), endpoint.Uri);
                    Log.WriteLine("Connecting to first server");
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
                Binding binding = new NetTcpBinding(SecurityMode.None);
                InstanceContext instanceContext = new InstanceContext(callback);
                return DuplexChannelFactory<ITetriNET>.CreateChannel(instanceContext, binding, address);
            }
            return null;
        }
    }
}
