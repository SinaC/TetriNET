using System;
using System.Collections.Generic;
using System.Text;
using TetriNET.Common;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.UI
{
    public class ConsoleUI
    {
        private readonly IClient _client;

        public ConsoleUI(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;
            _client.OnGameStarted += OnGameStarted;
            _client.OnGameFinished += OnGameFinished;
            _client.OnRedraw += OnRedraw;
            _client.OnRedrawBoard += OnRedrawBoard;
            _client.OnTetriminoMoving += OnTetriminoMoving;
            _client.OnTetriminoMoved += OnTetriminoMoved;
            _client.OnPlayerRegistered += OnPlayerRegistered;
            _client.OnWinListModified += OnWinListModified;
            _client.OnServerMasterModified += OnServerMasterModified;
            _client.OnPlayerLost += OnPlayerLost;
            _client.OnPlayerWon += OnPlayerWon;
            _client.OnPlayerJoined += OnPlayerJoined;
            _client.OnPlayerLeft += OnPlayerLeft;
            _client.OnPlayerPublishMessage += OnPlayerPublishMessage;
            _client.OnServerPublishMessage += OnServerPublishMessage;
            _client.OnSpecialUsed += OnSpecialUsed;
            _client.OnRoundStarted += OnRoundStarted;
            _client.OnLinesClearedChanged += OnLinesClearedChanged;
            _client.OnLevelChanged += OnLevelChanged;

            //Console.SetWindowSize(30, 30);
            //Console.BufferWidth = 30;
            //Console.BufferHeight = 30;
        }

        private void OnLevelChanged()
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 6);
            Console.Write("Level: {0}", _client.Level);
        }

        private void OnLinesClearedChanged()
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 5);
            Console.Write("#Lines cleared: {0}", _client.LinesCleared);
        }

        private void OnRoundStarted()
        {
            DisplayNextTetriminoColor();
        }

        private void OnSpecialUsed()
        {
            DisplayInventory();
        }

        private void OnGameFinished()
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 4);
            Console.Write("Game finished");
        }

        private void OnGameStarted()
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 4);
            Console.Write("Game started");

            OnRedraw();
            DisplayNextTetriminoColor();
            OnLinesClearedChanged();
            OnLevelChanged();
        }

        private void OnServerPublishMessage(string msg)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 0);
            Console.Write("SERVER: {0}", msg);
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 0);
            Console.Write("{0}: {1}", playerName, msg);
        }

        private void OnPlayerLeft(int playerId, string playerName)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 2);
            Console.Write("{0} [{1}] left", playerName, playerId);
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 2);
            Console.Write("{0} [{1}] joined", playerName, playerId);
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2, 1);
                Console.Write("Registration succeeded -> {0}", playerId);
            }
            else
            {
                Console.ResetColor();
                Console.SetCursorPosition(60, 1);
                Console.Write("Registration failed!!!");
            }
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 21);
            Console.Write("The winner is {0}", playerName);
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 20);
            Console.Write("Player {0} has lost", playerName);
        }

        private void OnServerMasterModified(bool isServerMaster)
        {
            Console.ResetColor();
            Console.SetCursorPosition(_client.Board.Width + 2, 10);
            if (isServerMaster)
                Console.Write("Yeehaw ... power is ours");
            else
                Console.Write("The power is for another one");
        }

        private void OnWinListModified(List<WinEntry> winList)
        {
            Console.ResetColor();
            for (int i = 0; i < winList.Count; i++)
            {
                Console.SetCursorPosition(_client.Board.Width + 2, 11 + i);
                Console.Write("{0}:{1}", winList[i].PlayerName, winList[i].Score);
            }
        }

        private void OnTetriminoMoved()
        {
            DisplayCurrentTetriminoColor();
        }

        private void OnTetriminoMoving()
        {
            HideCurrentTetriminoColor();
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            // NOP
        }

        private void OnRedraw()
        {
            // Board
            DisplayBoardColor();
            // Tetrimino
            DisplayCurrentTetriminoColor();
            // Inventory
            DisplayInventory();
        }

        private void DisplayBoardColor()
        {
            Console.ResetColor();
            for (int y = _client.Board.Height; y >= 1; y--)
            {
                Console.SetCursorPosition(0, _client.Board.Height - y);
                Console.Write("|");

                for (int x = 1; x <= _client.Board.Width; x++)
                {
                    Console.SetCursorPosition(x, _client.Board.Height - y);
                    byte cellValue = _client.Board[x, y];
                    Tetriminos cellTetrimino = CellHelper.Tetrimino(cellValue);
                    if (cellTetrimino == Tetriminos.Invalid)
                        Console.Write(" ");
                    else
                    {
                        Console.BackgroundColor = GetTetriminoColor(cellTetrimino);
                        Specials cellSpecial = CellHelper.Special(cellValue);
                        if (cellSpecial == Specials.Invalid)
                            Console.Write(" ");
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write(ConvertSpecial(cellSpecial));
                        }
                        Console.ResetColor();
                    }
                }
                Console.SetCursorPosition(_client.Board.Width+1, _client.Board.Height - y);
                Console.Write("|");
            }
            Console.SetCursorPosition(0, _client.Board.Height);
            Console.Write("".PadLeft(_client.Board.Width + 2, '-'));
        }

        private void DisplayCurrentTetriminoColor()
        {
            // draw current tetrimino
            if (_client.CurrentTetrimino != null)
            {
                Tetriminos cellTetrimino = CellHelper.Tetrimino(_client.CurrentTetrimino.Value);
                Console.BackgroundColor = GetTetriminoColor(cellTetrimino);
                for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                {
                    int x, y;
                    _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                    if (x >= 0 && x < _client.Board.Width && y >= 0 && y < _client.Board.Height)
                    {
                        Console.SetCursorPosition(x, _client.Board.Height - y);
                        Console.Write(" ");
                    }
                }
            }
        }

        private void HideCurrentTetriminoColor()
        {
            // draw current tetrimino
            if (_client.CurrentTetrimino != null)
            {
                Console.ResetColor();
                for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                {
                    int x, y;
                    _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                    if (x >= 0 && x < _client.Board.Width && y >= 0 && y < _client.Board.Height)
                    {
                        Console.SetCursorPosition(x, _client.Board.Height - y);
                        Console.Write(" ");
                    }
                }
            }
        }

        private void DisplayNextTetriminoColor()
        {
            // draw next tetrimino
            if (_client.NextTetrimino != null)
            {
                ITetrimino temp = _client.NextTetrimino.Clone();
                int minX, minY, maxX, maxY;
                temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
                // Move to top, left
                temp.Translate(-minX, 0);
                if (maxY > _client.Board.Height)
                    temp.Translate(0,_client.Board.Height- maxY);
                // Display tetrimino
                Tetriminos cellTetrimino = CellHelper.Tetrimino(temp.Value);
                Console.BackgroundColor = GetTetriminoColor(cellTetrimino);
                for (int i = 1; i <= temp.TotalCells; i++)
                {
                    int x, y;
                    temp.GetCellAbsolutePosition(i, out x, out y);
                    Console.SetCursorPosition(x, _client.Board.Height - y);
                    Console.Write(" ");
                }
            }
        }

        private void DisplayBoardNoColor()
        {
            for (int y = _client.Board.Height; y >= 1; y--)
            {
                StringBuilder sb = new StringBuilder("|");
                for (int x = 1; x <= _client.Board.Width; x++)
                {
                    byte cellValue = _client.Board[x, y];
                    Tetriminos cellTetrimino = CellHelper.Tetrimino(cellValue);
                    if (cellTetrimino == Tetriminos.Invalid)
                        sb.Append(" ");
                    else
                    {
                        Specials cellSpecial = CellHelper.Special(cellValue);
                        if (cellSpecial == Specials.Invalid)
                            sb.Append((int) cellTetrimino);
                        else
                            sb.Append(ConvertSpecial(cellSpecial));
                    }
                }
                sb.Append("|");
                Console.SetCursorPosition(0, _client.Board.Height - y);
                Console.Write(sb.ToString());
            }
            Console.SetCursorPosition(0, _client.Board.Height);
            Console.Write("".PadLeft(_client.Board.Width + 2, '-'));
        }

        private void DisplayCurrentTetriminoNoColor()
        {
            // draw current tetrimino
            if (_client.CurrentTetrimino != null)
            {
                Tetriminos cellTetrimino = CellHelper.Tetrimino(_client.CurrentTetrimino.Value);
                Console.BackgroundColor = GetTetriminoColor(cellTetrimino);
                for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                {
                    int x, y;
                    _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                    if (x >= 0 && x < _client.Board.Width && y >= 0 && y < _client.Board.Height)
                    {
                        Console.SetCursorPosition(x, _client.Board.Height - y);
                        Console.Write(_client.CurrentTetrimino.Value);
                    }
                }
            }
        }

        private void HideCurrentTetriminoNoColor()
        {
            // draw current tetrimino
            if (_client.CurrentTetrimino != null)
            {
                Console.ResetColor();
                for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                {
                    int x, y;
                    _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                    if (x >= 0 && x < _client.Board.Width && y >= 0 && y < _client.Board.Height)
                    {
                        Console.SetCursorPosition(x, _client.Board.Height - y);
                        Console.Write(" ");
                    }
                }
            }
        }

        private void DisplayInventory()
        {
            Console.ResetColor();
            List<Specials> specials = _client.Inventory;
            StringBuilder sb2 = new StringBuilder();
            for (int i = 0; i < specials.Count; i++)
            {
                Specials special = specials[i];
                if (i == 0)
                    sb2.Append(String.Format("[{0}]", ConvertSpecial(special)));
                else
                    sb2.Append(ConvertSpecial(special));
            }
            Console.SetCursorPosition(0, _client.Board.Height + 1);
            Console.Write(sb2.ToString().PadRight(20, ' '));
        }

        private ConsoleColor GetTetriminoColor(Tetriminos tetrimino)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return ConsoleColor.Red;
                case Tetriminos.TetriminoJ:
                    return ConsoleColor.Magenta;
                case Tetriminos.TetriminoL:
                    return ConsoleColor.Yellow;
                case Tetriminos.TetriminoO:
                    return ConsoleColor.Cyan;
                case Tetriminos.TetriminoS:
                    return ConsoleColor.Blue;
                case Tetriminos.TetriminoT:
                    return ConsoleColor.Gray;
                case Tetriminos.TetriminoZ:
                    return ConsoleColor.Green;
            }
            return ConsoleColor.Black;
        }

        private char ConvertSpecial(Specials special)
        {
            switch (special)
            {
                case Specials.AddLines:
                    return 'A';
                case Specials.ClearLines:
                    return 'C';
                case Specials.NukeField:
                    return 'N';
                case Specials.RandomBlocksClear:
                    return 'R';
                case Specials.SwitchFields:
                    return 'S';
                case Specials.ClearSpecialBlocks:
                    return 'B';
                case Specials.BlockGravity:
                    return 'G';
                case Specials.BlockQuake:
                    return 'Q';
                case Specials.BlockBomb:
                    return 'O';
            }
            return '?';
        }
    }
}