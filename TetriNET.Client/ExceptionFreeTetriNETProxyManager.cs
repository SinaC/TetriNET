using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Client
{
    public class ExceptionFreeTetriNETProxyManager : ITetriNETProxyManager
    {
        private readonly string _baseAddress;

        public ExceptionFreeTetriNETProxyManager(string baseAddress)
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
                Log.WriteLine("Connecting to server:" + address.Uri);
                Binding binding = new NetTcpBinding(SecurityMode.None);
                InstanceContext instanceContext = new InstanceContext(callback);
                ITetriNET proxy = DuplexChannelFactory<ITetriNET>.CreateChannel(instanceContext, binding, address);
                if (proxy != null)
                    return new ExceptionFreeTetriNETProxy(proxy, client);
            }
            return null;
        }
    }
}
