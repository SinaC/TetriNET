using System;
using System.Collections.Generic;
using System.Text;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.UI
{
    public class ConsoleUI
    {
        private const int BoardStartX = 0;
        private const int BoardStartY = 3;

        private readonly object _lock = new object();
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
            _client.OnInventoryChanged += OnInventoryChanged;
            _client.OnRoundStarted += OnRoundStarted;
            _client.OnRoundFinished += OnRoundFinished;
            _client.OnLinesClearedChanged += OnLinesClearedChanged;
            _client.OnLevelChanged += OnLevelChanged;
            _client.OnSpecialUsed += OnSpecialUsed;
            _client.OnPlayerAddLines += OnPlayerAddLines;

            Console.SetWindowSize(80, 30);
            Console.BufferWidth = 80;
            Console.BufferHeight = 30;
        }

        private void OnPlayerAddLines(string playerName, int specialId, int count)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 8 + (specialId%10));
                Console.Write("{0}. {1} line{2} added to All from {3}", specialId, count, (count > 1) ? "s" : "", playerName );
            }
        }

        private void OnSpecialUsed(string playerName, string targetName, int specialId, Specials special)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 8 + (specialId%10));
                Console.Write("{0}. {1} on {2} from {3}", specialId, GetSpecialString(special), targetName, playerName);
            }
        }

        private void OnLevelChanged()
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 6);
                Console.Write("Level: {0}", _client.Level);
            }
        }

        private void OnLinesClearedChanged()
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 5);
                Console.Write("#Lines cleared: {0}", _client.LinesCleared);
            }
        }

        private void OnRoundFinished()
        {
            HideNextTetriminoColor();
        }

        private void OnRoundStarted()
        {
            DisplayNextTetriminoColor();
        }

        private void OnInventoryChanged()
        {
            DisplayInventory();
        }

        private void OnGameFinished()
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 4);
                Console.Write("Game finished");
            }
        }

        private void OnGameStarted()
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 4);
                Console.Write("Game started");
            }

            OnRedraw();
            DisplayNextTetriminoColor();
            OnLinesClearedChanged();
            OnLevelChanged();
        }

        private void OnServerPublishMessage(string msg)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
                Console.Write("SERVER: {0}", msg);
            }
        }

        private void OnPlayerPublishMessage(string playerName, string msg)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
                Console.Write("{0}: {1}", playerName, msg);
            }
        }

        private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 2);
                Console.Write("{0} [{1}] left {2}", playerName, playerId, reason);
            }
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 2);
                Console.Write("{0} [{1}] joined", playerName, playerId);
            }
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                lock (_lock)
                {
                    Console.ResetColor();
                    Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 1);
                    Console.Write("Registration succeeded -> {0}", playerId);
                }
            }
            else
            {
                lock (_lock)
                {
                    Console.ResetColor();
                    Console.SetCursorPosition(60, 1);
                    Console.Write("Registration failed!!!");
                }
            }
        }

        private void OnPlayerWon(int playerId, string playerName)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 21);
                Console.Write("The winner is {0}", playerName);
            }
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 20);
                Console.Write("Player {0} has lost", playerName);
            }
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 3);
                if (_client.IsServerMaster)
                    Console.Write("Yeehaw ... power is ours");
                else
                    Console.Write("The power is for another one");
            }
        }

        private void OnWinListModified(List<WinEntry> winList)
        {
            lock (_lock)
            {
                Console.ResetColor();
                // Display only top 5
                winList.Sort((entry1, entry2) => entry2.Score.CompareTo(entry1.Score)); // descending
                for (int i = 0; i < (winList.Count > 5 ? 5 : winList.Count); i++)
                {
                    Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 22 + i);
                    Console.Write("{0}:{1}", winList[i].PlayerName, winList[i].Score);
                }
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
            lock (_lock)
            {
                Console.ResetColor();
                for (int y = _client.Board.Height; y >= 1; y--)
                {
                    Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height - y + BoardStartY);
                    Console.Write("|");

                    for (int x = 1; x <= _client.Board.Width; x++)
                    {
                        Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                        byte cellValue = _client.Board[x, y];
                        if (cellValue == CellHelper.EmptyCell)
                            Console.Write(".");
                        else
                        {
                            Tetriminos cellTetrimino = CellHelper.GetColor(cellValue);
                            Console.BackgroundColor = GetTetriminoColor(cellTetrimino);
                            Specials cellSpecial = CellHelper.GetSpecial(cellValue);
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
                    Console.SetCursorPosition(_client.Board.Width + 1 + BoardStartX, _client.Board.Height - y + BoardStartY);
                    Console.Write("|");
                }
                Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height + BoardStartY);
                Console.Write("".PadLeft(_client.Board.Width + 2, '-'));
            }
        }

        private void DisplayCurrentTetriminoColor()
        {
            lock (_lock)
            {
                // draw current tetrimino
                if (_client.CurrentTetrimino != null)
                {
                    Tetriminos cellTetrimino = _client.CurrentTetrimino.Value;
                    Console.BackgroundColor = GetTetriminoColor(cellTetrimino);
                    for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                    {
                        int x, y;
                        _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                        if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
                        {
                            Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                            Console.Write(" ");
                        }
                    }
                }
            }
        }

        private void HideCurrentTetriminoColor()
        {
            lock (_lock)
            {
                // hide current tetrimino
                if (_client.CurrentTetrimino != null)
                {
                    Console.ResetColor();
                    for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                    {
                        int x, y;
                        _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                        if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
                        {
                            Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                            Console.Write(".");
                        }
                    }
                }
            }
        }

        private void DisplayNextTetriminoColor()
        {
            lock (_lock)
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
                        temp.Translate(0, _client.Board.Height - maxY);
                    // Display tetrimino
                    Tetriminos cellTetrimino = temp.Value;
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
        }

        private void HideNextTetriminoColor()
        {
            lock (_lock)
            {
                // hide next tetrimino
                if (_client.NextTetrimino != null)
                {
                    Console.ResetColor();
                    ITetrimino temp = _client.NextTetrimino.Clone();
                    int minX, minY, maxX, maxY;
                    temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
                    // Move to top, left
                    temp.Translate(-minX, 0);
                    if (maxY > _client.Board.Height)
                        temp.Translate(0, _client.Board.Height - maxY);
                    // hide tetrimino
                    for (int i = 1; i <= temp.TotalCells; i++)
                    {
                        int x, y;
                        temp.GetCellAbsolutePosition(i, out x, out y);
                        Console.SetCursorPosition(x, _client.Board.Height - y);
                        Console.Write(" ");
                    }
                }
            }
        }

        private void DisplayBoardNoColor()
        {
            lock (_lock)
            {
                for (int y = _client.Board.Height; y >= 1; y--)
                {
                    StringBuilder sb = new StringBuilder("|");
                    for (int x = 1; x <= _client.Board.Width; x++)
                    {
                        byte cellValue = _client.Board[x, y];
                        if (cellValue == CellHelper.EmptyCell)
                            sb.Append(".");
                        else {
                            Tetriminos cellTetrimino = CellHelper.GetColor(cellValue);
                            Specials cellSpecial = CellHelper.GetSpecial(cellValue);
                            if (cellSpecial == Specials.Invalid)
                                sb.Append((int) cellTetrimino);
                            else
                                sb.Append(ConvertSpecial(cellSpecial));
                        }
                    }
                    sb.Append("|");
                    Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height - y + BoardStartY);
                    Console.Write(sb.ToString());
                }
                Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height + BoardStartY);
                Console.Write("".PadLeft(_client.Board.Width + 2, '-'));
            }
        }

        private void DisplayCurrentTetriminoNoColor()
        {
            lock (_lock)
            {
                // draw current tetrimino
                if (_client.CurrentTetrimino != null)
                {
                    for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                    {
                        int x, y;
                        _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                        if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
                        {
                            Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                            Console.Write(_client.CurrentTetrimino.Value);
                        }
                    }
                }
            }
        }

        private void HideCurrentTetriminoNoColor()
        {
            lock (_lock)
            {
                // draw current tetrimino
                if (_client.CurrentTetrimino != null)
                {
                    for (int i = 1; i <= _client.CurrentTetrimino.TotalCells; i++)
                    {
                        int x, y;
                        _client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y);
                        if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
                        {
                            Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                            Console.Write(".");
                        }
                    }
                }
            }
        }

        private void DisplayInventory()
        {
            lock (_lock)
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
                Console.SetCursorPosition(0, _client.Board.Height + 1 + BoardStartY);
                Console.Write(sb2.ToString().PadRight(20, ' '));
            }
        }

        private ConsoleColor GetTetriminoColor(Tetriminos tetrimino)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return ConsoleColor.Blue;
                case Tetriminos.TetriminoJ:
                    return ConsoleColor.Green;
                case Tetriminos.TetriminoL:
                    return ConsoleColor.Magenta;
                case Tetriminos.TetriminoO:
                    return ConsoleColor.Yellow;
                case Tetriminos.TetriminoS:
                    return ConsoleColor.Blue;
                case Tetriminos.TetriminoT:
                    return ConsoleColor.Yellow;
                case Tetriminos.TetriminoZ:
                    return ConsoleColor.Red;
            }
            return ConsoleColor.Gray;
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
                case Specials.ClearColumn:
                    return 'V';
                case Specials.ZebraField:
                    return 'Z';
            }
            return '?';
        }

        private string GetSpecialString(Specials special)
        {
            switch (special)
            {
                case Specials.AddLines:
                    return "Add Line";
                case Specials.ClearLines:
                    return "Clear Line";
                case Specials.NukeField:
                    return "Nuke Field";
                case Specials.RandomBlocksClear:
                    return "Random Blocks Clear";
                case Specials.SwitchFields:
                    return "Switch Fields";
                case Specials.ClearSpecialBlocks:
                    return "Clear Special Blocks";
                case Specials.BlockGravity:
                    return "Block Gravity";
                case Specials.BlockQuake:
                    return "Block Quake";
                case Specials.BlockBomb:
                    return "Block Bomb";
                case Specials.ClearColumn:
                    return "Clear Column";
                case Specials.ZebraField:
                    return "Zebra Field";
            }
            return special.ToString();
        }
    }
}