using System;
using System.Configuration;
using TetriNET.Client.Proxy;
using TetriNET.Client.TenGen;
using TetriNET.Common;

namespace TetriNET.Client
{
    public class Program
    {
        public static ITetrimino CreateTetrimino(Tetriminos tetrimino, int width, int height)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return new TetriminoI(width, height);
                case Tetriminos.TetriminoJ:
                    return new TetriminoJ(width, height);
                case Tetriminos.TetriminoL:
                    return new TetriminoL(width, height);
                case Tetriminos.TetriminoO:
                    return new TetriminoO(width, height);
                case Tetriminos.TetriminoS:
                    return new TetriminoS(width, height);
                case Tetriminos.TetriminoT:
                    return new TetriminoI(width, height);
                case Tetriminos.TetriminoZ:
                    return new TetriminoZ(width, height);
            }
            return null;
        }

        static void Main(string[] args)
        {
            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            Client client = new Client(callback => new WCFProxy(baseAddress, callback), CreateTetrimino);
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
                client.Dump();
                Console.WriteLine("========================");
            }
        }
    }
}
