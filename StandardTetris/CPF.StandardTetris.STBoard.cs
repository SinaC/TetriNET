// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;



namespace CPF.StandardTetris
{
    public class STBoard
    {
        // logical characteristics
        //
        //   * The horizontal axis is called "X" and points to the right,
        //     and has an origin such that the left column is called X = 1,
        //     and the right column is called X = Width.
        //
        //   * The vertical axis is called "Y" and points upward, and has
        //     an origin such that the bottom row is called Y = 1, and the
        //     top row is called Y = Height.
        //
        //    * Each piece spawns with its origin coincident with the cell
        //         (   X = (1 + (int)(Width/2)),    Y = Height   ).
        //
        //    * The standard width is 10, and the standard height is 20.
        //
        // implementation
        //
        // The board is stored in a one-dimensional array of bytes, arranged
        // such that cells are accessed according to the following index:
        //
        //    Cell Index   =   (Width * (Y - 1))    +    (X - 1);
        //
        // The array has (Width * Height) bytes total.  The first byte
        // corresponds to the cell (1,1), and the last byte corresponds to
        // the cell (Width, Height).

        private int mWidth;
        private int mHeight;
        private byte[] mCells;

        private const int mDefaultWidth = 10;
        private const int mDefaultHeight = 20;

        // Junk row generator
        private STRandom mSTRandomForJunkRows;




        private void InitializeAllFields ( )
        {
            this.mWidth = STBoard.mDefaultWidth;
            this.mHeight = STBoard.mDefaultHeight;
            this.mCells = new byte[this.mWidth * this.mHeight];
            this.mSTRandomForJunkRows = new STRandom();
        }


        public STBoard ( )
        {
            this.InitializeAllFields( );
        }


        public int GetWidth ( )
        {
            return (this.mWidth);
        }

        public int GetHeight ( )
        {
            return (this.mHeight);
        }

        public int GetPieceSpawnX ( )
        {
            return (1 + (this.mWidth / 2));
        }

        public int GetPieceSpawnY ( )
        {
            return (this.mHeight);
        }

        public int GetTotalCells ( )
        {
            return (this.mWidth * this.mHeight);
        }

        public int GetCellIndex ( int x, int y )
        {
            return ( (x-1) + (this.mWidth * (y-1)) );
        }



        public void SetBoardDimensions( int width, int height )
        {
            if (width < 4)
            {
                width = 4;
            }

            if (height < 4)
            {
                height = 4;
            }

            this.mWidth = width;
            this.mHeight = height;
            this.mCells = new byte[this.mWidth * this.mHeight];
        }
        

        public void ClearCells( )
        {
            if (null == this.mCells)
            {
                return;
            }

            int i = 0;
            int n = 0;
            n = this.mCells.Length;
            for ( i = 0; i < n; i++ )
            {
                this.mCells[i] = (byte)0;
            }
        }


        public byte GetCell( int x, int y )
        {
            if (x <= 0)
            {
                return( (byte)0 );
            }

            if (y <= 0)
            {
                return( (byte)0 );
            }

            if (x > this.mWidth)
            {
                return( (byte)0 );
            }

            if (y > this.mHeight)
            {
                return( (byte)0 );
            }

            if (null == this.mCells)
            {
                return( (byte)0 );
            }

            int n = 0;
            n = this.mCells.Length;

            int i = 0;
            i = GetCellIndex( x, y );

            if ((i < 0) || (i >= n))
            {
                return( (byte)0 );
            }

            return( this.mCells[i] );
        }



        public void SetCell( int x, int y, byte value )
        {
            if (x <= 0)
            {
                return;
            }

            if (y <= 0)
            {
                return;
            }

            if (x > this.mWidth)
            {
                return;
            }

            if (y > this.mHeight)
            {
                return;
            }

            if (null == this.mCells)
            {
                return;
            }

            int n = 0;
            n = this.mCells.Length;

            int i = 0;
            i = GetCellIndex( x, y );

            if ((i < 0) || (i >= n))
            {
                return;
            }

            this.mCells[i] = value;
        }


        public void CopyFrom( STBoard board )
        {
            this.mWidth = board.mWidth;
            this.mHeight = board.mHeight;
            this.mCells = new byte[this.mWidth * this.mHeight];
            board.mCells.CopyTo( this.mCells, 0 );
        }



        public int CollapseAnyCompletedRows ( ) // Collapse and return total completed rows
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int totalRowsCollapsed = 0;


            // The following algorithm is space-efficient, but not time-efficient; it's
            // designed to be reliable and easy to understand:
            //
            //   Scan each row of the current board, starting from the bottom of the pile.
            //   If any row is completely filled, then "eliminate" the row by collapsing
            //   all rows above the complete row down to fill the gap.  Note that we must
            //   check the same row again if we do a collapse.

            int y = 0;

            for ( y = 1; y < this.mHeight; y++ ) // bottom-to-top (except top row)
            {
                // Check if the row is full
                int x = 0;
                bool rowIsFull = true; // hypothesis
                for (x = 1; ((x <= this.mWidth) && (true == rowIsFull)); x++)
                {
                    byte cellValue = this.GetCell( x, y );
                    if ((byte)0 == cellValue) rowIsFull = false;
                }

                // If the row is full, increment count, and collapse pile down
                if (true == rowIsFull)
                {
                    totalRowsCollapsed++; // A full row is to be collapsed

                    int copySourceX = 0;
                    int copySourceY = 0;

                    for
                      (
                      copySourceY = (y + 1);
                      copySourceY <= this.mHeight;
                      copySourceY++
                      )
                    {
                        for
                          (
                          copySourceX = 1;
                          copySourceX <= this.mWidth;
                          copySourceX++
                          )
                        {
                            byte cellValue = this.GetCell( copySourceX, copySourceY );
                            this.SetCell( copySourceX, (copySourceY - 1), cellValue );
                        }
                    }

                    // Clear top row ("copy" from infinite emptiness above board)
                    for
                      (
                      copySourceX = 1;
                      copySourceX <= this.mWidth;
                      copySourceX++
                      )
                    {
                        this.SetCell( copySourceX, this.mHeight, (byte)0 );
                    }

                    y--; // Force the same row to be checked again
                }
            }

            return ( totalRowsCollapsed );
        }




        public void LiftPileByOneRowAndAddRandomJunk ( )
        {
            // First, do top-down row copying to raise all rows by one row, with the
            // top row being discarded.
            byte cellValue = (byte)0;
            int x = 0;
            int y = 0;
            for ( y = this.mHeight; y > 1; y-- )  // top-down
            {
                // Copy row (y-1) to row y; i.e., copy upward.
                for ( x = 1; x <= this.mWidth; x++ )
                {
                    cellValue = this.GetCell( x, (y-1) );
                    this.SetCell( x, y, cellValue );
                }
            }
            // Put random junk in bottom row (row 1).
            y = 1;
            for ( x = 1; x <= this.mWidth; x++ )
            {
                cellValue = (byte)0; // 0 == empty cell
                this.mSTRandomForJunkRows.Advance( );
                int randomValue = this.mSTRandomForJunkRows.GetIntegerInRangeUsingCurrentState( 1, 100 );
                if (randomValue > 50)
                {
                    cellValue = (byte)8; // 8 == junk cell
                }
                this.SetCell( x, y, cellValue );
            }
        }


        public int GetTotalOccupiedCells ( )  // result range: 0..(Width * Height)
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int n = 0;
            n = this.mCells.Length;

            int totalOccupiedCells = 0;

            int i = 0;
            for (i = 0; i < n; i++)
            {
                if ((byte)0 != this.mCells[i])
                {
                    totalOccupiedCells++;
                }
            }

            return (totalOccupiedCells);
        }





        public int GetTotalShadowedHoles( )
        {
            if (null == this.mCells)  
            {
                return( 0 );
            }

            int totalShadowedHoles = 0;

            // For each column we search top-down through the rows,
            // noting the first occupied cell, and counting any
            // unoccupied cells after that point...
            int x = 0;
            for ( x = 1; x <= this.mWidth; x++ )
            {
                bool encounteredOccupiedCell = false;
                int y = 0;
                for ( y = this.mHeight; y >= 1; y-- ) // top-to-bottom
                {
                    byte cellValue = 0;
                    cellValue = this.GetCell( x, y );

                    if ((byte)0 != cellValue)
                    {
                        encounteredOccupiedCell = true;
                    }
                    else
                    {
                        // cell is un-occupied...
                        if (true == encounteredOccupiedCell)
                        {
                            // ...and we already encountered an occupied cell above,
                            // so this counts as a hole.
                            totalShadowedHoles++;
                        }
                    }
                }
            }

            return (totalShadowedHoles);
        }







        public int GetPileMaxHeight( ) // result range: 0..Height
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int x = 0;
            int y = 0;

            // top-down search for non-empty cell
            for ( y = this.mHeight; y >= 1; y-- )
            {
                for ( x = 1; x <= this.mWidth; x++ )
                {
                    byte cellValue = (byte)0;
                    cellValue = this.GetCell( x, y );
                    if ((byte)0 != cellValue)  
                    {
                        return( y );
                    }
                }
            }

            return( 0 ); // entire board is empty
        }




        public int GetPileHeightWeightedCells( )
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int x = 0;
            int y = 0;

            int totalWeightedCells = 0;

            for ( y = 1; y <= this.mHeight; y++ )
            {
                for ( x = 1; x <= this.mWidth; x++ )
                {
                    byte cellValue = (byte)0;
                    cellValue = this.GetCell( x, y );
                    if ((byte)0 != cellValue)
                    {
                        totalWeightedCells += y;
                    }
                }
            }

            return (totalWeightedCells);
        }








        public int GetColumnHeight( int x ) // result range: 0..Height
        {
            if (null == this.mCells)
            {
                return (0);
            }

            if (x <= 0) 
            {
                return( 0 );
            }

            if (x > this.mWidth)
            {
                return( 0 );
            }

            // top-down search for first occupied cell
            int y = 0;
            for ( y = this.mHeight; y >= 1; y-- ) // top-down search
            {
                byte cellValue = this.GetCell( x, y );
                if ((byte)0 != cellValue)
                {
                    return( y );
                }
            }
            return( 0 );
        }







		public int  GetSumOfWellHeights( )  
		{
            if (null == this.mCells)
            {
                return (0);
            }

			int  sumOfWellHeights    = 0;
			int  columnHeight        = 0;
			int  columnHeightToLeft  = 0;
			int  columnHeightToRight = 0;

			// Determine height of LEFT  well
			columnHeight = this.GetColumnHeight( 1 );
            columnHeightToRight = this.GetColumnHeight( 2 );
			if (columnHeightToRight > columnHeight)
			{
				sumOfWellHeights += (columnHeightToRight - columnHeight);
			}

			// Determine height of RIGHT well
            columnHeightToLeft = this.GetColumnHeight( this.mWidth - 1 );
            columnHeight = this.GetColumnHeight( this.mWidth );
			if (columnHeightToLeft > columnHeight)
			{
				sumOfWellHeights += (columnHeightToLeft - columnHeight);
			}


			// Now do all interior columns, 1..(width-1), which have TWO
			// adjacent lines...
			int  x = 0;
            for ( x = 2; x <= (this.mWidth - 1); x++ )
			{
				columnHeightToLeft  =  this.GetColumnHeight( x - 1 );
				columnHeight        =  this.GetColumnHeight( x     );
				columnHeightToRight =  this.GetColumnHeight( x + 1 );

				if (columnHeightToLeft > columnHeightToRight)
				{
					columnHeightToLeft = columnHeightToRight;
				}
				else
				{
					columnHeightToRight = columnHeightToLeft;
				}

				if (columnHeightToLeft > columnHeight)
				{
					sumOfWellHeights += (columnHeightToLeft - columnHeight);
				}
			}

			return( sumOfWellHeights );
		}






        public int GetTotalCompletedRows( ) // result range: 0..Height
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int totalCompletedRows = 0;

            int x = 0;
            int y = 0;

            // check each row
            for ( y = 1; y <= this.mHeight; y++ )
            {
                // check if this row is full.
                bool rowIsFull = true; // hypothesis
                for ( x = 1; ((x <= this.mWidth) && (true == rowIsFull)); x++ )
                {
                    byte cellValue = (byte)0;
                    cellValue = this.GetCell( x, y );
                    if ((byte)0 == cellValue)  
                    {
                        rowIsFull = false;
                    }
                }

                if (true == rowIsFull) 
                {
                    totalCompletedRows++;
                }
            }

            return (totalCompletedRows);
        }







        public int GetTransitionCountForRow( int y ) // result range: 0..Width
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int transitionCount = 0;

            int x = 0;
            byte cellA = (byte)0;
            byte cellB = (byte)0;

            // check cell and neighbor to right...
            for ( x = 1; x < this.mWidth; x++ )
            {
                cellA = this.GetCell( x, y );
                cellB = this.GetCell( (x + 1), y );

                // If a transition from occupied to unoccupied, or
                // from unoccupied to occupied, then it's a transition.
                if 
                (
                    (((byte)0 != cellA) && ((byte)0 == cellB)) ||
                    (((byte)0 == cellA) && ((byte)0 != cellB)) 
                )
                {
                    transitionCount++;
                }
            }

            // check transition between left-exterior and column 1.
            // (Note: exterior is implicitly "occupied".)
            cellA = this.GetCell( 1, y );
            if ((byte)0 == cellA) 
            {
                transitionCount++;
            }

            // check transition between column 'mWidth' and right-exterior.
            // (NOTE: Exterior is implicitly "occupied".)
            cellA = this.GetCell( this.mWidth, y );
            if ((byte)0 == cellA) 
            {
                transitionCount++;
            }

            return (transitionCount);
        }








        
        public int GetTransitionCountForColumn( int x ) // result range: 1..(Height + 1)
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int transitionCount = 0;

            int y = 0;
            byte cellA = (byte)0;
            byte cellB = (byte)0;


            // check cell and neighbor above...
            for ( y = 1; y < this.mHeight; y++ )
            {
                cellA = this.GetCell( x, y );
                cellB = this.GetCell( x, (y + 1) );

                // If a transition from occupied to unoccupied, or
                // from unoccupied to occupied, then it's a transition.
                if 
                (
                    (((byte)0 != cellA) && ((byte)0 == cellB)) ||
                    (((byte)0 == cellA) && ((byte)0 != cellB))
                )
                {
                    transitionCount++;
                }
            }

            // check transition between bottom-exterior and row Y=1.
            // (Note: Bottom exterior is implicitly "occupied".)
            cellA = this.GetCell( x, 1 );
            if ((byte)0 == cellA) 
            {
                transitionCount++;
            }

            // check transition between column 'mHeight' and above-exterior.
            // (Note: Sky above is implicitly UN-"occupied".)
            cellA = this.GetCell( x, this.mHeight );
            if ((byte)0 != cellA) 
            {
                transitionCount++;
            }

            return (transitionCount);
        }







        public int GetBuriedHolesForColumn( int x ) // result range: 0..(Height-1)
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int totalHoles = 0;
            byte cellValue = (byte)0;
            bool enable = false;
            int y = 0;

            for ( y = this.mHeight; y >= 1; y-- )
            {
                cellValue = this.GetCell( x, y );

                if ((byte)0 != cellValue)
                {
                    enable = true;
                }
                else
                {
                    if (true == enable)
                    {
                        totalHoles++;
                    }
                }
            }

            return (totalHoles);
        }








        public int GetBlanksDownBeforeBlockedForColumn // result range: 0..topY
        (
            int x,
            int topY 
        )
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int totalBlanksBeforeBlocked = 0;
            byte cellValue = (byte)0;
            int y = 0;

            for ( y = topY; y >= 1; y-- )
            {
                cellValue = this.GetCell( x, y );

                if ((byte)0 != cellValue)
                {
                    return (totalBlanksBeforeBlocked);
                }
                else
                {
                    totalBlanksBeforeBlocked++;
                }
            }

            return (totalBlanksBeforeBlocked);
        }






        public int GetAllWellsForColumn( int x ) // result range: 0..O(Height*mHeight)
        {
            if (null == this.mCells)
            {
                return (0);
            }

            int wellValue = 0;

            byte cellLeft = (byte)0;
            byte cellRight = (byte)0;
            int y = 0;

            for ( y = this.mHeight; y >= 1; y-- )
            {
                if ((x - 1) >= 1)
                {
                    cellLeft = this.GetCell( (x - 1), y );
                }
                else
                {
                    cellLeft = (byte)1;
                }

                if ((x + 1) <= this.mWidth)
                {
                    cellRight = this.GetCell( (x + 1), y );
                }
                else
                {
                    cellRight = (byte)1;
                }

                if (((byte)0 != cellLeft) && ((byte)0 != cellRight))
                {
                    int blanksDown = 0;
                    blanksDown = this.GetBlanksDownBeforeBlockedForColumn( x, y );
                    wellValue += blanksDown;
                }
            }

            return( wellValue );
        }








        public bool DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells
        (
            STPiece piece
        )
        {
            if (null == this.mCells)
            {
                return ( false );
            }

            if (false == piece.IsValid()) 
            {
                return( false );
            }

            // Fast check: If piece origin lies outside board, goal is not acceptable.
            if (piece.GetX() < 1) 
            {
                return( false );
            }

            if (piece.GetY() < 1) 
            {
                return( false );
            }

            if (piece.GetX() > this.GetWidth ()) 
            {
                return( false );
            }

            if (piece.GetY() > this.GetHeight()) 
            {
                return( false );
            }

            // Consider the board position of all cells of the piece.
            // If any of the piece cells lie outside the board, then the goal
            //   is not acceptable.
            // If any of the piece cells coincide with an occupied cell of the board,
            //   then the goal is not acceptable.
            int totalCells = 0;
            totalCells = piece.GetTotalCells( );

            int cellIndex = 0;
            for (cellIndex = 1; cellIndex <= totalCells; cellIndex++)
            {
                int boardX = 0;
                int boardY = 0;
                piece.GetTranslatedCellXY( cellIndex, ref boardX, ref boardY );

                // board-relative cell must be in the board area
                if (boardX < 1) 
                {
                return( false );
                }

                if (boardX > this.GetWidth( ))
                {
                    return (false);
                }

                if (boardY < 1)
                {
                    return (false);
                }

                if (boardY > this.GetHeight( )) 
                {
                    return (false);
                }

                // board-relative cell cannot overlap an occupied cell of the board.
                if ((byte)0 != this.GetCell( boardX, boardY ))
                {
                    return (false);
                }
            }

            // If we made it to this point, the goal is acceptable.
            return (true);
        }






        public void DetermineAccessibleTranslationsForPieceOrientation
        (
            STPiece piece,
            ref bool movePossible, // false == no moves possible
            ref int minDeltaX, // displacement to left-most limit
            ref int maxDeltaX // displacement to right-most limit
        )
        {
            movePossible = false; // false == no moves possible
            minDeltaX = 0; // displacement to left-most limit
            maxDeltaX = 0; // displacement to right-most limit


            if (null == this.mCells)
            {
                return;
            }

            if (false == piece.IsValid()) 
            {
                return;
            }


            // Get dimensions of board
            int width  = 0;
            width  = this.GetWidth();
            // int height = 0;
            // height = this.GetHeight();


            STPiece tempPiece = new STPiece();
            bool moveAcceptable = false;
            int trialTranslationDelta = 0;


            // Check if we can move at all.
            moveAcceptable =
                this.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells( piece );
            if (true == moveAcceptable)
            {
                movePossible = true;
            }
            else
            {
                return;
            }


            // Scan from center to left to find left limit.
            bool stillAcceptable = true;
            for 
            ( 
                trialTranslationDelta = 0;
                ( (trialTranslationDelta >= (-(width))) && (true == stillAcceptable) );
                trialTranslationDelta-- 
            )
            {
                // Copy piece to temp and translate
                tempPiece.CopyFrom( piece );
                tempPiece.Translate( trialTranslationDelta, 0 );

                moveAcceptable =
                    this.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells( tempPiece );

                if (true == moveAcceptable)
                {
                    minDeltaX = trialTranslationDelta;
                }
                else
                {
                    stillAcceptable = false;
                }
            }


            // Scan from center to right to find right limit.
            stillAcceptable = true;
            for 
            ( 
                trialTranslationDelta = 0;
                ( (trialTranslationDelta <= width) && (true == stillAcceptable) );
                trialTranslationDelta++ 
            )
            {
                // Copy piece to temp and translate
                tempPiece.CopyFrom( piece );
                tempPiece.Translate( trialTranslationDelta, 0 );

                moveAcceptable =
                  this.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells( tempPiece );

                if (true == moveAcceptable)
                {
                    maxDeltaX = trialTranslationDelta;
                }
                else
                {
                    stillAcceptable = false;
                }
            }
        }





        public void CommitPieceToBoard
        ( 
            STPiece piece
        )
        {

            if (null == this.mCells)
            {
                return;
            }

            if (false == piece.IsValid( ))
            {
                return;
            }


            // Fast check: If piece origin lies outside board, adding is not acceptable.
            if (piece.GetX( ) < 1) 
            {
                return;
            }

            if (piece.GetY( ) < 1) 
            {
                return;
            }

            if (piece.GetX( ) > this.GetWidth( )) 
            {
                return;
            }

            if (piece.GetY( ) > this.GetHeight( )) 
            {
                return;
            }

            // Consider the absolute position of all points of the piece, and set the
            // corresponding cells on the board.
            
            int boardX = 0;
            int boardY = 0;
            
            int totalCells = 0;
            totalCells = piece.GetTotalCells( );

            int cellIndex = 0;
            for (cellIndex = 1; cellIndex <= totalCells; cellIndex++)
            {
                piece.GetTranslatedCellXY( cellIndex, ref boardX, ref boardY );

                // Fill in cell on the board.
                // (Note: board will silently discard cells with invalid (x,y).)
                this.SetCell( boardX, boardY, piece.GetByteCodeValue() );
            }
        }







        public void DropPieceAsFarAsPossibleButDoNotModifyBoard
        ( 
            STPiece  piece
        )
        {
            if (null == this.mCells)
            {
                return;
            }

            if (false == piece.IsValid( ))
            {
                return;
            }


            // Special case: cannot place piece at starting location.
            bool goalAcceptable = false;

            goalAcceptable = 
                this.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells( piece );

            if (false == goalAcceptable)
            {
                // cannot drop piece at all
                return;
            }



            // Try successively larger drop distances, up to the point of failure.
            // The last successful drop distance becomes our drop distance.
            int boardHeight = 0;
            int lastSuccessfulDropDistance = 0;
            bool firstFailureEncountered = false;
            int trialDropDistance = 0;

            boardHeight = this.GetHeight();

            STPiece tempPiece = new STPiece();
            tempPiece.CopyFrom( piece );

            for 
            ( 
                trialDropDistance = 0;
                ( (false == firstFailureEncountered) && (trialDropDistance <= boardHeight) );
                trialDropDistance++ 
            )
            {
                // Set temporary piece to new trial Y
                tempPiece.SetY( piece.GetY( ) - trialDropDistance );

                goalAcceptable = 
                    this.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells( tempPiece );

                if (false == goalAcceptable)
                {
                    // We failed to drop this far.  Stop drop search.
                    firstFailureEncountered = true;
                }
                else
                {
                    lastSuccessfulDropDistance = trialDropDistance;
                }
            }

            // Simply update the piece Y value.
            piece.SetY( piece.GetY( ) - lastSuccessfulDropDistance );
        }







        // WARNING: The following method will unconditionally add the piece to
        // the board, even if putting the piece down at the starting location 
        // (let alone dropping) is not acceptable (i.e., overlaps occupied cells).
        public void FullDropAndCommitPieceToBoard
        ( 
            STPiece piece
        )
        {
            if (null == this.mCells)
            {
                return;
            }

            if (false == piece.IsValid( ))
            {
                return;
            }


            // Drop piece as far as it will go.  A drop of zero distance is possible.
            this.DropPieceAsFarAsPossibleButDoNotModifyBoard( piece );

            // Commit the translated piece to the board
            this.CommitPieceToBoard( piece );
        }








        // The following counts the number of cells (0..4) of a piece that would
        // be eliminated by dropping the piece.
        public int CountPieceCellsEliminated
        ( 
            STPiece piece
        )
        {
            if (null == this.mCells)
            {
                return(0);
            }

            if (false == piece.IsValid( ))
            {
                return(0);
            }

            // Copy piece and board so that this measurement is not destructive.
            STBoard copyOfBoard = new STBoard();
            copyOfBoard.CopyFrom( this );

            STPiece copyOfPiece = new STPiece();
            copyOfPiece.CopyFrom( piece );


            // Drop copy of piece on to the copy of the board
            copyOfBoard.FullDropAndCommitPieceToBoard( copyOfPiece );


            // Scan rows.  For each full row, check all board Y values for the
            // piece.  If any board Y of the piece matches the full row Y,
            // increment the total eliminated cells.
            int pieceCellsEliminated = 0;


            int width = 0;
            int height = 0;
            width = copyOfBoard.GetWidth( );
            height = copyOfBoard.GetHeight( );

            int y = 0;
            for (y = 1; y <= height; y++)
            {

                bool fullRow = false;
                fullRow = true; // hypothesis

                int x = 0;
                for (x = 1; ((x <= width) && (true == fullRow)); x++)
                {
                    byte cellValue = (byte)0;
                    cellValue = copyOfBoard.GetCell( x, y );
                    if ((byte)0 == cellValue) 
                    {
                        fullRow = false;     
                    }
                }

                if (true == fullRow)
                {
                    // Find any matching board-relative Y values in dropped copy of piece.
                    int totalCells = 0;
                    totalCells = piece.GetTotalCells( );

                    int cellIndex = 0;
                    for (cellIndex = 1; cellIndex <= totalCells; cellIndex++)
                    {
                        int boardX = 0;
                        int boardY = 0;
                        copyOfPiece.GetTranslatedCellXY( cellIndex, ref boardX, ref boardY );
                        if (boardY == y)  
                        {
                            pieceCellsEliminated++;  // Moohahahaaa!
                        }
                    }
                }
            }

            return (pieceCellsEliminated);
        }





    }
}
