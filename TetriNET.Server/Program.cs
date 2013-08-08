using System;

namespace TetriNET.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();
            server.StartService();
            server.StartGame();

            Console.WriteLine("Press enter to stop server");
            
            Console.ReadLine();

            server.StopGame();
            server.StopService();
        }
    }
}
