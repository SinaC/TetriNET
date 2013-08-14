using System;
using System.Configuration;
using System.Linq;

namespace TetriNET.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            //
            PlayerManager playerManager = new PlayerManager(6);

            //
            WCFHost wcfHost = new WCFHost(playerManager, (playerName, callback) => new RemotePlayer(playerName, callback))
            {
                Port = ConfigurationManager.AppSettings["port"]
            };

            //
            BuiltInHost builtInHost = new BuiltInHost(playerManager, playerName => new BuiltInPlayer(playerName, new DummyTetriNETCallback()) );

            //
            Server server = new Server(playerManager, wcfHost, builtInHost);

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
                            builtInHost.RegisterPlayer("BuiltIn-Joel");
                            break;
                        case ConsoleKey.D:
                            builtInHost.UnregisterPlayer();
                            break;
                    }
                }
                else
                    System.Threading.Thread.Sleep(1000);
                if (playerManager.Players.Any(x => x is BuiltInPlayer))
                {
                    IPlayer player = playerManager.Players.First(x => x is BuiltInPlayer);
                    //player.
                    // TODO: send a message from player to Server
                }
            }
        }
    }
}
