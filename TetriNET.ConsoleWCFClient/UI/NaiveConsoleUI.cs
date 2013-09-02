using System;
using System.Text;
using TetriNET.Common;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.UI
{
    public class NaiveConsoleUI
    {
        private readonly IClient _client;

        public NaiveConsoleUI(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;
            _client.OnRedraw += Redraw;

            //Console.SetWindowSize(30, 30);
            //Console.BufferWidth = 30;
            //Console.BufferHeight = 30;
        }

        public void Redraw()
        {
            // Board
            for (int y = _client.Board.Height; y >= 1; y--)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 1; x <= _client.Board.Width; x++)
                {
                    byte cellValue = _client.Board[x, y];
                    Tetriminos cellTetrimino = ByteHelper.Tetrimino(cellValue);
                    Specials cellSpecial = ByteHelper.Special(cellValue);
                    if (cellSpecial == 0)
                        sb.Append((int)cellTetrimino);
                    else
                    {
                        switch (cellSpecial)
                        {
                            case Specials.AddLines:
                                sb.Append('A');
                                break;
                            case Specials.ClearLines: 
                                sb.Append('C');
                                break;
                            case Specials.NukeField:
                                sb.Append('N');
                                break;
                            case Specials.RandomBlocksClear:
                                sb.Append('R');
                                break;
                            case Specials.SwitchFields:
                                sb.Append('S');
                                break;
                            case Specials.ClearSpecialBlocks:
                                sb.Append('B');
                                break;
                            case Specials.BlockGravity:
                                sb.Append('G');
                                break;
                            case Specials.BlockQuake:
                                sb.Append('Q');
                                break;
                            case Specials.BlockBomb:
                                sb.Append('O');
                                break;
                        }
                    }
                }
                Console.SetCursorPosition(0, _client.Board.Height-y);
                Console.Write(sb.ToString());
            }
            // Piece
            if (_client.CurrentTetrimino != null)
                for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                {
                    int x, y;
                    _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                    Console.SetCursorPosition(x-1, _client.Board.Height - y);
                    Console.Write(_client.CurrentTetrimino.Value);
                }
            //
            Console.SetCursorPosition(_client.Board.Width + 1, 0);
        }
    }
}