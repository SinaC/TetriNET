using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace TetriNET.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            PlayerManager playerManager = new PlayerManager(6);

            //
            WCFHost wcfHost = new WCFHost(playerManager, (playerName, callback) => new Player(playerName, callback))
            {
                Port = ConfigurationManager.AppSettings["port"]
            };

            //
            BuiltInHost builtInHost = new BuiltInHost(playerManager, (playerName, callback) => new Player(playerName, callback));

            //
            SocketHost socketHost = new SocketHost(playerManager, (playerName, callback) => new Player(playerName, callback))
            {
                Port = 5656
            };

            //
            List<DummyBuiltInClient> clients = new List<DummyBuiltInClient>
            {
                new DummyBuiltInClient("BuiltIn-Joel" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost),
                new DummyBuiltInClient("BuiltIn-Celine" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost)
            };

            //
            //Server server = new Server(playerManager, wcfHost, builtInHost);
            Server server = new Server(playerManager, socketHost);

            //
            server.StartServer();

            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("m: Send message broadcast");
            Console.WriteLine("c: Connect local player");
            Console.WriteLine("d: Disconnect local player");

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
                        case ConsoleKey.S:
                            server.StartGame();
                            break;
                        case ConsoleKey.T:
                            server.StopGame();
                            break;
                        case ConsoleKey.M:
                            server.BroadcastRandomMessage();
                            break;
                        case ConsoleKey.C:
                            // TODO
                            break;
                        case ConsoleKey.D:
                            // TODO
                            //dummyBuiltInClient.DisconnectFromServer();
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                    //dummyBuiltInClient.Test();
                    Parallel.ForEach(
                        clients, 
                        client => client.Test());
                }
            }
        }
    }
}
