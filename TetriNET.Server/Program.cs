using System;

namespace TetriNET.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();
            server.StartService();

            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");

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
                    }
                }
                else
                    System.Threading.Thread.Sleep(1000);
            }

            server.StopService();
        }
    }
}
