using System;

namespace TetriNET.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            GameClient client = new GameClient();
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
