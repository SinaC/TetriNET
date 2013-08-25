using System;
using System.Configuration;
using TetriNET.Client.GameController;
using TetriNET.Client.Proxy;
using TetriNET.Client.TenGen;
using TetriNET.Client.UI;
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
                    return new TetriminoT(width, height);
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
            Client client = new Client(callback => new WCFProxy(callback, baseAddress), CreateTetrimino);
            client.Name = "joel-wpf-client"+Guid.NewGuid().ToString().Substring(0,5);
            client.__Register();

            //
            GameController.GameController controller = new GameController.GameController(client);
            //
            NaiveConsoleUI ui = new NaiveConsoleUI(client);

            //
            Console.Title = client.Name;
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
                        case ConsoleKey.D:
                            client.Dump();
                            break;
                        case ConsoleKey.P:
                            client.DumpPlayers();
                            break;
                        case ConsoleKey.LeftArrow:
                            controller.KeyDown(Commands.Left);
                            controller.KeyUp(Commands.Left);
                            break;
                        case ConsoleKey.RightArrow:
                            controller.KeyDown(Commands.Right);
                            controller.KeyUp(Commands.Right);
                            break;
                        case ConsoleKey.DownArrow:
                            controller.KeyDown(Commands.Down);
                            controller.KeyUp(Commands.Down);
                            break;
                        case ConsoleKey.Spacebar:
                            controller.KeyDown(Commands.Drop);
                            controller.KeyUp(Commands.Drop);
                            break;
                        case ConsoleKey.UpArrow:
                            controller.KeyDown(Commands.RotateClockwise);
                            controller.KeyUp(Commands.RotateClockwise);
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
