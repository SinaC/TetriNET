using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using TetriNET.Server.Host;
using TetriNET.Server.Player;

namespace TetriNET.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            PlayerManager playerManager = new PlayerManager(6);

            //
            WCFHost wcfHost = new WCFHost(playerManager, (playerName, callback) => new Player.Player(playerName, callback))
            {
                Port = ConfigurationManager.AppSettings["port"]
            };

            //
            BuiltInHost builtInHost = new BuiltInHost(playerManager, (playerName, callback) => new Player.Player(playerName, callback));

            //
            //SocketHost socketHost = new SocketHost(playerManager, (playerName, callback) => new Player(playerName, callback))
            //{
            //    Port = 5656
            //};

            //
            List<DummyBuiltInClient> clients = new List<DummyBuiltInClient>
            {
                //new DummyBuiltInClient("BuiltIn-Joel" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost),
                //new DummyBuiltInClient("BuiltIn-Celine" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost)
            };

            //
            Server server = new Server(playerManager, wcfHost, builtInHost);
            //Server server = new Server(playerManager, socketHost);

            //
            server.StartServer();

            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("a: add dummy player");
            Console.WriteLine("r: remove dummy player");
            Console.WriteLine("l: dummy player lose");
            Console.WriteLine("d: dump player list");

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
                        case ConsoleKey.A:
                            clients.Add(new DummyBuiltInClient("BuiltIn-" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost));
                            break;
                        case ConsoleKey.R:
                        {
                            DummyBuiltInClient client = clients.LastOrDefault();
                            if (client != null)
                            {
                                client.DisconnectFromServer();
                                clients.Remove(client);
                            }
                            break;
                        }
                        case ConsoleKey.L:
                        {
                            DummyBuiltInClient client = clients.LastOrDefault();
                            if (client != null)
                                client.Lose();
                            break;
                        }
                        case ConsoleKey.D:
                            foreach (IPlayer p in playerManager.Players)
                                Console.WriteLine("{0}) {1} {2} {3} {4:HH:mm:ss.fff}", playerManager.GetId(p), p.Name, p.State, p.TetriminoIndex, p.LastAction);
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                    Parallel.ForEach(
                        clients, 
                        client => client.Test());
                }
            }
        }
    }
}
