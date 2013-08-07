using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using TetriNET.Common.Interfaces;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Single)]
    internal class GameServer : ITetriNET
    {
        public GameServer() { 
        }

        public void StartService() {
            Uri baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;

            ServiceHost host = new ServiceHost(this, baseAddress);
            host.AddDefaultEndpoints();
            host.Open();
        }

        public void RegisterPlayer(string playerName)
        {
            Console.WriteLine("Registering player:" + playerName);
        }
    }
}
