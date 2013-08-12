using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common;
using TetriNET.Common.WCF;

namespace TetriNET.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            //string baseAddress = "net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            //SimpleTetriNETProxyManager proxyManager = new SimpleTetriNETProxyManager(baseAddress);
            ExceptionFreeTetriNETProxyManager proxyManager = new ExceptionFreeTetriNETProxyManager(baseAddress);

            GameClient client = new GameClient(proxyManager);
            client.PlayerName = "Joel_" + Guid.NewGuid().ToString().Substring(0,6);

            Console.WriteLine("Press any key to stop client");

            while (true)
            {
                client.Test();
                if (Console.KeyAvailable)
                    break;
                System.Threading.Thread.Sleep(250);
            }

            Console.ReadLine();
        }
    }
}
