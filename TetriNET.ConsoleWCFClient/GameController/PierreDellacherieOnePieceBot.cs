using System;
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
        }

        private void GetBestMove(IBoard board, ITetrimino piece, out int bestRotationDelta, out int bestTranslationDelta)
        {
            bestRotationDelta = 0;
            bestTranslationDelta = 0;

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
                        tempPiece.Translate(trialRotationDelta, 0);

                        // Check if move is acceptable
                        if (board.CheckNoConflict(tempPiece))
                        {
                            // Copy board
                            tempBoard.CopyFrom(board);
                            // Drop piece
                            tempBoard.DropAndCommit(tempPiece);

                            // Evaluate
                        }
                    }
                }
            }
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
        private void EvaluteMove(IBoard board, ITetrimino piece, out double rating, out int priority)
        {
            int pieceMinX = 0;
            int pieceMinY = 0;
            int pieceMaxX = 0;
            int pieceMaxY = 0;
            GetAbsoluteBoundingRectangle(piece, out pieceMinX, out pieceMinY, out pieceMaxX, out pieceMaxY);

            // Landing Height (vertical midpoint)
            double landingHeight = 0.0;
            landingHeight = 0.5 * (double)(pieceMinY + pieceMaxY);

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


            // TODO
        }

        private void GetAbsoluteBoundingRectangle(ITetrimino tetrimino, out int minX, out int minY, out int maxX, out int maxY)
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

        private int GetTotalCompletedRows(IBoard board) // result range: 0..Height
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
        private int CountPieceCellsEliminated( IBoard board, ITetrimino piece)
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
                        int boardX = 0;
                        int boardY = 0;
                        copyOfPiece.GetCellAbsolutePosition(cellIndex, out boardX, out boardY);
                        if (boardY == y)
                            pieceCellsEliminated++;  // Moohahahaaa!
                    }
                }
            }

            return pieceCellsEliminated;
        }

        private int GetPileMaxHeight(IBoard board) // result range: 0..Height
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
    }
}
