using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using TetriNET.Common;
using TetriNET.Common.Contracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.Client.DefaultBoardAndTetriminos
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
                Cells[i] = 0;
        }

        public void FillWithRandomCells(Func<Tetriminos> randomFunc)
        {
            for (int i = 0; i < Width * Height; i++)
                Cells[i] = CellHelper.SetTetrimino(randomFunc());
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
                    return 0;
                if (x > Width)
                    return 0;
                if (y <= 0)
                    return 0;
                if (y > Height)
                    return 0;
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

        public int TetriminoSpawnX
        {
            get { return 1 + (Width / 2); }
        }

        public int TetriminoSpawnY
        {
            get { return Height; }
        }

        public int CollapseCompletedRows(out List<Specials> specials)
        {
            specials = new List<Specials>();
            // Scan each row of the current board, starting from the bottom of the pile.
            // If any row is completely filled, then "eliminate" the row by collapsing
            // all rows above the complete row down to fill the gap.  Note that we must
            // check the same row again if we do a collapse.
            int rows = 0;
            for (int y = 1; y < Height; y++) // bottom-to-top (except top row)
            {
                // Check if the row is full
                bool rowIsFull = true; // hypothesis
                for (int x = 1; x <= Width; x++)
                {
                    byte cellValue = this[x, y];
                    if (cellValue == 0)
                    {
                        rowIsFull = false;
                        break;
                    }
                }

                // If the row is full, increment count, and collapse pile down
                if (rowIsFull)
                {
                    rows++; // A full row is to be collapsed

                    // Get specials from row
                    for (int x = 1; x <= Width; x++)
                    {
                        Specials cellSpecial = CellHelper.Special(this[x, y]);
                        if (cellSpecial > 0)
                            specials.Add(cellSpecial);
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
                        this[copySourceX, Height] = 0;

                    y--; // Force the same row to be checked again
                }
            }

            return rows;
        }

        public bool CheckNoConflict(ITetrimino tetrimino)
        {
            if (tetrimino.PosX < 1 || tetrimino.PosX > Width)
                return false;
            if (tetrimino.PosY < 1 || tetrimino.PosY > Height)
                return false;
            for (int i = 1; i <= tetrimino.TotalCells; i++)
            {
                // Get cell position in board
                int x, y;
                tetrimino.GetCellAbsolutePosition(i, out x, out y);
                // Check out of board
                if (x < 1 || x > Width)
                    return false;
                if (y < 1 || y > Height)
                    return false;
                // Check if cell already occupied
                if (this[x, y] > 0)
                    return false;
            }
            return true;
        }

        public void GetAccessibleTranslationsForOrientation(ITetrimino tetrimino, out bool isMovePossible, out int minDeltaX, out int maxDeltaX)
        {
            isMovePossible = false;
            minDeltaX = 0;
            maxDeltaX = 0;

            ITetrimino tempTetrimino = tetrimino.Clone();
            
            // Check if we can move
            bool moveAcceptable = CheckNoConflict(tempTetrimino);
            if (!moveAcceptable)
                return;
            isMovePossible = true;

            // Scan from center to left to find left limit.
            for (int trial = 0; trial >= -Width; trial--)
            {
                // Copy tetrimino
                tempTetrimino.CopyFrom(tetrimino);
                // Translate
                tempTetrimino.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = CheckNoConflict(tempTetrimino);
                if (moveAcceptable)
                    minDeltaX = trial;
                else
                    break;
            }

            // Scan from center to right to find right limit.
            for (int trial = 0; trial <= Width; trial++)
            {
                // Copy tetrimino
                tempTetrimino.CopyFrom(tetrimino);
                // Translate
                tempTetrimino.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = CheckNoConflict(tempTetrimino);
                if (moveAcceptable)
                    maxDeltaX = trial;
                else
                    break;
            }
        }

        public void CommitTetrimino(ITetrimino tetrimino)
        {
            if (tetrimino.PosX < 1 || tetrimino.PosX > Width)
                return;
            if (tetrimino.PosY < 1 || tetrimino.PosY > Height)
                return;
            for (int i = 1; i <= tetrimino.TotalCells; i++)
            {
                // Get cell position in board
                int x, y;
                tetrimino.GetCellAbsolutePosition(i, out x, out y);
                // Add cell in board
                this[x, y] = tetrimino.Value;
            }
        }

        public void DropAndCommit(ITetrimino tetrimino)
        {
            Drop(tetrimino);
            CommitTetrimino(tetrimino);
        }

        public void Drop(ITetrimino tetrimino)
        {
            // Special case: cannot place tetrimino at starting location.
            bool goalAcceptable = CheckNoConflict(tetrimino);

            if (!goalAcceptable) // cannot drop tetrimino at all
                return;

            // Try successively larger drop distances, up to the point of failure.
            // The last successful drop distance becomes our drop distance.
            int lastSuccessfulDropDistance = 0;
            ITetrimino tempTetrimino = tetrimino.Clone();
            for (int trial = 0; trial <= Height; trial++)
            {
                tempTetrimino.CopyFrom(tetrimino);
                // Set temporary tetrimino to new trial Y
                tempTetrimino.Translate(0, -trial);

                goalAcceptable = CheckNoConflict(tempTetrimino);
                if (!goalAcceptable)
                    // We failed to drop this far.  Stop drop search.
                    break;
                else
                    lastSuccessfulDropDistance = trial;
            }

            // Simply update the tetrimino Y value.
            tetrimino.Translate(0, -lastSuccessfulDropDistance);
        }

        public bool MoveLeft(ITetrimino tetrimino)
        {
            // Special case: cannot place tetrimino at starting location.
            if (!CheckNoConflict(tetrimino))
                return false;
            // Try to move left
            ITetrimino tempTetrimino = tetrimino.Clone();
            tempTetrimino.Translate(-1, 0);
            if (!CheckNoConflict(tempTetrimino))
                return false;
            // Simply update the tetrimino X value
            tetrimino.Translate(-1, 0);
            return true;
        }

        public bool MoveRight(ITetrimino tetrimino)
        {
            // Special case: cannot place tetrimino at starting location.
            if (!CheckNoConflict(tetrimino))
                return false;
            // Try to move right
            ITetrimino tempTetrimino = tetrimino.Clone();
            tempTetrimino.Translate(+1, 0);
            if (!CheckNoConflict(tempTetrimino))
                return false;
            // Simply update the tetrimino X value
            tetrimino.Translate(+1, 0);
            return true;
        }

        public bool MoveDown(ITetrimino tetrimino)
        {
            // Special case: cannot place tetrimino at starting location.
            if (!CheckNoConflict(tetrimino))
                return false;
            // Try to move down
            ITetrimino tempTetrimino = tetrimino.Clone();
            tempTetrimino.Translate(0, -1);
            if (!CheckNoConflict(tempTetrimino))
                return false;
            // Simply update the tetrimino Y value
            tetrimino.Translate(0, -1);
            return true;
        }

        public bool RotateClockwise(ITetrimino tetrimino)
        {
            // Special case: cannot place tetrimino at starting location.
            if (!CheckNoConflict(tetrimino))
                return false;
            // Try to rotate
            ITetrimino tempTetrimino = tetrimino.Clone();
            tempTetrimino.RotateClockwise();
            if (!CheckNoConflict(tempTetrimino))
                return false;
            // Simply update the tetrimino rotation
            tetrimino.RotateClockwise();
            return true;
        }

        public bool RotateCounterClockwise(ITetrimino tetrimino)
        {
            // Special case: cannot place tetrimino at starting location.
            if (!CheckNoConflict(tetrimino))
                return false;
            // Try to rotate
            ITetrimino tempTetrimino = tetrimino.Clone();
            tempTetrimino.RotateCounterClockwise();
            if (!CheckNoConflict(tempTetrimino))
                return false;
            // Simply update the tetrimino rotation
            tetrimino.RotateCounterClockwise();
            return true;
        }

        #region Specials

        /// <summary>
        /// Add <param name="count"/> junk lines
        /// </summary>
        /// <param name="count">Number of lines to add</param>
        /// <param name="randomFunc">Cell random generator</param>
        public void AddLines(int count, Func<Tetriminos> randomFunc)
        {
            for(int i = 0; i < count; i++)
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
                Cells[i] = 0;
        }

        /// <summary>
        /// Remove <paramref name="count"/> random cells
        /// </summary>
        /// <param name="count"></param>
        public void RandomBlocksClear(int count)
        {
            for (int i = 0; i < count; i++)
            {
                int index = _random.Next(Width*Height);
                Cells[index] = 0;
            }
        }

        /// <summary>
        /// Switches board with another player's board. If either of the board is over 16 cell high, the board will be lowered.
        /// </summary>
        /// <param name="cells"></param>
        public void SwitchFields(byte[] cells)
        {
            // NOTHING TO DO, this is handled by the server
        }

        /// <summary>
        /// Removes all special cells from a players field
        /// </summary>
        public void ClearSpecialBlocks()
        {
            for (int i = 0; i < Width*Height; i++)
                Cells[i] = CellHelper.ClearSpecial(Cells[i]); // remove special
        }

        /// <summary>
        /// Takes all the cells on the field and "pulls" them all towards the bottom of the field eliminating any gaps in the blockstack
        /// </summary>
        public void BlockGravity()
        {
            // For each column
            //  Get pile max
            //  For each row from bottom to pile max, if cell is hole, find first non-empty cell above and move it
            for (int x = 1; x <= Width; x++)
            {
                int pileHeight = 0;
                for(int y = Height; y >= 1; y--)
                    if (this[x, y] > 0)
                    {
                        pileHeight = y;
                        break;
                    }
                for(int y = 1; y < pileHeight; y++)
                    if (this[x, y] == 0) // hole
                    {
                        bool foundNonEmptyCell = false;
                        for (int yi = 1; yi <= pileHeight; yi++) // get first non-empty cell above
                        {
                            byte cellValue = this[x, y + yi];
                            if (cellValue > 0) // found one, move it
                            {
                                this[x, y] = cellValue;
                                this[x, y + yi] = 0;
                                foundNonEmptyCell = true;
                                break;
                            }
                        }
                        if (!foundNonEmptyCell) // no more cell above
                            break;
                    }
            }
        }

        /// <summary>
        /// Each of the lines of cells on a players field to randomly shift left or right or not at all
        /// </summary>
        public void BlockQuake()
        {
            for (int y = 1; y <= Height; y++) // top-down
            {
                int shift = _random.Next(3) - 1; // 0 -> 2 ==> -1 -> 1
                if (shift < 0) {
                    for (int x = 1; x <= Width - 1; x++) // x <- x+1
                    {
                        byte cellValue = this[x+1, y];
                        this[x, y] = cellValue;
                    }
                    // Clear last cell in row
                    this[Width, y] = 0;
                }
                else if (shift > 0)
                {
                    for (int x = Width; x >= 2; x--) // x <- x-1
                    {
                        byte cellValue = this[x - 1, y];
                        this[x, y] = cellValue;
                    }
                    // Clear first cell in row
                    this[1, y] = 0;
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
                .Where(x => CellHelper.Special(x.cell) == Specials.BlockBomb)
                .Select(x => x.index).ToList();
            // Compute scattered parts new locations
            List<Tuple<int, int, byte>> scattered = new List<Tuple<int, int, byte>>(); // Keep scattered parts new location (old location, new location, cell value)
            foreach (int index in bombIndexes)
            {
                int x, y;
                GetCellXY(index, out x, out y);
                if (x > 0 && y > 0)
                {
                    this[x, y] = 0; // clear bomb
                    for (int yi = -1; yi <= 1; yi++)
                        for (int xi = -1; xi <= 1; xi++)
                            if (xi != 0 && yi != 0)
                            {
                                int oldX = x + xi;
                                int oldY = y + yi;
                                byte cellValue = this[oldX, oldY]; // no need to check out-of-range, this is done by indexer
                                if (cellValue != 0) // no need to move empty cell
                                {
                                    // get scattered coordinates
                                    int oldIndex = GetCellIndex(oldX, oldY);
                                    // get scattered new coordinates
                                    int newX = x + xi * 2 + (_random.Next(3) - 1 ); // some x deviation -3 -> +3
                                    int newY = y + 5 + _random.Next(5); // some y deviation  +5 -> +10
                                    if (newX <= 1) newX = 1;
                                    if (newX >= Width) newX = Width;
                                    if (newY <= 1) newY = 1;
                                    if (newY >= Height-4) newY = Height-4; // we don't want to scatter to high
                                    int newIndex = GetCellIndex(newX, newY);
                                    scattered.Add(new Tuple<int, int, byte>(oldIndex, newIndex, cellValue));
                                }
                            }
                }
            }
            // Copy scattered part back in board
            foreach (Tuple<int, int, byte> tuple in scattered)
            {
                Cells[tuple.Item1] = 0; // remove old part
                Cells[tuple.Item2] = tuple.Item3; // set new part
            }
        }

        public void SpawnSpecialBlocks(int count, Func<Specials> randomFunc)
        {
            // Build list of cells without any specials
            List<int> cellsOccupiedWithoutSpecials = Cells.Select((cell, index) => new
            {
                cell,
                index
            })
                .Where(x => x.cell > 0 && CellHelper.Special(x.cell) == Specials.Invalid)
                .Select(x => x.index)
                .ToList();
            // Transform 'count' cells into special
            for (int i = 0; i < count; i++)
            {
                int n = cellsOccupiedWithoutSpecials.Count;
                if (n > 0) // if there is at least one non-special cell
                {
                    // get random cell without specials
                    int randomCell = _random.Next(n);
                    int cellIndex = cellsOccupiedWithoutSpecials[randomCell];
                    // get random special
                    Specials special = randomFunc();
                    // add special
                    Cells[cellIndex] = CellHelper.SetSpecial(Cells[cellIndex], special);

                    // remove cell from available list
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
                    this[x, y] = 0;
        }
        #endregion

        protected void AddJunkLine(Func<Tetriminos> randomFunc)
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
            int hole = _random.Next(Width);
            for (int x = 1; x <= Width; x++)
            {
                // Fill row except hole
                byte cellValue = CellHelper.SetTetrimino(x == hole ? Tetriminos.Invalid : randomFunc());
                this[x, 1] = cellValue;
            }
        }

        protected void GetCellXY(int index, out int x, out int y)
        {
            x = y = 0;
            if (index < 0)
                return;
            if (index >= Width*Height)
                return;
            x = 1 + (index%Width);
            y = 1 + (index/Width);
            Debug.Assert(GetCellIndex(x,y) == index);
        }
    }
}
