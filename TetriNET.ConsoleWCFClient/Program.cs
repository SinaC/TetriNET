using System;
using System.Configuration;
using TetriNET.Common;
using TetriNET.Common.Interfaces;
using TetriNET.ConsoleWCFClient.GameController;
using TetriNET.ConsoleWCFClient.Proxy;
using TetriNET.ConsoleWCFClient.TenGen;
using TetriNET.ConsoleWCFClient.UI;

namespace TetriNET.ConsoleWCFClient
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

        public static ITetrimino CloneTetrimino(ITetrimino tetrimino)
        {
            switch (tetrimino.TetriminoValue)
            {
                case Tetriminos.TetriminoI:
                    return new TetriminoI(tetrimino.GridWidth, tetrimino.GridHeight);
                case Tetriminos.TetriminoJ:
                    return new TetriminoJ(tetrimino.GridWidth, tetrimino.GridHeight);
                case Tetriminos.TetriminoL:
                    return new TetriminoL(tetrimino.GridWidth, tetrimino.GridHeight);
                case Tetriminos.TetriminoO:
                    return new TetriminoO(tetrimino.GridWidth, tetrimino.GridHeight);
                case Tetriminos.TetriminoS:
                    return new TetriminoS(tetrimino.GridWidth, tetrimino.GridHeight);
                case Tetriminos.TetriminoT:
                    return new TetriminoT(tetrimino.GridWidth, tetrimino.GridHeight);
                case Tetriminos.TetriminoZ:
                    return new TetriminoZ(tetrimino.GridWidth, tetrimino.GridHeight);
            }
            return null;
        }

        static void Main(string[] args)
        {
            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            IClient client = new Client.Client(callback => new WCFProxy(callback, baseAddress), CreateTetrimino);
            string name = "joel-wpf-client" + Guid.NewGuid().ToString().Substring(0, 5);
            client.Register(name);

            //
            //GameController.GameController controller = new GameController.GameController(client);
            FirstBot bot = new FirstBot(client, CloneTetrimino);
            //
            NaiveConsoleUI ui = new NaiveConsoleUI(client);

            //
            Console.Title = name;
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
                        //case ConsoleKey.LeftArrow:
                        //    controller.KeyDown(Commands.Left);
                        //    controller.KeyUp(Commands.Left);
                        //    break;
                        //case ConsoleKey.RightArrow:
                        //    controller.KeyDown(Commands.Right);
                        //    controller.KeyUp(Commands.Right);
                        //    break;
                        //case ConsoleKey.DownArrow:
                        //    controller.KeyDown(Commands.Down);
                        //    controller.KeyUp(Commands.Down);
                        //    break;
                        //case ConsoleKey.Spacebar:
                        //    controller.KeyDown(Commands.Drop);
                        //    controller.KeyUp(Commands.Drop);
                        //    break;
                        //case ConsoleKey.UpArrow:
                        //    controller.KeyDown(Commands.RotateClockwise);
                        //    controller.KeyUp(Commands.RotateClockwise);
                        //    break;
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
