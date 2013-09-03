using System;
using System.Configuration;
using TetriNET.Client;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common;
using TetriNET.Common.Interfaces;
using TetriNET.ConsoleWCFClient.GameController;
using TetriNET.ConsoleWCFClient.Proxy;
using TetriNET.ConsoleWCFClient.UI;

namespace TetriNET.ConsoleWCFClient
{
    public class Program
    {
        public static ITetrimino CreateTetrimino(Tetriminos tetrimino, int spawnX, int spawnY, int spawnOrientation)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return new TetriminoI(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoJ:
                    return new TetriminoJ(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoL:
                    return new TetriminoL(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoO:
                    return new TetriminoO(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoS:
                    return new TetriminoS(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoT:
                    return new TetriminoT(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoZ:
                    return new TetriminoZ(spawnX, spawnY, spawnOrientation);
            }
            return new TetriminoZ(spawnX, spawnY, spawnOrientation); // TODO: sometimes server takes time to send next tetrimino, it should send 2 or 3 next tetriminoes to ensure this never happens
            //return new TetriminoL(spawnX, spawnY, spawnOrientation);
        }

        static void Main(string[] args)
        {
            Log.DisplayThreadId = false;
            Log.DisplayInConsole = false;

            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            IClient client = new Client.Client(callback => new WCFProxy(callback, baseAddress), CreateTetrimino, () => new Board(12,22));
            string name = "joel-wpf-client" + Guid.NewGuid().ToString().Substring(0, 5);

            //
            GameController.GameController controller = new GameController.GameController(client);
            PierreDellacherieOnePieceBot bot = new PierreDellacherieOnePieceBot(client)
            {
                SleepTime = 250
            };
            //
            ConsoleUI ui = new ConsoleUI(client);

            //
            client.Register(name);

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
                        //
                        case ConsoleKey.X:
                            stopped = true;
                            break;
                        case ConsoleKey.D:
                            client.Dump();
                            break;
                        case ConsoleKey.S:
                            client.StartGame();
                            break;
                        case ConsoleKey.T:
                            client.StopGame();
                            break;
                        case ConsoleKey.P:
                            client.PauseGame();
                            break;
                        case ConsoleKey.R:
                            client.ResumeGame();
                            break;

                        // Bot
                        case ConsoleKey.A:
                            bot.Activated = !bot.Activated;
                            break;

                        // Game controller
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
                        case ConsoleKey.NumPad1:
                        case ConsoleKey.D1:
                            controller.KeyDown(Commands.UseSpecialOn1);
                            controller.KeyUp(Commands.UseSpecialOn1);
                            break;
                        case ConsoleKey.NumPad2:
                        case ConsoleKey.D2:
                            controller.KeyDown(Commands.UseSpecialOn2);
                            controller.KeyUp(Commands.UseSpecialOn2);
                            break;
                        case ConsoleKey.NumPad3:
                        case ConsoleKey.D3:
                            controller.KeyDown(Commands.UseSpecialOn3);
                            controller.KeyUp(Commands.UseSpecialOn3);
                            break;
                        case ConsoleKey.NumPad4:
                        case ConsoleKey.D4:
                            controller.KeyDown(Commands.UseSpecialOn4);
                            controller.KeyUp(Commands.UseSpecialOn4);
                            break;
                        case ConsoleKey.NumPad5:
                        case ConsoleKey.D5:
                            controller.KeyDown(Commands.UseSpecialOn5);
                            controller.KeyUp(Commands.UseSpecialOn5);
                            break;
                        case ConsoleKey.NumPad6:
                        case ConsoleKey.D6:
                            controller.KeyDown(Commands.UseSpecialOn6);
                            controller.KeyUp(Commands.UseSpecialOn6);
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
