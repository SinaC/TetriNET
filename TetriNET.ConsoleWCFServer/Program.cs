using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.ConsoleWCFServer.Host;
using TetriNET.Server.Interfaces;
using TetriNET.Server.TCPHost;

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
            Console.WriteLine("i: dump statistics");
        }

        static void Main(string[] args)
        {
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyCompanyAttribute), false)).Company;
            string product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(Assembly.GetExecutingAssembly(), typeof(AssemblyProductAttribute), false)).Product;
            Log.WriteLine(Log.LogLevels.Info, "{0} {1}.{2} by {3}", product, version.Major, version.Minor, company);

            //
            IFactory factory = new Factory();

            //
            IBanManager banManager = factory.CreateBanManager();

            //
            IPlayerManager playerManager = factory.CreatePlayerManager(6);
            ISpectatorManager spectatorManager = factory.CreateSpectatorManager(10);

            //
            IHost wcfHost = new Server.WCFHost.WCFHost(
                playerManager, 
                spectatorManager, 
                banManager, 
                factory)
            {
                Port = ConfigurationManager.AppSettings["port"]
            };

            //
            IHost builtInHost = new BuiltInHost(
                playerManager,
                spectatorManager,
                banManager,
                factory);


            IHost socketHost = new TCPHost(playerManager,
                spectatorManager,
                banManager,
                factory)
            {
                Port = 5656
            };

            //
            List<DummyBuiltInClient> clients = new List<DummyBuiltInClient>
            {
                //new DummyBuiltInClient("BuiltIn-Joel" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost),
                //new DummyBuiltInClient("BuiltIn-Celine" + Guid.NewGuid().ToString().Substring(0, 5), () => builtInHost)
            };

            //
            IPieceProvider pieceProvider = factory.CreatePieceProvider();

            //
            IServer server = new Server.Server(playerManager, spectatorManager, pieceProvider, wcfHost, builtInHost, socketHost);

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
                            Console.WriteLine("Players:");
                            foreach (IPlayer p in playerManager.Players)
                                Console.WriteLine("{0}) {1} [{2}] {3} {4} {5:HH:mm:ss.fff} {6:HH:mm:ss.fff}", p.Id, p.Name, p.Team, p.State, p.PieceIndex, p.LastActionFromClient, p.LastActionToClient);
                            Console.WriteLine("Spectators:");
                            foreach (ISpectator s in spectatorManager.Spectators)
                                Console.WriteLine("{0}) {1} {2:HH:mm:ss.fff} {3:HH:mm:ss.fff}", s.Id, s.Name, s.LastActionFromClient, s.LastActionToClient);
                            break;
                        case ConsoleKey.W:
                            foreach (WinEntry e in server.WinList)
                                Console.WriteLine("{0}[{1}]: {2} pts", e.PlayerName, e.Team, e.Score);
                            break;
                        case ConsoleKey.Q:
                            server.ResetWinList();
                            break;
                        case ConsoleKey.O:
                            {
                                GameOptions options = server.Options;
                                foreach (PieceOccurancy occurancy in options.PieceOccurancies)
                                    Console.WriteLine("{0}:{1}", occurancy.Value, occurancy.Occurancy);
                                foreach (SpecialOccurancy occurancy in options.SpecialOccurancies)
                                    Console.WriteLine("{0}:{1}", occurancy.Value, occurancy.Occurancy);
                            }
                            break;
                        case ConsoleKey.I:
                            {
                                foreach(KeyValuePair<string, GameStatisticsByPlayer> byPlayer in server.GameStatistics)
                                {
                                    GameStatisticsByPlayer stats = byPlayer.Value;
                                    string playerName = byPlayer.Key;
                                    Console.WriteLine("{0}) {1} : 1:{2} 2:{3} 3:{4} 4:{5}", byPlayer.Key, playerName, stats.SingleCount, stats.DoubleCount, stats.TripleCount, stats.TetrisCount);
                                    foreach (KeyValuePair<Specials, Dictionary<string, int>> bySpecial in stats.SpecialsUsed)
                                    {
                                        Console.WriteLine("Special {0}", bySpecial.Key);
                                        foreach (KeyValuePair<string, int> kv in bySpecial.Value)
                                        {
                                            string otherName = kv.Key;
                                            Console.WriteLine("\t{0}:{1}", otherName, kv.Value);
                                        }
                                    }
                                }
                                break;
                            }
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
