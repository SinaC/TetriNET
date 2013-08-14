using System;

namespace TetriNET.Server
{
    

    class Program
    {
        static void Main(string[] args)
        {
            //GameServer server = new GameServer(new BasicCallbackManager());
            PlayerManager playerManager = new PlayerManager();
            GameServer server = new GameServer(new ExceptionFreeCallbackManager(playerManager), playerManager);
            server.StartService();

            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("m: Send message broadcast");

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
                    }
                }
                else
                    System.Threading.Thread.Sleep(1000);
            }

            server.StopService();
        }
    }
}
