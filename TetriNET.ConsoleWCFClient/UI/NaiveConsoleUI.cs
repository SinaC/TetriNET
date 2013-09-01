using System;
using System.Text;
using TetriNET.Client;
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
            // TODO
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < _client.Width*_client.Height; i++)
            //{
            //    int x = i%_client.Width;
            //    int y = i/_client.Width;
            //    bool foundPart = false;
            //    if (_client.CurrentTetrimino != null)
            //        if (x >= _client.CurrentTetrimino.PosX && x < _client.CurrentTetrimino.PosX + _client.CurrentTetrimino.Width && y >= _client.CurrentTetrimino.PosY && y < _client.CurrentTetrimino.PosY + _client.CurrentTetrimino.Height)
            //        {
            //            int partX = x - _client.CurrentTetrimino.PosX;
            //            int partY = y - _client.CurrentTetrimino.PosY;
            //            int linearPartPos = partY * _client.CurrentTetrimino.Width + partX;
            //            if (_client.CurrentTetrimino.Parts[linearPartPos] > 0)
            //            {
            //                sb.Append(_client.CurrentTetrimino.Parts[linearPartPos]);
            //                foundPart = true;
            //            }
            //        }
            //    if (!foundPart)
            //        sb.Append(_client.Grid[i]);
            //    if ((i + 1)%_client.Width == 0)
            //    {
            //        Console.SetCursorPosition(0, y);
            //        Console.WriteLine(sb.ToString());
            //        sb.Clear();
            //    }
            //}
            // Board
            for (int y = _client.Board.Height; y >= 1; y--)
            {
                StringBuilder sb = new StringBuilder();
                for (int x = 1; x <= _client.Board.Width; x++)
                    sb.Append(_client.Board[x, y]);
                Console.SetCursorPosition(0, _client.Board.Height-y);
                Console.Write(sb.ToString());
            }
            // Piece
            for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
            {
                int x, y;
                _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                Console.SetCursorPosition(x-1, _client.Board.Height - y);
                Console.Write(_client.CurrentTetrimino.Value);
            }
            //
            Console.SetCursorPosition(0, _client.Board.Height);
            Console.WriteLine("============================");
            Console.SetCursorPosition(_client.Board.Width + 1, 0);
        }
    }
}