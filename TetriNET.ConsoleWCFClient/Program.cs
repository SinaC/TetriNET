﻿using System;
using System.Configuration;
using TetriNET.DefaultBoardAndPieces;
using TetriNET.Common.Interfaces;
using TetriNET.ConsoleWCFClient.AI;
using TetriNET.ConsoleWCFClient.UI;

namespace TetriNET.ConsoleWCFClient
{
    public class Program
    {
        static void Main(string[] args)
        {
            string name = "CONSOLE_" + Guid.NewGuid().ToString().Substring(0, 5);

            Logger.Log.Initialize(ConfigurationManager.AppSettings["logpath"], name+".log");

            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            IClient client = new Client.Client(Piece.CreatePiece, () => new Board(12,22));
            //IClient client = new Client.Client((piece, posX, posY, orientation, index) => new MutatedZ(posX, posY, orientation, index), () => new Board(12, 22));

            string baseAddress = ConfigurationManager.AppSettings["address"];
            client.Connect(callback => new WCFProxy.WCFProxy(callback, baseAddress));

            //
            GameController.GameController controller = new GameController.GameController(client);
            //PierreDellacherieOnePieceBot bot = new PierreDellacherieOnePieceBot(client)
            ColinFaheyTwoPiecesBot bot = new ColinFaheyTwoPiecesBot(client)
            {
                SleepTime = 75,
                Activated = true,
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
                        case ConsoleKey.V:
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
                        case ConsoleKey.D:
                            controller.KeyDown(Commands.DiscardFirstSpecial);
                            controller.KeyUp(Commands.DiscardFirstSpecial);
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

            client.Unregister();
            client.Disconnect();
        }
    }
}
