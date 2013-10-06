using System;
using System.Text;
using TetriNET.Client.Board;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Test();
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
