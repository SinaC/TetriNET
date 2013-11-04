using System;
using System.Collections.Generic;
using System.Text;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.ConsoleWCFClient.UI
{
    public class ConsoleUI
    {
        private const int BoardStartX = 0;
        private const int BoardStartY = 3;

        private readonly object _lock = new object();
        private readonly IClient _client;

        private bool _immunity;

        public ConsoleUI(IClient client)
        {
            if (client == null)
                throw new ArgumentNullException("client");

            _client = client;
            _client.OnGameStarted += OnGameStarted;
            _client.OnGameFinished += OnGameFinished;
            _client.OnRedraw += OnRedraw;
            _client.OnRedrawBoard += OnRedrawBoard;
            _client.OnPieceMoving += OnPieceMoving;
            _client.OnPieceMoved += OnPieceMoved;
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
            _client.OnScoreChanged += OnScoreChanged;
            _client.OnSpecialUsed += OnSpecialUsed;
            _client.OnPlayerAddLines += OnPlayerAddLines;
            _client.OnContinuousEffectToggled += OnContinuousEffectToggled;
            _client.OnAchievementEarned += OnAchievementEarned;
            _client.OnPlayerAchievementEarned += OnPlayerAchievementEarned;

            Console.SetWindowSize(80, 30);
            Console.BufferWidth = 80;
            Console.BufferHeight = 30;
        }

        private void OnPlayerAchievementEarned(int playerId, string playerName, int achievementId, string achievementTitle)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
                Console.Write("{0} has earned [{1}]", playerName, achievementTitle);
            }
        }

        private void OnAchievementEarned(IAchievement achievement, bool firstTime)
        {
           lock (_lock)
           {
               Console.ResetColor();
               Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 0);
               Console.Write("You have earned [{0}]", achievement.Title);
           }
        }

        private void OnPlayerAddLines(int playerId, string playerName, int specialId, int count)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 8 + (specialId%10));
                Console.Write("{0}. {1} line{2} added to All from {3}", specialId, count, (count > 1) ? "s" : "", playerName );
            }
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 8 + (specialId%10));
                Console.Write("{0}. {1} on {2} from {3}", specialId, GetSpecialString(special), targetName, playerName);
            }
        }

        private void OnLevelChanged(int level)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 6);
                Console.Write("Level: {0}", level == 0 ? _client.Level : level);
            }
        }

        private void OnLinesClearedChanged(int linesCleared)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 5);
                Console.Write("#Lines cleared: {0}", linesCleared == 0 ? _client.LinesCleared : linesCleared);
            }
        }


        private void OnScoreChanged(int score)
        {
            lock (_lock)
            {
                Console.ResetColor();
                Console.SetCursorPosition(_client.Board.Width + 2 + BoardStartX, 7);
                Console.Write("Score: {0:#,0}", score == 0 ? _client.Score : score);
            }
        }

        private void OnRoundFinished(int deletedRows)
        {
            HideNextPieceColor();
        }

        private void OnRoundStarted()
        {
            DisplayNextPieceColor();
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

            _immunity = false;

            OnRedraw();
            DisplayNextPieceColor();
            OnLinesClearedChanged(0);
            OnLevelChanged(0);
            OnScoreChanged(0);
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

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
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
                    Console.Write("Registration failed!!! {0}", result);
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
                    Console.Write("{0}[{1}]:{2}", winList[i].PlayerName, winList[i].Team, winList[i].Score);
                }
            }
        }

        private void OnContinuousEffectToggled(Specials special, bool active, double durationLeftInSeconds)
        {
            if (special == Specials.Immunity)
            {
                _immunity = active;
                DisplayBoardColor();
            }
        }

        private void OnPieceMoved()
        {
            DisplayCurrentPieceColor();
        }

        private void OnPieceMoving()
        {
            HideCurrentPieceColor();
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            // NOP
        }

        private void OnRedraw()
        {
            // Board
            DisplayBoardColor();
            // Piece
            DisplayCurrentPieceColor();
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
                    Console.Write(_immunity ? "*" : "|");

                    for (int x = 1; x <= _client.Board.Width; x++)
                    {
                        Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                        byte cellValue = _client.Board[x, y];
                        if (cellValue == CellHelper.EmptyCell)
                            Console.Write(".");
                        else
                        {
                            Pieces cellPiece = CellHelper.GetColor(cellValue);
                            Console.BackgroundColor = GetPieceColor(cellPiece);
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
                    Console.Write(_immunity ? "*" : "|");
                }
                Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height + BoardStartY);
                Console.Write("".PadLeft(_client.Board.Width + 2, _immunity ? '*' : '-'));
            }
        }

        private void DisplayCurrentPieceColor()
        {
            lock (_lock)
            {
                // draw current piece
                if (_client.CurrentPiece != null)
                {
                    Pieces cellPiece = _client.CurrentPiece.Value;
                    Console.BackgroundColor = GetPieceColor(cellPiece);
                    for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
                    {
                        int x, y;
                        _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
                        if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
                        {
                            Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                            Console.Write(" ");
                        }
                    }
                }
            }
        }

        private void HideCurrentPieceColor()
        {
            lock (_lock)
            {
                // hide current piece
                if (_client.CurrentPiece != null)
                {
                    Console.ResetColor();
                    for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
                    {
                        int x, y;
                        _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
                        if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
                        {
                            Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
                            Console.Write(".");
                        }
                    }
                }
            }
        }

        private void DisplayNextPieceColor()
        {
            lock (_lock)
            {
                // draw next piece
                if (_client.NextPiece != null)
                {
                    IPiece temp = _client.NextPiece.Clone();
                    int minX, minY, maxX, maxY;
                    temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
                    // Move to top, left
                    temp.Translate(-minX, 0);
                    if (maxY > _client.Board.Height)
                        temp.Translate(0, _client.Board.Height - maxY);
                    // Display piece
                    Pieces cellPiece = temp.Value;
                    Console.BackgroundColor = GetPieceColor(cellPiece);
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

        private void HideNextPieceColor()
        {
            lock (_lock)
            {
                // hide next piece
                if (_client.NextPiece != null)
                {
                    Console.ResetColor();
                    IPiece temp = _client.NextPiece.Clone();
                    int minX, minY, maxX, maxY;
                    temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
                    // Move to top, left
                    temp.Translate(-minX, 0);
                    if (maxY > _client.Board.Height)
                        temp.Translate(0, _client.Board.Height - maxY);
                    // hide piece
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

        //private void DisplayBoardNoColor()
        //{
        //    lock (_lock)
        //    {
        //        for (int y = _client.Board.Height; y >= 1; y--)
        //        {
        //            StringBuilder sb = new StringBuilder("|");
        //            for (int x = 1; x <= _client.Board.Width; x++)
        //            {
        //                byte cellValue = _client.Board[x, y];
        //                if (cellValue == CellHelper.EmptyCell)
        //                    sb.Append(".");
        //                else {
        //                    Pieces cellPiece = CellHelper.GetColor(cellValue);
        //                    Specials cellSpecial = CellHelper.GetSpecial(cellValue);
        //                    if (cellSpecial == Specials.Invalid)
        //                        sb.Append((int) cellPiece);
        //                    else
        //                        sb.Append(ConvertSpecial(cellSpecial));
        //                }
        //            }
        //            sb.Append("|");
        //            Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height - y + BoardStartY);
        //            Console.Write(sb.ToString());
        //        }
        //        Console.SetCursorPosition(0 + BoardStartX, _client.Board.Height + BoardStartY);
        //        Console.Write("".PadLeft(_client.Board.Width + 2, '-'));
        //    }
        //}

        //private void DisplayCurrentPieceNoColor()
        //{
        //    lock (_lock)
        //    {
        //        // draw current piece
        //        if (_client.CurrentPiece != null)
        //        {
        //            for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
        //            {
        //                int x, y;
        //                _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
        //                if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
        //                {
        //                    Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
        //                    Console.Write(_client.CurrentPiece.Value);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void HideCurrentPieceNoColor()
        //{
        //    lock (_lock)
        //    {
        //        // draw current piece
        //        if (_client.CurrentPiece != null)
        //        {
        //            for (int i = 1; i <= _client.CurrentPiece.TotalCells; i++)
        //            {
        //                int x, y;
        //                _client.CurrentPiece.GetCellAbsolutePosition(i, out x, out y);
        //                if (x >= 0 && x <= _client.Board.Width && y >= 0 && y <= _client.Board.Height)
        //                {
        //                    Console.SetCursorPosition(x + BoardStartX, _client.Board.Height - y + BoardStartY);
        //                    Console.Write(".");
        //                }
        //            }
        //        }
        //    }
        //}

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

        private ConsoleColor GetPieceColor(Pieces piece)
        {
            switch (piece)
            {
                case Pieces.TetriminoI:
                    return ConsoleColor.Blue;
                case Pieces.TetriminoJ:
                    return ConsoleColor.Green;
                case Pieces.TetriminoL:
                    return ConsoleColor.Magenta;
                case Pieces.TetriminoO:
                    return ConsoleColor.Yellow;
                case Pieces.TetriminoS:
                    return ConsoleColor.Blue;
                case Pieces.TetriminoT:
                    return ConsoleColor.Yellow;
                case Pieces.TetriminoZ:
                    return ConsoleColor.Red;
            }
            return ConsoleColor.Gray;
        }

        private static char ConvertSpecial(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return attribute == null ? '?' : attribute.ShortName;
        }

        private static string GetSpecialString(Specials special)
        {
            SpecialAttribute attribute = EnumHelper.GetAttribute<SpecialAttribute>(special);
            return attribute == null ? special.ToString() : attribute.LongName;
        }
    }
}