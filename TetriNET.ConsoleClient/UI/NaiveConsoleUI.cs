using System;
using System.Text;
using TetriNET.Common;

namespace TetriNET.Client.UI
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < _client.Width*_client.Height; i++)
            {
                int x = i%_client.Width;
                int y = i/_client.Width;
                bool foundPart = false;
                if (_client.CurrentTetrimino != null)
                    if (x >= _client.CurrentTetrimino.PosX && x <= _client.CurrentTetrimino.PosX + _client.CurrentTetrimino.Width && y >= _client.CurrentTetrimino.PosY && y <= _client.CurrentTetrimino.PosY + _client.CurrentTetrimino.Height)
                        for (int j = 0; j < _client.CurrentTetrimino.Width*_client.CurrentTetrimino.Height; j++)
                            if (_client.CurrentTetrimino.Parts[j] > 0)
                            {
                                int partInGlobalX = (j%_client.CurrentTetrimino.Width) + _client.CurrentTetrimino.PosX;
                                int partInGlobalY = (j/_client.CurrentTetrimino.Width) + _client.CurrentTetrimino.PosY;
                                if (partInGlobalX == x && partInGlobalY == y)
                                {
                                    sb.Append(_client.CurrentTetrimino.Parts[j]);
                                    foundPart = true;
                                    break;
                                }
                            }
                if (!foundPart)
                    sb.Append(_client.Grid[i]);
                if ((i + 1)%_client.Width == 0)
                {
                    Console.SetCursorPosition(0, y);
                    Console.WriteLine(sb.ToString());
                    sb.Clear();
                }
            }
            Console.SetCursorPosition(0, _client.Width + 1);
        }
    }
}