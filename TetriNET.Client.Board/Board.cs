using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Board
{
    public class Board : IBoard
    {
        private readonly Random _random; // TODO: create own static thread-safe random class

        public int Width { get; private set; }
        public int Height { get; private set; }
        public byte[] Cells { get; private set; }

        public Board(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new byte[Width*Height];

            _random = new Random();
        }

        public IBoard Clone()
        {
            Board board = new Board(Width, Height);
            Cells.CopyTo(board.Cells, 0);
            return board;
        }

        public bool CopyFrom(IBoard board)
        {
            if (Width != board.Width || Height != board.Height)
                return false;
            SetCells(board.Cells);
            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < Cells.Length; i++)
                Cells[i] = CellHelper.EmptyCell;
        }

        public void FillWithRandomCells(Func<Pieces> randomFunc)
        {
            for (int i = 0; i < Width*Height; i++)
                Cells[i] = CellHelper.SetColor(randomFunc());
        }

        public bool SetCells(byte[] cells)
        {
            if (cells.Length != Width*Height)
                return false;
            cells.CopyTo(Cells, 0);
            return true;
        }

        public int TotalCells
        {
            get { return Width*Height; }
        }

        public byte this[int x, int y]
        {
            get
            {
                if (x <= 0)
                    return CellHelper.EmptyCell;
                if (x > Width)
                    return CellHelper.EmptyCell;
                if (y <= 0)
                    return CellHelper.EmptyCell;
                if (y > Height)
                    return CellHelper.EmptyCell;
                int index = GetCellIndex(x, y);
                return Cells[index];
            }
            protected set
            {
                if (x <= 0)
                    return;
                if (x > Width)
                    return;
                if (y <= 0)
                    return;
                if (y > Height)
                    return;
                int index = GetCellIndex(x, y);
                Cells[index] = value;
            }
        }

        public int GetCellIndex(int x, int y)
        {
            return (x - 1) + (y - 1)*Width;
        }

        public int NonEmptyCellsCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Width*Height; i++)
                    if (Cells[i] != CellHelper.EmptyCell)
                        count++;
                return count;
            }
        }

        public int PieceSpawnX
        {
            get { return 1 + (Width/2); }
        }

        public int PieceSpawnY
        {
            get { return Height; }
        }

        public bool CheckNoConflict(IPiece piece, bool checkTop = false)
        {
            //if (piece.PosX < 1)
            //    return false;
            //if (piece.PosX > Width)
            //    return false;
            //if (piece.PosY < 1)
            //    return false;
            //if (checkTop && piece.PosY > Height)
            //    return false;
            for (int i = 1; i <= piece.TotalCells; i++)
            {
                // Get piece position in board
                int x, y;
                piece.GetCellAbsolutePosition(i, out x, out y);
                // Check out of board
                if (x < 1)
                    return false;
                if (x > Width)
                    return false;
                if (y < 1)
                    return false;
                if (checkTop && y > Height)
                    return false;
                // Check if piece already occupied
                if (this[x, y] != CellHelper.EmptyCell)
                    return false;
            }
            return true;
        }

        //public bool CheckNoConflictWithBoard(IPiece piece, bool checkTop = false)
        //{
        //    if (piece.PosX < 1)
        //        return false;
        //    if (piece.PosX > Width)
        //        return false;
        //    if (piece.PosY < 1)
        //        return false;
        //    if (checkTop && piece.PosY > Height)
        //        return false;
        //    for (int i = 1; i <= piece.TotalCells; i++)
        //    {
        //        // Get piece position in board
        //        int x, y;
        //        piece.GetCellAbsolutePosition(i, out x, out y);
        //        // Check out of board
        //        if (x < 1)
        //            return false;
        //        if (x > Width)
        //            return false;
        //        if (y < 1)
        //            return false;
        //        if (checkTop && y > Height)
        //            return false;
        //    }
        //    return true;
        //}

        public int CollapseCompletedRows()
        {
            int rows = 0;
            // Scan each row of the current board, starting from the bottom of the pile.
            // If any row is completely filled, then "eliminate" the row by collapsing
            // all rows above the complete row down to fill the gap.  Note that we must
            // check the same row again if we do a collapse.
            for (int y = 1; y < Height; y++) // bottom-to-top (except top row)
            {
                // Check if the row is full
                bool rowIsFull = true; // hypothesis
                for (int x = 1; x <= Width; x++)
                {
                    byte cellValue = this[x, y];
                    if (cellValue == CellHelper.EmptyCell)
                    {
                        rowIsFull = false;
                        break;
                    }
                }

                // If the row is full, increment count, and collapse pile down
                if (rowIsFull)
                {
                    rows++; // A full row is to be collapsed

                    // Copy rows
                    for (int copySourceY = (y + 1); copySourceY <= Height; copySourceY++)
                        for (int copySourceX = 1; copySourceX <= Width; copySourceX++)
                        {
                            byte cellValue = this[copySourceX, copySourceY];
                            this[copySourceX, copySourceY - 1] = cellValue;
                        }

                    // Clear top row ("copy" from infinite emptiness above board)
                    for (int copySourceX = 1; copySourceX <= Width; copySourceX++)
                        this[copySourceX, Height] = CellHelper.EmptyCell;

                    y--; // Force the same row to be checked again
                }
            }

            return rows;
        }

        public int CollapseCompletedRows(out List<Specials> specials, out List<Pieces> pieces)
        {
            specials = new List<Specials>();
            pieces = new List<Pieces>();
            int rows = 0;
            // Scan each row of the current board, starting from the bottom of the pile.
            // If any row is completely filled, then "eliminate" the row by collapsing
            // all rows above the complete row down to fill the gap.  Note that we must
            // check the same row again if we do a collapse.
            for (int y = 1; y < Height; y++) // bottom-to-top (except top row)
            {
                // Check if the row is full
                bool rowIsFull = true; // hypothesis
                for (int x = 1; x <= Width; x++)
                {
                    byte cellValue = this[x, y];
                    if (cellValue == CellHelper.EmptyCell)
                    {
                        rowIsFull = false;
                        break;
                    }
                }

                // If the row is full, increment count, and collapse pile down
                if (rowIsFull)
                {
                    rows++; // A full row is to be collapsed

                    // Get specials/pieces from row
                    for (int x = 1; x <= Width; x++)
                    {
                        Specials cellSpecial = CellHelper.GetSpecial(this[x, y]);
                        if (cellSpecial != Specials.Invalid)
                            specials.Add(cellSpecial);
                        Pieces cellPiece = CellHelper.GetColor(this[x, y]);
                        if (cellPiece != Pieces.Invalid)
                            pieces.Add(cellPiece);
                    }

                    // Copy rows
                    for (int copySourceY = (y + 1); copySourceY <= Height; copySourceY++)
                        for (int copySourceX = 1; copySourceX <= Width; copySourceX++)
                        {
                            byte cellValue = this[copySourceX, copySourceY];
                            this[copySourceX, copySourceY - 1] = cellValue;
                        }

                    // Clear top row ("copy" from infinite emptiness above board)
                    for (int copySourceX = 1; copySourceX <= Width; copySourceX++)
                        this[copySourceX, Height] = CellHelper.EmptyCell;

                    y--; // Force the same row to be checked again
                }
            }

            return rows;
        }

        public void CommitPiece(IPiece piece)
        {
            //if (piece.PosX < 1 || piece.PosX > Width)
            //    return;
            //if (piece.PosY < 1 || piece.PosY > Height)
            //    return;
            for (int i = 1; i <= piece.TotalCells; i++)
            {
                // Get piece position in board
                int x, y;
                piece.GetCellAbsolutePosition(i, out x, out y);
                // Check out of board
                if (x < 1)
                    return;
                if (x > Width)
                    return;
                if (y < 1)
                    return;
                if (y > Height)
                    return;
                // Add piece in board
                this[x, y] = CellHelper.SetColor(piece.Value); // indexer will handle cell out of board
            }
        }

        public void DropAndCommit(IPiece piece)
        {
            Drop(piece);
            CommitPiece(piece);
        }

        public void Drop(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            bool goalAcceptable = CheckNoConflict(piece);

            if (!goalAcceptable) // cannot drop piece at all
                return;

            // Try successively larger drop distances, up to the point of failure.
            // The last successful drop distance becomes our drop distance.
            int lastSuccessfulDropDistance = 0;
            IPiece tempPiece = piece.Clone();
            for (int trial = 0; trial <= Height; trial++)
            {
                tempPiece.CopyFrom(piece);
                // Set temporary piece to new trial Y
                tempPiece.Translate(0, -trial);

                goalAcceptable = CheckNoConflict(tempPiece);
                if (!goalAcceptable)
                    // We failed to drop this far.  Stop drop search.
                    break;
                else
                    lastSuccessfulDropDistance = trial;
            }
            // Simply update the piece Y value.
            piece.Translate(0, -lastSuccessfulDropDistance);
        }

        public bool MoveLeft(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to move left
            IPiece tempPiece = piece.Clone();
            tempPiece.Translate(-1, 0);
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece X value
            piece.Translate(-1, 0);
            return true;
        }

        public bool MoveRight(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to move right
            IPiece tempPiece = piece.Clone();
            tempPiece.Translate(+1, 0);
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece X value
            piece.Translate(+1, 0);
            return true;
        }

        public bool MoveDown(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to move down
            IPiece tempPiece = piece.Clone();
            tempPiece.Translate(0, -1);
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece Y value
            piece.Translate(0, -1);
            return true;
        }

        public virtual bool RotateClockwise(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to rotate
            IPiece tempPiece = piece.Clone();
            tempPiece.RotateClockwise();
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece rotation
            piece.RotateClockwise();
            return true;
        }

        public virtual bool RotateCounterClockwise(IPiece piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to rotate
            IPiece tempPiece = piece.Clone();
            tempPiece.RotateCounterClockwise();
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece rotation
            piece.RotateCounterClockwise();
            return true;
        }

        #region Specials

        /// <summary>
        /// Add <param name="count"/> junk lines
        /// </summary>
        /// <param name="count">Number of lines to add</param>
        /// <param name="randomFunc">Cell random generator</param>
        public void AddLines(int count, Func<Pieces> randomFunc)
        {
            for (int i = 0; i < count; i++)
                AddJunkLine(randomFunc);
        }

        /// <summary>
        /// Clear bottom line
        /// </summary>
        public void ClearLine()
        {
            // Copy rows y-1 <- y
            for (int copySourceY = 1; copySourceY <= Height; copySourceY++)
                for (int copySourceX = 1; copySourceX <= Width; copySourceX++)
                {
                    byte cellValue = this[copySourceX, copySourceY];
                    this[copySourceX, copySourceY - 1] = cellValue;
                }
        }

        /// <summary>
        /// Remove all cells
        /// </summary>
        public void NukeField()
        {
            for (int i = 0; i < Width*Height; i++)
                Cells[i] = CellHelper.EmptyCell;
        }

        /// <summary>
        /// Remove <paramref name="count"/> random cells
        /// </summary>
        /// <param name="count"></param>
        public void RandomBlocksClear(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = 1 + _random.Next(Width*Height);
                Cells[index] = CellHelper.EmptyCell;
            }
        }

        /// <summary>
        /// Switches board with another player's board. If either of the board is over 16 piece high, the board will be lowered.
        /// </summary>
        /// <param name="cells"></param>
        public void SwitchFields(byte[] cells)
        {
            // NOTHING TO DO, this is handled by the server
        }

        /// <summary>
        /// Removes all special cells from a players field
        /// </summary>
        public void ClearSpecialBlocks(Func<Pieces> randomFunc)
        {
            for (int i = 0; i < Width*Height; i++)
                if (CellHelper.IsSpecial(Cells[i]))
                    Cells[i] = CellHelper.SetColor(randomFunc()); // set random piece
        }

        /// <summary>
        /// Takes all the cells on the field and "pulls" them all towards the bottom of the field eliminating any gaps in the blockstack
        /// </summary>
        public void BlockGravity()
        {
            // For each column
            //  Get pile max
            //  For each row from bottom to pile max, if piece is hole, find first non-empty piece above and move it
            for (int x = 1; x <= Width; x++)
            {
                int pileHeight = 0;
                for (int y = Height; y >= 1; y--)
                    if (this[x, y] != CellHelper.EmptyCell)
                    {
                        pileHeight = y;
                        break;
                    }
                for (int y = 1; y < pileHeight; y++)
                    if (this[x, y] == CellHelper.EmptyCell) // hole
                    {
                        bool foundNonEmptyCell = false;
                        for (int yi = 1; yi <= pileHeight; yi++) // get first non-empty piece above
                        {
                            byte cellValue = this[x, y + yi];
                            if (cellValue != CellHelper.EmptyCell) // found one, move it
                            {
                                this[x, y] = cellValue;
                                this[x, y + yi] = CellHelper.EmptyCell;
                                foundNonEmptyCell = true;
                                break;
                            }
                        }
                        if (!foundNonEmptyCell) // no more piece above
                            break;
                    }
            }
            // Delete completed rows
            CollapseCompletedRows();
        }

        /// <summary>
        /// Each of the lines of cells on a players field to randomly shift left or right or not at all
        /// </summary>
        public void BlockQuake()
        {
            for (int y = 1; y <= Height; y++) // top-down
            {
                int shift = _random.Next(3) - 1; // 0 -> 2 ==> -1 -> 1
                if (shift < 0)
                {
                    for (int x = 1; x <= Width - 1; x++) // x <- x+1
                    {
                        byte cellValue = this[x + 1, y];
                        this[x, y] = cellValue;
                    }
                    // Clear last piece in row
                    this[Width, y] = CellHelper.EmptyCell;
                }
                else if (shift > 0)
                {
                    for (int x = Width; x >= 2; x--) // x <- x-1
                    {
                        byte cellValue = this[x - 1, y];
                        this[x, y] = cellValue;
                    }
                    // Clear first piece in row
                    this[1, y] = CellHelper.EmptyCell;
                }
                // if shift == 0, nothing to do
            }
        }

        /// <summary>
        /// When used on a player, it clears 3x3 portions on their field anywhere there are 'o' cells. Any cells that were in the 3x3 areas are scattered around the field.
        /// </summary>
        public void BlockBomb()
        {
            // Get bombs
            List<int> bombIndexes = Cells.Select((cell, index) => new
            {
                cell,
                index
            })
                .Where(x => CellHelper.GetSpecial(x.cell) == Specials.BlockBomb)
                .Select(x => x.index).ToList();
            // Compute scattered parts new locations
            List<Tuple<int, int, byte>> scattered = new List<Tuple<int, int, byte>>(); // Keep scattered parts new location (old location, new location, piece value)
            foreach (int index in bombIndexes)
            {
                int x, y;
                GetCellXY0Based(index, out x, out y);
                if (x > 0 && y > 0)
                {
                    this[x, y] = CellHelper.EmptyCell; // clear bomb
                    for (int yi = -1; yi <= 1; yi++)
                        for (int xi = -1; xi <= 1; xi++)
                            if (xi != 0 && yi != 0)
                            {
                                int oldX = x + xi;
                                int oldY = y + yi;
                                byte cellValue = this[oldX, oldY]; // no need to check out-of-range, this is done by indexer
                                if (cellValue != CellHelper.EmptyCell) // no need to move empty piece
                                {
                                    // get scattered coordinates
                                    int oldIndex = GetCellIndex(oldX, oldY);
                                    // get scattered new coordinates
                                    int newX = x + xi*2 + (_random.Next(3) - 1); // some x deviation -3 -> +3
                                    int newY = y + 5 + _random.Next(5); // some y deviation  +5 -> +10
                                    if (newX <= 1) newX = 1;
                                    if (newX >= Width) newX = Width;
                                    if (newY <= 1) newY = 1;
                                    if (newY >= Height - 4) newY = Height - 4; // we don't want to scatter to high
                                    int newIndex = GetCellIndex(newX, newY);
                                    scattered.Add(new Tuple<int, int, byte>(oldIndex, newIndex, cellValue));
                                }
                            }
                }
            }
            // Copy scattered part back in board
            foreach (Tuple<int, int, byte> tuple in scattered)
            {
                Cells[tuple.Item1] = CellHelper.EmptyCell; // remove old part
                Cells[tuple.Item2] = tuple.Item3; // set new part
            }
        }

        /// <summary>
        /// Clear an random column
        /// </summary>
        public void ClearColumn()
        {
            int column = 1 + _random.Next(Width);
            for (int y = 1; y <= Height; y++)
                this[column, y] = CellHelper.EmptyCell;
        }

        /// <summary>
        /// Clears every second column of the field
        /// </summary>
        public void ZebraField()
        {
            for (int x = 2; x <= Width; x += 2)
                for (int y = 1; y <= Height; y++)
                    this[x, y] = CellHelper.EmptyCell;
        }

        /// <summary>
        /// Take all the cells on the field and pulls them all towards the left of the field eliminating any gaps in the blockstack
        /// </summary>
        public void LeftGravity()
        {
            // For each row
            //  Get pile max
            //  For each column from left to pile max, if piece is hole, find first non-empty piece on the right and move it
            for (int y = 1; y <= Height; y++)
            {
                int pileWidth = 0;
                for (int x = Width; x >= 1; x--)
                    if (this[x, y] != CellHelper.EmptyCell)
                    {
                        pileWidth = x;
                        break;
                    }
                for (int x = 1; x < pileWidth; x++)
                    if (this[x, y] == CellHelper.EmptyCell) // hole
                    {
                        bool foundNonEmptyCell = false;
                        for (int xi = 1; xi <= pileWidth; xi++) // get first non-empty piece on the right
                        {
                            byte cellValue = this[x + xi, y];
                            if (cellValue != CellHelper.EmptyCell) // found one, move it
                            {
                                this[x, y] = cellValue;
                                this[x + xi, y] = CellHelper.EmptyCell;
                                foundNonEmptyCell = true;
                                break;
                            }
                        }
                        if (!foundNonEmptyCell) // no more piece above
                            break;
                    }
            }
        }

        // Associated with Specials
        public void SpawnSpecialBlocks(int count, Func<Specials> randomFunc)
        {
            // Build list of cells without any specials
            List<int> cellsOccupiedWithoutSpecials = Cells.Select((cell, index) => new
            {
                cell,
                index
            })
                .Where(x => x.cell != CellHelper.EmptyCell && !CellHelper.IsSpecial(x.cell))
                .Select(x => x.index)
                .ToList();
            // Transform 'count' cells into special
            for (int i = 0; i < count; i++)
            {
                int n = cellsOccupiedWithoutSpecials.Count;
                if (n > 0) // if there is at least one non-special piece
                {
                    // get random piece without specials
                    int randomCell = _random.Next(n);
                    int cellIndex = cellsOccupiedWithoutSpecials[randomCell];
                    // get random special
                    Specials special = randomFunc();
                    // add special
                    Cells[cellIndex] = CellHelper.SetSpecial(special);

                    // remove piece from available list
                    cellsOccupiedWithoutSpecials.RemoveAt(randomCell);
                }
                else
                    break; // no more cells without specials
            }
        }

        public void RemoveCellsHigherThan(int height)
        {
            // Used when boards are switched between players
            for (int y = Height; y >= height; y--) // top-down
                for (int x = 1; x <= Width; x++)
                    this[x, y] = CellHelper.EmptyCell;
        }

        #endregion

        protected void AddJunkLine(Func<Pieces> randomFunc)
        {
            // First, do top-down row copying to raise all rows by 'count' row, with the top row being discarded.
            for (int y = Height; y > 1; y--) // top-down
                // Copy row (y-1) to row y; i.e., copy upward.
                for (int x = 1; x <= Width; x++)
                {
                    byte cellValue = this[x, y - 1];
                    this[x, y] = cellValue;
                }
            // Put random junk in bottom row (row 1).
            int hole = 1 + _random.Next(Width);
            for (int x = 1; x <= Width; x++)
            {
                // Fill row except hole
                byte cellValue = x == hole ? CellHelper.EmptyCell : CellHelper.SetColor(randomFunc());
                this[x, 1] = cellValue;
            }
        }

        protected void GetCellXY0Based(int index, out int x, out int y)
        {
            x = y = 0;
            if (index < 0)
                return;
            if (index >= Width*Height)
                return;
            x = 1 + (index%Width);
            y = 1 + (index/Width);
        }
    }
}
