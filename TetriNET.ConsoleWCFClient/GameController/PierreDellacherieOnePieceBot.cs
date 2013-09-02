using System;
using System.Text;
using System.Timers;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.GameController
{
    public class PierreDellacherieOnePieceBot
    {
        private readonly IClient _client;
        private readonly Timer _timer;

        public PierreDellacherieOnePieceBot(IClient client)
        {
            _client = client;

            _timer = new Timer(100);
            _timer.Elapsed += _timer_Elapsed;

            _client.OnTetriminoPlaced += _client_OnTetriminoPlaced;
            _client.OnGameStarted += client_OnGameStarted;
            _client.OnGameFinished += _client_OnGameFinished;
            _client.OnGameOver += _client_OnGameOver;
            _client.OnGamePaused += _client_OnGamePaused;
            _client.OnGameResumed += _client_OnGameResumed;
        }

        private void _client_OnTetriminoPlaced()
        {
            _timer.Start();
        }

        private void client_OnGameStarted()
        {
            _timer.Start();    
        }

        private void _client_OnGameFinished()
        {
            _timer.Stop();
        }

        private void _client_OnGameOver()
        {
            _timer.Stop();
        }

        private void _client_OnGamePaused()
        {
            _timer.Stop();
        }
        
        private void _client_OnGameResumed()
        {
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            // Get best move
            int bestRotationDelta;
            int bestTranslationDelta;
            GetBestMove(_client.Board, _client.CurrentTetrimino, out bestRotationDelta, out bestTranslationDelta);

            // Perform move

            // TODO: animate move, fill a queue with movement to do and pop a movement every x ms

            // ROTATE
            for (int rotateCount = 0; rotateCount < bestRotationDelta; rotateCount++)
                _client.RotateClockwise();

            // TRANSLATE
            if (bestTranslationDelta < 0)
                for (int translateCount = 0; translateCount > bestTranslationDelta; translateCount--)
                    _client.MoveLeft();
            if (bestTranslationDelta > 0)
                for (int translateCount = 0; translateCount < bestTranslationDelta; translateCount++)
                    _client.MoveRight();

            // DROP
            System.Threading.Thread.Sleep(100); // delay drop
            _client.Drop();
        }

        private void GetBestMove(IBoard board, ITetrimino piece, out int bestRotationDelta, out int bestTranslationDelta)
        {
            int currentBestTranslationDelta = 0;
            int currentBestRotationDelta = 0;
            double currentBestRating = -1.0e+20; // Really bad!
            int currentBestPriority = 0;

            IBoard tempBoard = board.Clone();
            ITetrimino tempPiece = piece.Clone();

            // Consider all possible rotations
            for (int trialRotationDelta = 0; trialRotationDelta < piece.MaxOrientations; trialRotationDelta++)
            {
                // Copy piece
                tempPiece.CopyFrom(piece);
                // Rotate
                tempPiece.Rotate(trialRotationDelta);

                // Get translation range
                bool isMovePossible;
                int minDeltaX;
                int maxDeltaX;
                board.GetAccessibleTranslationsForOrientation(tempPiece, out isMovePossible, out minDeltaX, out maxDeltaX);

                StringBuilder sb = new StringBuilder();
                for (int i = 1; i <= tempPiece.TotalCells; i++)
                {
                    int x, y;
                    tempPiece.GetCellAbsolutePosition(i, out x, out y);
                    sb.Append(String.Format("[{0}->{1},{2}]", i, x-tempPiece.PosX, y-tempPiece.PosY));
                }
                //Log.WriteLine("{0} {1} -> {2}  {3}", trialRotationDelta, minDeltaX, maxDeltaX, sb.ToString());
                if (isMovePossible)
                {
                    // Consider all allowed translations
                    for (int trialTranslationDelta = minDeltaX; trialTranslationDelta <= maxDeltaX; trialTranslationDelta++)
                    {
                        // Evaluate this move

                        // Copy piece
                        tempPiece.CopyFrom(piece);
                        // Rotate
                        tempPiece.Rotate(trialRotationDelta);
                        // Translate
                        tempPiece.Translate(trialTranslationDelta, 0);

                        // Check if move is acceptable
                        if (board.CheckNoConflict(tempPiece))
                        {
                            // Copy board
                            tempBoard.CopyFrom(board);
                            // Drop piece
                            tempBoard.DropAndCommit(tempPiece);

                            // Evaluate
                            double trialRating;
                            int trialPriority;
                            EvaluteMove(tempBoard, tempPiece, out trialRating, out trialPriority);

                            //Log.WriteLine("R:{0:0.0000} P:{1} R:{2} T:{3}", trialRating, trialPriority, trialRotationDelta, trialTranslationDelta);

                            // Check if better than previous best
                            if (trialRating > currentBestRating || (Math.Abs(trialRating - currentBestRating) < 0.0001 && trialPriority > currentBestPriority))
                            {
                                currentBestRating = trialRating;
                                currentBestPriority = trialPriority;
                                currentBestTranslationDelta = trialTranslationDelta;
                                currentBestRotationDelta = trialRotationDelta;
                            }
                        }
                    }
                }
            }

            // commit to this move
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta = currentBestRotationDelta;

            //Console.SetCursorPosition(0, _client.Board.Height+1);
           // Console.WriteLine("{0} {1} {2:0.000} {3}", bestRotationDelta, bestTranslationDelta, currentBestRating, currentBestPriority);
        }

        // The following evaluation function was adapted from Pascal code submitted by:
        // Pierre Dellacherie (France).  (E-mail : dellache@club-internet.fr)
        //
        // This amazing one-piece algorithm completes an average of roughly 600 000 
        // rows, and often attains 2 000 000 or 2 500 000 rows.  However, the algorithm
        // sometimes completes as few as 15 000 rows.  I am fairly certain that this
        // is NOT due to statistically abnormal patterns in the falling piece sequence.
        //
        // Pierre Dellacherie corresponded with me via e-mail to help me with the 
        // conversion of his Pascal code to C++.
        //
        // WARNING:
        //     If there is a single board and piece combination with the highest
        //     'rating' value, it is the best combination.  However, among
        //     board and piece combinations with EQUAL 'rating' values,
        //     the highest 'priority' value wins.
        //
        //     So, the complete rating is: { rating, priority }.
        private static void EvaluteMove(IBoard board, ITetrimino piece, out double rating, out int priority)
        {
            int pieceMinX;
            int pieceMinY;
            int pieceMaxX;
            int pieceMaxY;
            GetAbsoluteBoundingRectangle(piece, out pieceMinX, out pieceMinY, out pieceMaxX, out pieceMaxY);

            // Landing Height (vertical midpoint)
            double landingHeight = 0.5 * (pieceMinY + pieceMaxY);

            //
            int completedRows = GetTotalCompletedRows(board);
            int erodedPieceCellsMetric = 0;
            if (completedRows > 0)
            {
                // Count piece cells eroded by completed rows before doing collapse on pile.
                int pieceCellsEliminated = CountPieceCellsEliminated(board, piece);

                // Now it's okay to collapse completed rows
                board.CollapseCompletedRows();

                // Weight eroded cells by completed rows
                erodedPieceCellsMetric = (completedRows * pieceCellsEliminated);
            }

            //
            int pileHeight = GetPileMaxHeight(board);

            // Each empty row (above pile height) has two (2) "transitions"
            // (We could call ref_Board.GetTransitionCountForRow( y ) for
            // these unoccupied rows, but this is an optimization.)
            int boardRowTransitions = 2 * (board.Height - pileHeight);

            // Only go up to the pile height, and later we'll account for the
            // remaining rows transitions (2 per empty row).
            for (int y = 1; y <= pileHeight; y++)
                boardRowTransitions += GetTransitionCountForRow(board, y);

            //
            int boardColumnTransitions = 0;
            int boardBuriedHoles = 0;
            int boardWells = 0;
            for (int x = 1; x <= board.Width; x++)
            {
                boardColumnTransitions += GetTransitionCountForColumn(board, x);
                boardBuriedHoles += GetBuriedHolesForColumn(board, x);
                boardWells += GetAllWellsForColumn(board, x);
            }

            // Final rating
            //   [1] Punish landing height
            //   [2] Reward eroded piece cells
            //   [3] Punish row    transitions
            //   [4] Punish column transitions
            //   [5] Punish buried holes (cellars)
            //   [6] Punish wells

            rating = 0.0;
            rating += -1.0 * landingHeight;
            rating += 1.0 * erodedPieceCellsMetric;
            rating += -1.0 * boardRowTransitions;
            rating += -1.0 * boardColumnTransitions;
            rating += -4.0 * boardBuriedHoles;
            rating += -1.0 * boardWells;

            // PRIORITY:  
            //   Priority is further differentiation between possible moves.
            //   We further rate moves accoding to the following:
            //            * Reward deviation from center of board
            //            * Reward pieces to the left of center of the board
            //            * Punish rotation
            //   Priority is less important than the rating, but among equal
            //   ratings we select the option with the greatest priority.
            //   In principle we could simply factor priority in to the rating,
            //   as long as the priority was less significant than the smallest
            //   variations in rating, but for large board widths (>100), the
            //   risk of loss of precision in the lowest bits of the rating
            //   is too much to tolerate.  So, this priority is stored in a
            //   separate variable.

            int absoluteDistanceX = Math.Abs(piece.PosX - board.TetriminoSpawnX);

            priority = 0;
            priority += (100 * absoluteDistanceX);
            if (piece.PosX < board.TetriminoSpawnX)
                priority += 10;
            priority -= piece.Orientation - 1;
        }

        private static void GetAbsoluteBoundingRectangle(ITetrimino tetrimino, out int minX, out int minY, out int maxX, out int maxY)
        {
            minX = 0;
            minY = 0;
            maxX = 0;
            maxY = 0;

            if (tetrimino.TotalCells < 1) return;

            int x;
            int y;

            // start bounding limits using first cell
            tetrimino.GetCellAbsolutePosition(1, out x, out y); // first cell
            minX = x;
            maxX = x;
            minY = y;
            maxY = y;

            // expand bounding limits with other cells
            for (int cellIndex = 2; cellIndex <= tetrimino.TotalCells; cellIndex++)
            {
                tetrimino.GetCellAbsolutePosition(cellIndex, out x, out y);
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }

        private static int GetTotalCompletedRows(IBoard board) // result range: 0..Height
        {

            int totalCompletedRows = 0;

            // check each row
            for (int y = 1; y <= board.Height; y++)
            {
                // check if this row is full.
                bool rowIsFull = true; // hypothesis
                for (int x = 1; x <= board.Width; x++)
                {
                    byte cellValue = board[x, y];
                    if (cellValue == 0)
                    {
                        rowIsFull = false;
                        break;
                    }
                }

                if (rowIsFull)
                    totalCompletedRows++;
            }

            return totalCompletedRows;
        }

        // The following counts the number of cells (0..4) of a piece that would
        // be eliminated by dropping the piece.
        private static int CountPieceCellsEliminated(IBoard board, ITetrimino piece)
        {
            // Copy piece and board so that this measurement is not destructive.
            IBoard copyOfBoard = board.Clone();
            ITetrimino copyOfPiece = piece.Clone();

            // Drop copy of piece on to the copy of the board
            copyOfBoard.DropAndCommit(copyOfPiece);

            // Scan rows.  For each full row, check all board Y values for the
            // piece.  If any board Y of the piece matches the full row Y,
            // increment the total eliminated cells.
            int pieceCellsEliminated = 0;
            for (int y = 1; y <= copyOfBoard.Height; y++)
            {

                bool fullRow = true; // hypothesis
                for (int x = 1; x <= copyOfBoard.Width; x++)
                {
                    byte cellValue = copyOfBoard[x, y];
                    if (cellValue == 0)
                    {
                        fullRow = false;
                        break;
                    }
                }

                if (fullRow)
                {
                    // Find any matching board-relative Y values in dropped copy of piece.
                    for (int cellIndex = 1; cellIndex <= piece.TotalCells; cellIndex++)
                    {
                        int boardX;
                        int boardY;
                        copyOfPiece.GetCellAbsolutePosition(cellIndex, out boardX, out boardY);
                        if (boardY == y)
                            pieceCellsEliminated++;  // Moohahahaaa!
                    }
                }
            }

            return pieceCellsEliminated;
        }

        private static int GetPileMaxHeight(IBoard board) // result range: 0..Height
        {
            // top-down search for non-empty cell
            for (int y = board.Height; y >= 1; y--)
            {
                for (int x = 1; x <= board.Width; x++)
                {
                    byte cellValue = board[x, y];
                    if (0 != cellValue)
                        return y;
                }
            }
            return 0; // entire board is empty
        }

        private static int GetTransitionCountForRow(IBoard board, int y) // result range: 0..Width
        {
            int transitionCount = 0;

            // check cell and neighbor to right...
            for (int x = 1; x < board.Width; x++)
            {
                byte cellA = board[x, y];
                byte cellB = board[x + 1, y];

                // If a transition from occupied to unoccupied, or
                // from unoccupied to occupied, then it's a transition.
                if ( (cellA != 0 && cellB == 0) || (cellA == 0 && cellB != 0) )
                    transitionCount++;
            }

            // check transition between left-exterior and column 1.
            // (Note: exterior is implicitly "occupied".)
            byte cellLeft = board[1, y];
            if (cellLeft == 0)
                transitionCount++;

            // check transition between column 'mWidth' and right-exterior.
            // (NOTE: Exterior is implicitly "occupied".)
            byte cellRight = board[board.Width, y];
            if (cellRight == 0)
                transitionCount++;

            return transitionCount;
        }

        private static int GetTransitionCountForColumn(IBoard board, int x) // result range: 1..(Height + 1)
        {
            int transitionCount = 0;

            // check cell and neighbor above...
            for (int y = 1; y < board.Height; y++)
            {
                byte cellA = board[x, y];
                byte cellB = board[x, y + 1];

                // If a transition from occupied to unoccupied, or
                // from unoccupied to occupied, then it's a transition.
                if ((cellA != 0 && cellB == 0) || (cellA == 0 && cellB != 0))
                    transitionCount++;
            }

            // check transition between bottom-exterior and row Y=1.
            // (Note: Bottom exterior is implicitly "occupied".)
            int cellBottom = board[x, 1];
            if (cellBottom == 0)
                transitionCount++;

            // check transition between column 'mHeight' and above-exterior.
            // (Note: Sky above is implicitly UN-"occupied".)
            int cellTop = board[x, board.Height];
            if (cellTop != 0)
                transitionCount++;

            return transitionCount;
        }

        private static int GetBuriedHolesForColumn(IBoard board, int x) // result range: 0..(Height-1)
        {
            int totalHoles = 0;
            bool enable = false;

            for (int y = board.Height; y >= 1; y--)
            {
                byte cellValue = board[x, y];

                if (cellValue != 0)
                    enable = true;
                else if (enable)
                    totalHoles++;
            }

            return totalHoles;
        }

        private static int GetAllWellsForColumn(IBoard board, int x) // result range: 0..O(Height*mHeight)
        {
            int wellValue = 0;

            for (int y = board.Height; y >= 1; y--)
            {
                byte cellLeft;
                if (x - 1 >= 1)
                    cellLeft = board[x - 1, y];
                else
                    cellLeft = 1;

                byte cellRight;
                if (x + 1 <= board.Width)
                    cellRight = board[x + 1, y];
                else
                    cellRight = 1;

                if (cellLeft != 0 && cellRight != 0)
                {
                    int blanksDown = GetBlanksDownBeforeBlockedForColumn(board, x, y);
                    wellValue += blanksDown;
                }
            }

            return wellValue;
        }

        private static int GetBlanksDownBeforeBlockedForColumn(IBoard board, int x, int topY) // result range: 0..topY
        {
            int totalBlanksBeforeBlocked = 0;
            for (int y = topY; y >= 1; y--)
            {
                byte cellValue = board[x, y];

                if (cellValue != 0)
                    return totalBlanksBeforeBlocked;
                totalBlanksBeforeBlocked++;
            }

            return totalBlanksBeforeBlocked;
        }
    }
}
