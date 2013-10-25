using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Strategy
{
    // http://www.colinfahey.com/tetris/tetris.html
    public class PierreDellacherieOnePiece : IMoveStrategy
    {
        private class Move
        {
            public int RotationDelta { get; set; }
            public int TranslationDelta { get; set; }
            public double Rating { get; set; }
            public int Priority { get; set; }
        }

        private class MoveManager
        {
            private readonly List<Move> _moves = new List<Move>();

            public bool Exists(int rotationDelta, int translationDelta)
            {
                return _moves.Any(x => x.RotationDelta == rotationDelta && x.TranslationDelta == translationDelta);
            }

            public void Add(int rotationDelta, int translationDelta, double rating, int priority)
            {
                _moves.Add(new Move
                {
                    RotationDelta = rotationDelta,
                    TranslationDelta = translationDelta,
                    Rating = rating,
                    Priority = priority
                });
            }
        }

        public bool GetBestMove(IBoard board, IPiece current, IPiece next, out int bestRotationDelta, out int bestTranslationDelta, out bool rotationBeforeTranslation)
        {
            int currentBestTranslationDelta = 0;
            int currentBestRotationDelta = 0;
            double currentBestRating = -1.0e+20; // Really bad!
            int currentBestPriority = 0;

            //if (current.PosY == board.Height) // TODO: put current totally in board before trying to get best move
            //    current.Translate(0, -1);

            IBoard tempBoard = board.Clone();
            IPiece tempPiece = current.Clone();

            MoveManager moveManager = new MoveManager();

            //Log.WriteLine(Log.LogLevels.Debug, "Get Best Move for Piece {0} {1}", tempPiece.Value, tempPiece.Index);

            #region Rotation then translation

            // Consider all possible rotations
            for (int trialRotationDelta = 0; trialRotationDelta < current.MaxOrientations; trialRotationDelta++)
            {
                // Copy piece
                tempPiece.CopyFrom(current);
                // Rotate
                tempPiece.Rotate(trialRotationDelta);

                // Get translation range
                bool isMovePossible;
                int minDeltaX;
                int maxDeltaX;
                BoardHelper.GetAccessibleTranslationsForOrientation(board, tempPiece, out isMovePossible, out minDeltaX, out maxDeltaX);

                //Log.WriteLine(Log.LogLevels.Debug, "Accessible translation {0} {1} {2} {3} {4}  {5} {6}", minDeltaX, maxDeltaX, trialRotationDelta, current.PosX, current.PosY, tempPiece.Value, tempPiece.Index);

                //StringBuilder sb = new StringBuilder();
                //for (int i = 1; i <= tempPiece.TotalCells; i++)
                //{
                //    int x, y;
                //    tempPiece.GetCellAbsolutePosition(i, out x, out y);
                //    sb.Append(String.Format("[{0}->{1},{2}]", i, x - tempPiece.PosX, y - tempPiece.PosY));
                //}
                //Log.Log.WriteLine("{0} {1} -> {2}  {3}", trialRotationDelta, minDeltaX, maxDeltaX, sb.ToString());
                if (isMovePossible)
                {
                    // Consider all allowed translations
                    for (int trialTranslationDelta = minDeltaX; trialTranslationDelta <= maxDeltaX; trialTranslationDelta++)
                    {
                        // Check if not already evaluated
                        bool found = moveManager.Exists(trialRotationDelta, trialTranslationDelta);

                        if (!found)
                        {
                            // Evaluate this move

                            // Copy piece
                            tempPiece.CopyFrom(current);
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

                                //Log.Log.WriteLine("R:{0:0.0000} P:{1} R:{2} T:{3}", trialRating, trialPriority, trialRotationDelta, trialTranslationDelta);

                                // Check if better than previous best
                                if (trialRating > currentBestRating || (Math.Abs(trialRating - currentBestRating) < 0.0001 && trialPriority > currentBestPriority))
                                {
                                    currentBestRating = trialRating;
                                    currentBestPriority = trialPriority;
                                    currentBestTranslationDelta = trialTranslationDelta;
                                    currentBestRotationDelta = trialRotationDelta;
                                }

                                moveManager.Add(trialRotationDelta, trialTranslationDelta, trialRating, trialPriority);
                            }
                        }
                    }
                }
            }
            //Log.WriteLine(Log.LogLevels.Debug, "ROTATION + TRANSLATION: {0} {1} {2:0.000} {3}", currentBestTranslationDelta, currentBestRotationDelta, currentBestRating, currentBestPriority);
            #endregion

            #region Translation then rotation
            bool isTrialMovePossible;
            int minTrialDeltaX;
            int maxTrialDeltaX;
            BoardHelper.GetAccessibleTranslationsForOrientation(board, tempPiece, out isTrialMovePossible, out minTrialDeltaX, out maxTrialDeltaX);
            if (isTrialMovePossible)
            {
                for (int trialTranslationDelta = minTrialDeltaX; trialTranslationDelta <= maxTrialDeltaX; trialTranslationDelta++)
                {
                    // Copy piece
                    tempPiece.CopyFrom(current);
                    // Translate
                    tempPiece.Translate(trialTranslationDelta, 0);

                    // Consider all rotations
                    for (int trialRotationDelta = 0; trialRotationDelta <= current.MaxOrientations; trialRotationDelta++)
                    {
                        // Rotate
                        tempPiece.Rotate(trialRotationDelta);

                        // Check if not already evaluated
                        bool found = moveManager.Exists(trialRotationDelta, trialTranslationDelta);

                        if (!found)
                        {
                            // Evaluate this move

                            // Copy piece
                            tempPiece.CopyFrom(current);
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

                                //Log.Log.WriteLine("R:{0:0.0000} P:{1} R:{2} T:{3}", trialRating, trialPriority, trialRotationDelta, trialTranslationDelta);

                                // Check if better than previous best
                                if (trialRating > currentBestRating || (Math.Abs(trialRating - currentBestRating) < 0.0001 && trialPriority > currentBestPriority))
                                {
                                    currentBestRating = trialRating;
                                    currentBestPriority = trialPriority;
                                    currentBestTranslationDelta = trialTranslationDelta;
                                    currentBestRotationDelta = trialRotationDelta;
                                    //Log.WriteLine(Log.LogLevels.Debug, "FOUND BETTER MOVE USING TRANSLATION + ROTATION");
                                }

                                moveManager.Add(trialRotationDelta, trialTranslationDelta, trialRating, trialPriority);
                            }
                        }
                    }
                }
            }
            //Log.WriteLine(Log.LogLevels.Debug, "TRANSLATION + ROTATION: {0} {1} {2:0.000} {3}", currentBestTranslationDelta, currentBestRotationDelta, currentBestRating, currentBestPriority);
            #endregion

            // Commit to this move
            rotationBeforeTranslation = true;
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta = currentBestRotationDelta;

            // Log.WriteLine(Log.LogLevels.Debug, "{0} {1} {2:0.000} {3}", bestRotationDelta, bestTranslationDelta, currentBestRating, currentBestPriority);

            return true;
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
        private static void EvaluteMove(IBoard board, IPiece piece, out double rating, out int priority)
        {
            int pieceMinX;
            int pieceMinY;
            int pieceMaxX;
            int pieceMaxY;
            piece.GetAbsoluteBoundingRectangle(out pieceMinX, out pieceMinY, out pieceMaxX, out pieceMaxY);

            // Landing Height (vertical midpoint)
            double landingHeight = 0.5 * (pieceMinY + pieceMaxY);

            //
            int completedRows = BoardHelper.GetTotalCompletedRows(board);
            int erodedPieceCellsMetric = 0;
            if (completedRows > 0)
            {
                // Count piece cells eroded by completed rows before doing collapse on pile.
                int countPieceCellsEliminated = BoardHelper.CountPieceCellsEliminated(board, piece, true);

                // Now it's okay to collapse completed rows
                board.CollapseCompletedRows();

                // Weight eroded cells by completed rows
                erodedPieceCellsMetric = (completedRows * countPieceCellsEliminated);
            }

            //
            int pileHeight = BoardHelper.GetPileMaxHeight(board);

            // Each empty row (above pile height) has two (2) "transitions"
            // (We could call ref_Board.GetTransitionCountForRow( y ) for
            // these unoccupied rows, but this is an optimization.)
            int boardRowTransitions = 2 * (board.Height - pileHeight);

            // Only go up to the pile height, and later we'll account for the
            // remaining rows transitions (2 per empty row).
            for (int y = 1; y <= pileHeight; y++)
                boardRowTransitions += BoardHelper.GetTransitionCountForRow(board, y);

            //
            int boardColumnTransitions = 0;
            int boardBuriedHoles = 0;
            int boardWells = 0;
            for (int x = 1; x <= board.Width; x++)
            {
                boardColumnTransitions += BoardHelper.GetTransitionCountForColumn(board, x);
                boardBuriedHoles += BoardHelper.GetBuriedHolesForColumn(board, x);
                boardWells += BoardHelper.GetAllWellsForColumn(board, x);
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

            int absoluteDistanceX = Math.Abs(piece.PosX - board.PieceSpawnX);

            priority = 0;
            priority += (100 * absoluteDistanceX);
            if (piece.PosX < board.PieceSpawnX)
                priority += 10;
            priority -= piece.Orientation - 1;
        }
    }
}
