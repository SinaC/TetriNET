using System.Linq;
using TetriNET.Client.Interfaces;

namespace TetriNET.Client.Strategy
{
    public class LuckyToiletOnePiece : IMoveStrategy
    {
        public bool GetBestMove(IBoard board, IPiece current, IPiece next, out int bestRotationDelta, out int bestTranslationDelta, out bool rotationBeforeTranslation)
        {
            int currentBestTranslationDelta = 0;
            int currentBestRotationDelta = 0;
            double currentBestRating = -1.0e+20; // Really bad!

            //if (current.PosY == board.Height) // TODO: put current totally in board before trying to get best move
            //    current.Translate(0, -1);

            IBoard tempBoard = board.Clone();
            IPiece tempPiece = current.Clone();

            //Log.WriteLine(Log.LogLevels.Debug, "Get Best Move for Piece {0} {1}", tempPiece.Value, tempPiece.Index);

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
                            double trialRating = EvaluteMove(tempBoard, tempPiece);

                            //Log.Log.WriteLine("R:{0:0.0000} P:{1} R:{2} T:{3}", trialRating, trialRotationDelta, trialTranslationDelta);

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
            rotationBeforeTranslation = true;
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta = currentBestRotationDelta;

            // Log.WriteLine(Log.LogLevels.Debug, "{0} {1} {2:0.000}", bestRotationDelta, bestTranslationDelta, currentBestRating);

            return true;
        }

        private static double EvaluteMove(IBoard board, IPiece piece)
        {
            int pieceMinX;
            int pieceMinY;
            int pieceMaxX;
            int pieceMaxY;
            piece.GetAbsoluteBoundingRectangle(out pieceMinX, out pieceMinY, out pieceMaxX, out pieceMaxY);

            int totalHeight = BoardHelper.GetTotalCellHeight(board);
            int completedRows = BoardHelper.GetTotalCompletedRows(board);
            int totalHoles = Enumerable.Range(1, board.Width).Aggregate(0, (i, i1) => i + BoardHelper.GetBuriedHolesForColumn(board, i1));
            int totalBlockades = Enumerable.Range(1, board.Width).Aggregate(0, (i, i1) => i + BoardHelper.GetBlockadesForColumn(board, i1));
            int edgeTouchingAnotherBlock = 0; // TODO
            int edgeTouchingWall = 0; // TODO
            int edgeTouchingFloor = 0;

            //double rating = 0;
            //rating += -0.03 * totalHeight;
            //rating += -7.5 * totalHoles;
            //rating += -3.5 * totalBlockades;
            //rating += 8.0 * completedRows;
            //rating += 3.0 * edgeTouchingAnotherBlock;
            //rating += 2.5 * edgeTouchingWall;
            //rating += 5.0 * edgeTouchingFloor;
            //double rating = 0;
            //rating += -3.78 * totalHeight;
            //rating += -2.31 * totalHoles;
            //rating += -0.59 * totalBlockades;
            //rating += 1.6 * completedRows;
            //rating += 3.97 * edgeTouchingAnotherBlock;
            //rating += 6.52 * edgeTouchingWall;
            //rating += 0.65 * edgeTouchingFloor;
            double rating = 0;
            rating += -0.868099 * totalHeight;
            rating += -2.45402 * totalHoles;
            rating += -0.236702 * totalBlockades;
            rating += 3.59764 * completedRows;
            rating += 5.33378 * edgeTouchingAnotherBlock;
            rating += 8.20521 * edgeTouchingWall;
            rating += 0.00 * edgeTouchingFloor;

            return rating;
        }
    }
}
