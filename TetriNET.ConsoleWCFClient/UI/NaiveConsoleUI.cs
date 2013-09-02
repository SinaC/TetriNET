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