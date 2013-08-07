using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common.WCF;
using TetriNET.Common.Interfaces;

namespace TetriNET.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            EndpointAddress address = DiscoveryHelper.DiscoverAddress<ITetriNET>();
            Binding binding = new NetTcpBinding();

            ITetriNET proxy = ChannelFactory<ITetriNET>.CreateChannel(binding, address);
            proxy.RegisterPlayer("Joël");

            (proxy as ICommunicationObject).Close();

            //IMyContract proxy = DiscoveryFactory.CreateChannel<IMyContract>();
            //proxy.MyMethod();

            //(proxy as ICommunicationObject).Close();
        }
    }
}
