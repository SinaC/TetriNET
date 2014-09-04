using System;
using System.Configuration;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.ConsoleWCFClient.AI;
using TetriNET.ConsoleWCFClient.UI;

namespace TetriNET.ConsoleWCFClient
{
    public class Program
    {
        private static void DisplayBotName(IBot bot)
        {
            Console.SetCursorPosition(20, 29);
            if (bot.Activated)
                Console.Write("Bot:{0} {1}", bot.Name, bot.SleepTime);
            else
                Console.Write("NO BOT ACTIVATED");
        }

        static void Main(string[] args)
        {
            const string team = "CONSOLE";
            string name = "CONSOLE_" + Guid.NewGuid().ToString().Substring(0, 5);

            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], name + ".log");

            //
            IFactory factory = new Factory();

            //
            //string baseAddress = @"net.tcp://localhost:8765/TetriNET";
            IAchievementManager manager = factory.CreateAchievementManager();
            manager.FindAllAchievements();
            IClient client = new Client.Client(factory, manager);

            string baseAddress = ConfigurationManager.AppSettings["address"];
            client.ConnectAndRegisterAsPlayer(baseAddress, name, team);

            //
            GameController.GameController controller = new GameController.GameController(client);
            PierreDellacherieOnePieceBot bot1 = new PierreDellacherieOnePieceBot(client)
            {
                SleepTime = 75,
                Activated = false,
            };
            ColinFaheyTwoPiecesBot bot2 = new ColinFaheyTwoPiecesBot(client)
            {
                SleepTime = 75,
                Activated = false,
            };
            //
            ConsoleUI ui = new ConsoleUI(client);
            //
            IBot bot = bot1;
            bot.Activated = true;
            DisplayBotName(bot);

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
                        case ConsoleKey.Add:
                            bot.SleepTime += 100;
                            DisplayBotName(bot);
                            break;
                        case ConsoleKey.Subtract:
                            bot.SleepTime -= 100;
                            DisplayBotName(bot);
                            break;
                        case ConsoleKey.A:
                            bot.Activated = !bot.Activated;
                            break;
                        case ConsoleKey.Z:
                            bot.Activated = false;
                            if (bot == bot1)
                                bot = bot2;
                            else
                                bot = bot1;
                            bot.Activated = true;
                            DisplayBotName(bot);
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
                        case ConsoleKey.H:
                            controller.KeyDown(Commands.Hold);
                            controller.KeyUp(Commands.Hold);
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
                        case ConsoleKey.Enter:
                            controller.KeyDown(Commands.UseSpecialOnSelf);
                            controller.KeyUp(Commands.UseSpecialOnSelf);
                            break;
                        case ConsoleKey.Tab:
                            controller.KeyDown(Commands.UseSpecialOnRandomOpponent);
                            controller.KeyUp(Commands.UseSpecialOnRandomOpponent);
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                }
            }

            client.UnregisterAndDisconnect();
        }
    }
}
