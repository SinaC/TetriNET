﻿using System;
using System.Configuration;
using TetriNET.Client;
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
            return null;
            //return new TetriminoZ(spawnX, spawnY, spawnOrientation);
        }

        static void Main(string[] args)
        {
            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];
            IClient client = new Client.Client(callback => new WCFProxy(callback, baseAddress), CreateTetrimino, () => new Board(12,22));
            string name = "joel-wpf-client" + Guid.NewGuid().ToString().Substring(0, 5);
            client.Register(name);

            //
            GameController.GameController controller = new GameController.GameController(client);
            //FirstBot bot = new FirstBot(client);
            PierreDellacherieOnePieceBot bot = new PierreDellacherieOnePieceBot(client);
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
