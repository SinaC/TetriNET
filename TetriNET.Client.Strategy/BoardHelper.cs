using TetriNET.Client.Interfaces;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Strategy
{
    public static class BoardHelper
    {
        public static void GetAccessibleTranslationsForOrientation(IBoard board, IPiece piece, out bool isMovePossible, out int minDeltaX, out int maxDeltaX)
        {
            isMovePossible = false;
            minDeltaX = 0;
            maxDeltaX = 0;

            IPiece tempPiece = piece.Clone();

            // Check if we can move
            bool moveAcceptable = board.CheckNoConflict(tempPiece);
            if (!moveAcceptable)
                return;
            isMovePossible = true;

            // Scan from center to left to find left limit.
            for (int trial = 0; trial >= -board.Width; trial--)
            {
                // Copy piece
                tempPiece.CopyFrom(piece);
                // Translate
                tempPiece.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = board.CheckNoConflict(tempPiece);
                if (moveAcceptable)
                    minDeltaX = trial;
                else
                    break;
            }

            // Scan from center to right to find right limit.
            for (int trial = 0; trial <= board.Width; trial++)
            {
                // Copy piece
                tempPiece.CopyFrom(piece);
                // Translate
                tempPiece.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = board.CheckNoConflict(tempPiece);
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

        public static int GetTotalCompletedRows(IBoard board) // result range: 0..Height
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
        public static int CountPieceCellsEliminated(IBoard board, IPiece piece, bool alreadyDropped)
        {
            // Copy piece and board so that this measurement is not destructive.
            IBoard copyOfBoard = alreadyDropped ? board : board.Clone();
            IPiece copyOfPiece = alreadyDropped ? piece : piece.Clone();

            if (!alreadyDropped)
                // Drop copy of piece on to the copy of the board
                copyOfBoard.DropAndCommit(copyOfPiece);

            // Scan rows.  For each full row, check all board Y values for the
            // piece.  If any board Y of the piece matches the full row Y,
            // increment the total eliminated cells.
            int countPieceCellsEliminated = 0;
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
                            countPieceCellsEliminated++;  // Moohahahaaa!
                    }
                }
            }

            return countPieceCellsEliminated;
        }

        // Number of full to empty or empty to full cell transitions
        public static int GetTransitionCountForRow(IBoard board, int y) // result range: 0..Width
        {
            int transitionCount = 0;

            // check cell and neighbor to right...
            for (int x = 1; x < board.Width; x++)
            {
                byte cellA = board[x, y];
                byte cellB = board[x + 1, y];

                // If a transition from occupied to unoccupied, or
                // from unoccupied to occupied, then it's a transition.
                if ((cellA != 0 && cellB == 0) || (cellA == 0 && cellB != 0))
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

        // Number of full to empty or empty to full cell transitions
        public static int GetTransitionCountForColumn(IBoard board, int x) // result range: 1..(Height + 1)
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

        public static int GetBuriedHolesForColumn(IBoard board, int x) // result range: 0..(Height-1)
        {
            int totalHoles = 0;
            bool enable = false;

            for (int y = board.Height; y >= 1; y--)
            {
                byte cellValue = board[x, y];

                if (cellValue != CellHelper.EmptyCell)
                    enable = true;
                else if (enable)
                    totalHoles++;
            }

            return totalHoles;
        }

        public static int GetAllWellsForColumn(IBoard board, int x) // result range: 0..O(Height*Height)
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

        // Number of full cells in the column above each hole
        public static int GetHoleDepthForColumn(IBoard board, int x)
        {
            int count = 0;
            bool enable = false;
            for (int y = 1; y <= board.Height; y++)
            {
                if (board[x, y] == 0)
                    enable = true;
                else if (enable)
                    count++;
            }
            return count;
        }

        // Number of block that’s directly above a hole
        public static int GetBlockadesForColumn(IBoard board, int x)
        {
            int count = 0;
            //for (int y = 1; y <= board.Height; y++)
            //{
            //    if (board[x, y] == 0)
            //    {
            //        while (board[x, y] == 0 && y <= board.Height)
            //            y++;
            //        if (y > board.Height)
            //            break;
            //        while (board[x, y] > 0 && y <= board.Height)
            //        {
            //            y++;
            //            count++;
            //        }
            //        if (y > board.Height)
            //            break;
            //        y--;
            //    }
            //}
            bool enable = false;
            for (int y = 1; y <= board.Height; y++)
            {
                if (board[x, y] == 0)
                    enable = true;
                else if (enable)
                    count++;
            }
            return count;
        }

        public static int GetTotalCellHeight(IBoard board)
        {
            int total = 0;
            for (int y = 1; y <= board.Height; y++)
                for (int x = 1; x <= board.Width; x++)
                    if (board[x, y] != 0)
                        total += y;
            return total;
        }

        public static int GetTotalShadowedHoles(IBoard board)
        {
            int totalShadowedHoles = 0;

            // For each column we search top-down through the rows,
            // noting the first occupied cell, and counting any
            // unoccupied cells after that point...
            for (int x = 1; x <= board.Width; x++)
                totalShadowedHoles += GetBuriedHolesForColumn(board, x);

            return totalShadowedHoles;
        }

        public static int GetPileHeightWeightedCells(IBoard board)
        {
            int totalWeightedCells = 0;

            for (int y = 1; y <= board.Height; y++)
                for (int x = 1; x <= board.Width; x++)
                    if (board[x, y] != CellHelper.EmptyCell)
                        totalWeightedCells += y;

            return totalWeightedCells;
        }

        public static int GetSumOfWellHeights(IBoard board)
        {
            int sumOfWellHeights = 0;
            int columnHeight = 0;
            int columnHeightToLeft = 0;
            int columnHeightToRight = 0;

            // Determine height of LEFT  well
            columnHeight = GetColumnHeight(board, 1);
            columnHeightToRight = GetColumnHeight(board, 2);
            if (columnHeightToRight > columnHeight)
                sumOfWellHeights += (columnHeightToRight - columnHeight);

            // Determine height of RIGHT well
            columnHeightToLeft = GetColumnHeight(board, board.Width - 1);
            columnHeight = GetColumnHeight(board, board.Width);
            if (columnHeightToLeft > columnHeight)
                sumOfWellHeights += (columnHeightToLeft - columnHeight);

            // Now do all interior columns, 1..(width-1), which have TWO adjacent lines...
            for (int x = 2; x <= board.Width - 1; x++)
            {
                columnHeightToLeft = GetColumnHeight(board, x - 1);
                columnHeight = GetColumnHeight(board, x);
                columnHeightToRight = GetColumnHeight(board, x + 1);

                if (columnHeightToLeft > columnHeightToRight)
                    columnHeightToLeft = columnHeightToRight;
                else
                    columnHeightToRight = columnHeightToLeft;

                if (columnHeightToLeft > columnHeight)
                    sumOfWellHeights += (columnHeightToLeft - columnHeight);
            }

            return sumOfWellHeights;
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

        private static int GetColumnHeight(IBoard board, int x)
        {
            if (x <= 0)
                return 0;
            if (x > board.Width)
                return 0;

            // top-down search for first occupied cell
            for (int y = board.Height; y >= 1; y--) // top-down search
                if (board[x, y] != CellHelper.EmptyCell)
                    return y;
            return 0;
        }
    }
}
