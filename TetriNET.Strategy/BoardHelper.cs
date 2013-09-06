using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public static class BoardHelper
    {
        public static void GetAccessibleTranslationsForOrientation(IBoard board, ITetrimino tetrimino, out bool isMovePossible, out int minDeltaX, out int maxDeltaX)
        {
            isMovePossible = false;
            minDeltaX = 0;
            maxDeltaX = 0;

            ITetrimino tempTetrimino = tetrimino.Clone();

            // Check if we can move
            bool moveAcceptable = board.CheckNoConflict(tempTetrimino);
            if (!moveAcceptable)
                return;
            isMovePossible = true;

            // Scan from center to left to find left limit.
            for (int trial = 0; trial >= -board.Width; trial--)
            {
                // Copy tetrimino
                tempTetrimino.CopyFrom(tetrimino);
                // Translate
                tempTetrimino.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = board.CheckNoConflict(tempTetrimino);
                if (moveAcceptable)
                    minDeltaX = trial;
                else
                    break;
            }

            // Scan from center to right to find right limit.
            for (int trial = 0; trial <= board.Width; trial++)
            {
                // Copy tetrimino
                tempTetrimino.CopyFrom(tetrimino);
                // Translate
                tempTetrimino.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = board.CheckNoConflict(tempTetrimino);
                if (moveAcceptable)
                    maxDeltaX = trial;
                else
                    break;
            }
        }

        public static int GetPileMaxHeight(IBoard board) // result range: 0..Height
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
