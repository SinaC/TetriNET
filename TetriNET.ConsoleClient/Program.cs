using System;
using System.Configuration;
using TetriNET.Client.Blocks;

namespace TetriNET.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            const int width = 5;
            const int height = 5;
            byte[] grid = new byte[width * height];
            //grid[1 + 2 * width] = 9;

            BlockT block = new BlockT
                {
                    PosX = 1,
                    PosY = 1,
                };
            block.Dump();
            Console.WriteLine("--------");

            for (int i = 0; i < 4; i++)
            {
                Console.WriteLine("Perform rotation");
                bool rotated = block.RotateClockwise(grid, width, height);
                block.Dump();
                Console.WriteLine("Dumping grid");
                for (int j = 0; j < width * height; j++)
                {
                    int x = j%width;
                    int y = j/height;
                    bool foundPart = false;
                    if (x >= block.PosX && x <= block.PosX+4 && y >= block.PosY && y <= block.PosY+4)
                        for (int k = 0; k < 16; k++)
                            if (block.Parts[k] > 0)
                            {
                                int partInGlobalX = (k%4) + block.PosX;
                                int partInGlobalY = (k/4) + block.PosY;
                                if (partInGlobalX == x && partInGlobalY == y)
                                {
                                    Console.Write(block.Parts[k]);
                                    foundPart = true;
                                    break;
                                }
                            }
                    if (!foundPart)
                        Console.Write(grid[j]);
                    if ((j + 1) % width == 0)
                        Console.WriteLine();
                }
                Console.WriteLine("--------");
            }
        }

        public class DummyUserInterface
        {
            public IClient Client { get; private set; }

            public DummyUserInterface(IClient client)
            {
                Client = client;
            }
        }

        static void Main2(string[] args)
        {
            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            Client client = new Client(callback => new WCFProxy(baseAddress, callback));
            client.Name = "joel-wpf-client";
            client._proxy.RegisterPlayer(client, client.Name);
            //
            DummyUserInterface dummyUI = new DummyUserInterface(client);

            //
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
