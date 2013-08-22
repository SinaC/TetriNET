using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Xml;

namespace TetriNET.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            Client client = new Client(callback => new WCFProxy(baseAddress, callback));
            client.Name = "joel-wpf-client";
            client._proxy.RegisterPlayer(client, client.Name);

            bool stopped = false;
            while (!stopped)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            }
        }
    }
}
