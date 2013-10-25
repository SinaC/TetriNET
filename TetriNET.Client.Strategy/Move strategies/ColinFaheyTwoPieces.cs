using System.Collections.Generic;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Strategy
{
    public class ColinFaheyTwoPieces : IMoveStrategy
    {
        public bool GetBestMove(IBoard board, IPiece current, IPiece next, out int bestRotationDelta, out int bestTranslationDelta, out bool rotationBeforeTranslation)
        {
            int currentBestTranslationDelta = 0;
            int currentBestRotationDelta = 0;
            double currentBestRating = -1.0e+20; // Really bad!

            //current.Translate(0, -1);

            IBoard tempBoard = board.Clone();
            IPiece tempPiece = current.Clone();

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

                            // Do second move with next piece
                            IPiece tempNext = next.Clone();

                            // Evaluate
                            int nextPieceBestRotation;
                            int nextPieceBestTranslation;
                            double trialRating = EvaluteMove(tempBoard, tempNext, out nextPieceBestRotation, out nextPieceBestTranslation);

                            //Log.Log.WriteLine("R:{0:0.0000} P:{1} R:{2} T:{3}", trialRating, trialPriority, trialRotationDelta, trialTranslationDelta);

                            // Check if better than previous best
                            if (trialRating > currentBestRating)
                            {
                                currentBestRating = trialRating;
                                currentBestTranslationDelta = trialTranslationDelta;
                                currentBestRotationDelta = trialRotationDelta;
                            }
                        }
                    }
                }
            }

            // commit to this move
            rotationBeforeTranslation = true;
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta = currentBestRotationDelta;

            //Console.SetCursorPosition(0, _client.Board.Height+1);
            // Console.WriteLine("{0} {1} {2:0.000} {3}", bestRotationDelta, bestTranslationDelta, currentBestRating, currentBestPriority);

            return true;
        }

        private double EvaluteMove(IBoard board, IPiece piece, out int bestRotationDelta, out int bestTranslationDelta)
        {
            int currentBestTranslationDelta = 0;
            int currentBestRotationDelta = 0;
            double currentBestRating = -1.0e+20; // Really bad!
            int currentBestPriority = 0;

            //current.Translate(0, -1);

            IBoard tempBoard = board.Clone();
            IPiece tempPiece = piece.Clone();

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
                BoardHelper.GetAccessibleTranslationsForOrientation(board, tempPiece, out isMovePossible, out minDeltaX, out maxDeltaX);

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

                            tempBoard.CollapseCompletedRows();

                            double trialRating = 0;
                            trialRating += -0.65*BoardHelper.GetTotalShadowedHoles(board);
                            trialRating += -0.10*BoardHelper.GetPileHeightWeightedCells(board);
                            trialRating += -0.20*BoardHelper.GetSumOfWellHeights(board);

                            // Check if better than previous best
                            if (trialRating > currentBestRating)
                            {
                                currentBestRating = trialRating;
                                currentBestTranslationDelta = trialTranslationDelta;
                                currentBestRotationDelta = trialRotationDelta;
                            }
                        }
                    }
                }
            }
            // Commit to this move
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta = currentBestRotationDelta;

            return currentBestRating;
        }
    }
}
