using System;
using System.Configuration;
using TetriNET.Client.Proxy;
using TetriNET.Client.TenGen;

namespace TetriNET.Client
{
    public class Program
    {
        //private static void Main(string[] args)
        //{
        //    const int width = 5;
        //    const int height = 5;
        //    byte[] grid = new byte[width*height];
        //    grid[1 + 1 * width] = 9;

        //    ITetrimino block = new TetriminoS(width, height);
        //    block.Dump();
        //    Console.WriteLine("--------");

        //    for (int i = 0; i < 4; i++)
        //    {
        //        bool rotated = block.RotateClockwise(grid);
        //        Console.WriteLine("Perform rotation {0}", rotated);
        //        //Console.WriteLine("Perform move down");
        //        //bool movedDown = block.MoveDown(grid);
        //        //block.Dump();
        //        Console.WriteLine("Dumping grid");
        //        for (int j = 0; j < width*height; j++)
        //        {
        //            int x = j%width;
        //            int y = j/height;
        //            bool foundPart = false;
        //            if (x >= block.PosX && x <= block.PosX + block.Width && y >= block.PosY && y <= block.PosY + block.Height)
        //                for (int k = 0; k < block.Width*block.Height; k++)
        //                    if (block.Parts[k] > 0)
        //                    {
        //                        int partInGlobalX = (k % block.Width) + block.PosX;
        //                        int partInGlobalY = (k / block.Width) + block.PosY;
        //                        if (partInGlobalX == x && partInGlobalY == y)
        //                        {
        //                            Console.Write(block.Parts[k]);
        //                            foundPart = true;
        //                            break;
        //                        }
        //                    }
        //            if (!foundPart)
        //                Console.Write(grid[j]);
        //            if ((j + 1)%width == 0)
        //                Console.WriteLine();
        //        }
        //        Console.WriteLine("--------");
        //    }
        //}

        static void Main(string[] args)
        {
            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            Client client = new Client(callback => new WCFProxy(baseAddress, callback), (tetriminos, width, height) => new TetriminoO(width, height) /*TODO get random Tetrimino*/);
            client.Name = "joel-wpf-client";
            client.__Register();

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
                //client.Dump();
                //Console.WriteLine("========================");
            }
        }
    }
}
