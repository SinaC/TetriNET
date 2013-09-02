using System;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient
{
    public class Board : IBoard
    {
        private const int TetriminosCount = 7; // TODO: move this elsewhere
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

        public void FillWithRandomCells()
        {
            for (int i = 0; i < Width * Height; i++)
                Cells[i] = (byte)(1 + (_random.Next() % TetriminosCount));
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
            get { return Height-1; }
        }

        public int CollapseCompletedRows()
        {
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

        public bool CheckNoConflict(ITetrimino piece)
        {
            if (piece.PosX < 1 || piece.PosX > Width)
                return false;
            if (piece.PosY < 1 || piece.PosY > Height)
                return false;
            for (int i = 1; i <= piece.TotalCells; i++)
            {
                // Get cell position in board
                int x, y;
                piece.GetCellAbsolutePosition(i, out x, out y);
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

        public void GetAccessibleTranslationsForOrientation(ITetrimino piece, out bool isMovePossible, out int minDeltaX, out int maxDeltaX)
        {
            isMovePossible = false;
            minDeltaX = 0;
            maxDeltaX = 0;

            ITetrimino tempPiece = piece.Clone();
            
            // Check if we can move
            bool moveAcceptable = CheckNoConflict(tempPiece);
            if (!moveAcceptable)
                return;
            isMovePossible = true;

            // Scan from center to left to find left limit.
            for (int trial = 0; trial >= -Width; trial--)
            {
                // Copy piece
                tempPiece.CopyFrom(piece);
                // Translate
                tempPiece.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = CheckNoConflict(tempPiece);
                if (moveAcceptable)
                    minDeltaX = trial;
                else
                    break;
            }

            // Scan from center to right to find right limit.
            for (int trial = 0; trial <= Width; trial++)
            {
                // Copy piece
                tempPiece.CopyFrom(piece);
                // Translate
                tempPiece.Translate(trial, 0);
                // Check if move is valid
                moveAcceptable = CheckNoConflict(tempPiece);
                if (moveAcceptable)
                    maxDeltaX = trial;
                else
                    break;
            }
        }

        public void CommitPiece(ITetrimino piece)
        {
            if (piece.PosX < 1 || piece.PosX > Width)
                return;
            if (piece.PosY < 1 || piece.PosY > Height)
                return;
            for (int i = 1; i <= piece.TotalCells; i++)
            {
                // Get cell position in board
                int x, y;
                piece.GetCellAbsolutePosition(i, out x, out y);
                // Add cell in board
                this[x, y] = piece.Value;
            }
        }

        public void DropAndCommit(ITetrimino piece)
        {
            Drop(piece);
            CommitPiece(piece);
        }

        public void Drop(ITetrimino piece)
        {
            // Special case: cannot place piece at starting location.
            bool goalAcceptable = CheckNoConflict(piece);

            if (!goalAcceptable) // cannot drop piece at all
                return;

            // Try successively larger drop distances, up to the point of failure.
            // The last successful drop distance becomes our drop distance.
            int lastSuccessfulDropDistance = 0;
            ITetrimino tempPiece = piece.Clone();
            for (int trial = 0; trial <= Height; trial++)
            {
                tempPiece.CopyFrom(piece);
                // Set temporary piece to new trial Y
                //tempPiece.PosY -= trial;
                tempPiece.Translate(0,-trial);

                goalAcceptable = CheckNoConflict(tempPiece);
                if (!goalAcceptable)
                    // We failed to drop this far.  Stop drop search.
                    break;
                else
                    lastSuccessfulDropDistance = trial;
            }

            // Simply update the piece Y value.
            //piece.PosY -= lastSuccessfulDropDistance;
            piece.Translate(0, -lastSuccessfulDropDistance);
        }

        public bool MoveLeft(ITetrimino piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to move left
            ITetrimino tempPiece = piece.Clone();
            tempPiece.Translate(-1, 0);
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece X value
            piece.Translate(-1,0);
            return true;
        }

        public bool MoveRight(ITetrimino piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to move right
            ITetrimino tempPiece = piece.Clone();
            tempPiece.Translate(+1, 0);
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece X value
            piece.Translate(+1, 0);
            return true;
        }

        public bool MoveDown(ITetrimino piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to move down
            ITetrimino tempPiece = piece.Clone();
            tempPiece.Translate(0, -1);
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece Y value
            piece.Translate(0, -1);
            return true;
        }

        public bool RotateClockwise(ITetrimino piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to rotate
            ITetrimino tempPiece = piece.Clone();
            tempPiece.RotateClockwise();
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece rotation
            piece.RotateClockwise();
            return true;
        }

        public bool RotateCounterClockwise(ITetrimino piece)
        {
            // Special case: cannot place piece at starting location.
            if (!CheckNoConflict(piece))
                return false;
            // Try to rotate
            ITetrimino tempPiece = piece.Clone();
            tempPiece.RotateCounterClockwise();
            if (!CheckNoConflict(tempPiece))
                return false;
            // Simply update the piece rotation
            piece.RotateCounterClockwise();
            return true;
        }

        #region Specials
        
        public void AddLines(int count)
        {
            for(int i = 0; i < count; i++)
                AddJunkLine();
        }

        public void ClearLine()
        {
            // TODO
        }

        public void NukeField()
        {
            for (int i = 0; i < Width*Height; i++)
                Cells[i] = 0;
        }

        public void RandomBlocksClear(int count)
        {
            // TODO
        }

        public void SwitchFields(byte[] cells)
        {
            // TODO
        }

        public void ClearSpecialBlocks()
        {
            for (int i = 0; i < Width*Height; i++)
                Cells[i] = (byte)(Cells[i] & 0x0F); // remove special
        }

        public void BlockGravity()
        {
            // TODO
        }

        public void BlockQuake()
        {
            // TODO
        }

        public void BlockBomb()
        {
            // TODO
        }

        #endregion

        protected void AddJunkLine()
        {
            // First, do top-down row copying to raise all rows by 'count' row, with the top row being discarded.
            for (int y = Height; y > 1; y--) // top-down
            {
                // Copy row (y-1) to row y; i.e., copy upward.
                for (int x = 1; x <= Width; x++)
                {
                    byte cellValue = this[x, y - 1];
                    this[x, y] = cellValue;
                }
            }
            // Put random junk in bottom row (row 1).
            int hole = _random.Next(Width);
            for (int x = 1; x <= Width; x++)
            {
                byte cellValue;
                if (x == hole)
                    cellValue = 0;
                else
                    cellValue = (byte) (1 + (_random.Next()%TetriminosCount));
                this[x, 1] = cellValue;
            }
        }
    }
}
