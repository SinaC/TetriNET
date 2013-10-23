using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.ConsoleWCFServer.Ban;
using TetriNET.ConsoleWCFServer.Host;
using TetriNET.ConsoleWCFServer.Player;
using TetriNET.Server.Interfaces;

namespace TetriNET.ConsoleWCFServer
{
    class Program
    {
        static void DisplayHelp()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("x: Stop server");
            Console.WriteLine("s: Start game");
            Console.WriteLine("t: Stop game");
            Console.WriteLine("p: Pause game");
            Console.WriteLine("r: Resume game");
            Console.WriteLine("+: add dummy player");
            Console.WriteLine("-: remove dummy player");
            Console.WriteLine("l: dummy player lose");
            Console.WriteLine("d: dump player list");
            Console.WriteLine("w: dump win list");
            Console.WriteLine("q: reset win list");
            Console.WriteLine("*: toggle sudden death");
            Console.WriteLine("o: dump options");
        }

        static void Main(string[] args)
        {
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;
            string product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false)).Product;
            Log.WriteLine(Log.LogLevels.Info, "{0} {1}.{2} by {3}", product, version.Major, version.Minor, company);

            //
            BanManager banManager = new BanManager();

            //
            PlayerManager playerManager = new PlayerManager(6);

            //
            Server.WCFHost.WCFHost wcfHost = new Server.WCFHost.WCFHost(playerManager, banManager, (playerName, callback) => new Player.Player(playerName, callback))
            {
                Port = ConfigurationManager.AppSettings["port"]
            };

            //
            BuiltInHost builtInHost = new BuiltInHost(playerManager, banManager, (playerName, callback) => new Player.Player(playerName, callback));

            //
            //SocketHost socketHost = new SocketHost(playerManager, (playerName, callback) => new Player(playerName, callback))
            //{
            //    Port = 5656
            //};

            //
            List<DummyBuiltInClient> clients = new List<DummyBuiltInClient>
            {
                //new DummyBuiltInClient("BuiltIn-Joel" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost),
                //new DummyBuiltInClient("BuiltIn-Celine" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost)
            };

            //
            Server.Server server = new Server.Server(playerManager, wcfHost, builtInHost);
            //Server server = new Server(playerManager, socketHost);

            //
            server.StartServer();

            DisplayHelp();

            bool stopped = false;
            while (!stopped)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo cki = Console.ReadKey(true);
                    switch (cki.Key)
                    {
                        default:
                            DisplayHelp();
                            break;
                        case ConsoleKey.X:
                            server.StopServer();
                            stopped = true;
                            break;
                        case ConsoleKey.S:
                            server.StartGame();
                            break;
                        case ConsoleKey.T:
                            server.StopGame();
                            break;
                        case ConsoleKey.P:
                            server.PauseGame();
                            break;
                        case ConsoleKey.R:
                            server.ResumeGame();
                            break;
                        case ConsoleKey.Add:
                            clients.Add(new DummyBuiltInClient("BuiltIn-" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost));
                            break;
                        case ConsoleKey.Subtract:
                        {
                            DummyBuiltInClient client = clients.LastOrDefault();
                            if (client != null)
                            {
                                client.DisconnectFromServer();
                                clients.Remove(client);
                            }
                            break;
                        }
                        case ConsoleKey.L:
                        {
                            DummyBuiltInClient client = clients.LastOrDefault();
                            if (client != null)
                                client.Lose();
                            break;
                        }
                        case ConsoleKey.D:
                            foreach (IPlayer p in playerManager.Players)
                                Console.WriteLine("{0}) {1} [{2}] {3} {4} {5:HH:mm:ss.fff} {6:HH:mm:ss.fff}", playerManager.GetId(p), p.Name, p.Team, p.State, p.PieceIndex, p.LastActionFromClient, p.LastActionToClient);
                            break;
                        case ConsoleKey.W:
                            foreach(WinEntry e in server.WinList)
                                Console.WriteLine("{0}[{1}]: {2} pts", e.PlayerName, e.Team, e.Score);
                            break;
                        case ConsoleKey.Q:
                            server.ResetWinList();
                            break;
                        case ConsoleKey.Multiply:
                                server.ToggleSuddenDeath();
                                break;
                        case ConsoleKey.O:
                            {
                                GameOptions options = server.GetOptions();
                                foreach(PieceOccurancy occurancy in options.PieceOccurancies)
                                    Console.WriteLine("{0}:{1}", occurancy.Value, occurancy.Occurancy);
                                foreach(SpecialOccurancy occurancy in options.SpecialOccurancies)
                                    Console.WriteLine("{0}:{1}", occurancy.Value, occurancy.Occurancy);
                            }
                            break;
                    }
                }
                else
                {
                    System.Threading.Thread.Sleep(100);
                    Parallel.ForEach(
                        clients, 
                        client => client.Test());
                }
            }
        }
    }
}
