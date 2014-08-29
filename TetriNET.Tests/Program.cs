using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using TetriNET.Client.Board;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
using TetriNET.Common.Attributes;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Randomizer;

namespace TetriNET.Tests
{
    class Program
    {
        private class IntOccurancy : IOccurancy<int>
        {
            public int Value { get; set; }
            public int Occurancy { get; set; }
        }

        static void Main(string[] args)
        {
            Ioc.EasyIoc.Default.Register<IMoveStrategy, PierreDellacherieOnePiece>();
            bool registered = Ioc.EasyIoc.Default.IsRegistered<IMoveStrategy>();
            IMoveStrategy strategy = Ioc.EasyIoc.Default.Resolve<IMoveStrategy>();

            //Program p = new Program();
            //p.TestSpectator();

            List<IntOccurancy> occurancies = new List<IntOccurancy>
                {
                    new IntOccurancy
                        {
                            Value = 0,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 1,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 2,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 3,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 4,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 5,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 6,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 7,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 8,
                            Occurancy = 10,
                        },
                    new IntOccurancy
                        {
                            Value = 9,
                            Occurancy = 10,
                        },
                };
            int[] history =
                {
                    1, 2, 3, 4
                };
            int r = RangeRandom.Random(occurancies, history);
        }

        public class TestCallback : ITetriNETCallback
        {
            #region Implementation of ITetriNETCallback

            public void OnHeartbeatReceived()
            {
                throw new NotImplementedException();
            }
            public void OnServerStopped()
            {
                throw new NotImplementedException();
            }
            public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
            {
                throw new NotImplementedException();
            }
            public void OnPlayerJoined(int playerId, string name, string team)
            {
                throw new NotImplementedException();
            }
            public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
            {
                throw new NotImplementedException();
            }
            public void OnPlayerTeamChanged(int playerId, string team)
            {
                throw new NotImplementedException();
            }
            public void OnPublishPlayerMessage(string playerName, string msg)
            {
                throw new NotImplementedException();
            }
            public void OnPublishServerMessage(string msg)
            {
                throw new NotImplementedException();
            }
            public void OnPlayerLost(int playerId)
            {
                throw new NotImplementedException();
            }
            public void OnPlayerWon(int playerId)
            {
                throw new NotImplementedException();
            }
            public void OnGameStarted(List<Pieces> pieces)
            {
                throw new NotImplementedException();
            }
            public void OnGameFinished(GameStatistics statistics)
            {
                throw new NotImplementedException();
            }
            public void OnGamePaused()
            {
                throw new NotImplementedException();
            }
            public void OnGameResumed()
            {
                throw new NotImplementedException();
            }
            public void OnServerAddLines(int lineCount)
            {
                throw new NotImplementedException();
            }
            public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
            {
                throw new NotImplementedException();
            }
            public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
            {
                throw new NotImplementedException();
            }
            public void OnNextPiece(int firstIndex, List<Pieces> piece)
            {
                throw new NotImplementedException();
            }
            public void OnGridModified(int playerId, byte[] grid)
            {
                throw new NotImplementedException();
            }
            public void OnServerMasterChanged(int playerId)
            {
                throw new NotImplementedException();
            }
            public void OnWinListModified(List<WinEntry> winList)
            {
                throw new NotImplementedException();
            }
            public void OnContinuousSpecialFinished(int playerId, Specials special)
            {
                throw new NotImplementedException();
            }
            public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
            {
                throw new NotImplementedException();
            }
            public void OnOptionsChanged(GameOptions options)
            {
                throw new NotImplementedException();
            }
            public void OnSpectatorRegistered(RegistrationResults result, int spectatorId, bool gameStarted, GameOptions options)
            {
                throw new NotImplementedException();
            }
            public void OnSpectatorJoined(int spectatorId, string name)
            {
                throw new NotImplementedException();
            }
            public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public void TestSpectator()
        {
            TestCallback callback = new TestCallback();

            // Get WCF endpoint
            EndpointAddress endpointAddress = new EndpointAddress("net.tcp://localhost:8765/TetriNETSpectator");

            Binding binding = new NetTcpBinding(SecurityMode.None);
            InstanceContext instanceContext = new InstanceContext(callback);
            //_proxy = DuplexChannelFactory<IWCFTetriNET>.CreateChannel(instanceContext, binding, endpointAddress);
            DuplexChannelFactory<IWCFTetriNETSpectator> factory = new DuplexChannelFactory<IWCFTetriNETSpectator>(instanceContext, binding, endpointAddress);
            IWCFTetriNETSpectator proxy = factory.CreateChannel(instanceContext);

            proxy.RegisterSpectator("joel");

            while(true)
            {
                System.Threading.Thread.Sleep(10);
            }
        }

        public void Test()
        {
            IBoard board = new Board(4, 12);
            byte[] cells = new byte[board.Width*board.Height];
            for (int i = 0; i < board.Height; i += 3)
                cells[2 + i * board.Width] = 1;
            board.SetCells(cells);

            DisplayBoard(board);

            Console.ReadLine();

            board.LeftGravity();
            DisplayBoard(board);

            int buriedHoles = BoardHelper.GetBuriedHolesForColumn(board, 1);
            int holeDepth = BoardHelper.GetHoleDepthForColumn(board, 1);
            int allWells = BoardHelper.GetAllWellsForColumn(board, 1);
            int blockades = BoardHelper.GetBlockadesForColumn(board, 1);

            Console.WriteLine();
            Console.WriteLine("{0} {1} {2} {3}", buriedHoles, holeDepth, allWells, blockades);
        }

        public void DisplayBoard(IBoard board)
        {
            for (int y = board.Height; y >= 1; y--)
            {
                StringBuilder sb = new StringBuilder(String.Format("{0:00}|", y));
                for (int x = 1; x <= board.Width; x++)
                {
                    byte cellValue = board[x, y];
                    if (cellValue == CellHelper.EmptyCell)
                        sb.Append(".");
                    else
                    {
                        Pieces cellPiece = CellHelper.GetColor(cellValue);
                        Specials cellSpecial = CellHelper.GetSpecial(cellValue);
                        if (cellSpecial == Specials.Invalid)
                            sb.Append((int)cellPiece);
                        else
                            sb.Append(ConvertSpecial(cellSpecial));
                    }
                }
                sb.Append("|");
                Console.SetCursorPosition(0 + 0, board.Height - y + 0);
                Console.Write(sb.ToString());
            }
            Console.SetCursorPosition(0 + 2, board.Height + 0);
            Console.Write("".PadLeft(board.Width + 2, '-'));
        }

        private static char ConvertSpecial(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return attribute == null ? '?' : attribute.ShortName;
        }
    }
}
