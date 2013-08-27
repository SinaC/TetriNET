// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;



namespace CPF.StandardTetris
{
    public class STPiece
    {
        public enum STPieceShape
        {
            None = 0,
            O = 1,
            I = 2,
            S = 3,
            Z = 4,
            L = 5,
            J = 6,
            T = 7
        }

        private STPieceShape mShape;
        private int mX; // origin x; must be within the board
        private int mY; // origin y; must be within the board
        private int mOrientation; // 1,2,3,4 (0,1/4,2/4,3/4-"turn"); 0 == none


        private void InitializeAllFields ( )
        {
            this.mShape = STPieceShape.None;
            this.mX = 0;
            this.mY = 0;
            this.mOrientation = 0;
        }


        public STPiece ( )
        {
            this.InitializeAllFields( );
        }


        public void Clear ( )
        {
            this.InitializeAllFields( );
        }


        public void CopyFrom ( STPiece piece )
        {
            this.mShape = piece.mShape;
            this.mX = piece.mX;
            this.mY = piece.mY;
            this.mOrientation = piece.mOrientation;
        }




        public static int GetMaximumOrientationsOfShape
        (
            STPieceShape shape
        )
        {
            if (STPieceShape.O == shape) return( 1 );
            if (STPieceShape.I == shape) return( 2 );
            if (STPieceShape.S == shape) return( 2 );
            if (STPieceShape.Z == shape) return( 2 );
            if (STPieceShape.L == shape) return( 4 );
            if (STPieceShape.J == shape) return( 4 );
            if (STPieceShape.T == shape) return( 4 );
            if (STPieceShape.None == shape) return( 0 );

            return( 0 );
        }




        public STPieceShape GetShape()
        {
            return(this.mShape);
        }


        public static byte GetByteCodeValueOfShape
        (
            STPieceShape shape
        )
        {
            if (STPieceShape.O == shape) return ((byte)1);
            if (STPieceShape.I == shape) return ((byte)2);
            if (STPieceShape.S == shape) return ((byte)3);
            if (STPieceShape.Z == shape) return ((byte)4);
            if (STPieceShape.L == shape) return ((byte)5);
            if (STPieceShape.J == shape) return ((byte)6);
            if (STPieceShape.T == shape) return ((byte)7);
            if (STPieceShape.None == shape) return ((byte)0);
            return ((byte)0);
        }

        public static STPieceShape GetShapeCorrespondingToByteCode
        (
             byte shapeCode
        )
        {
            if ((byte)1 == shapeCode) return (STPieceShape.O);
            if ((byte)2 == shapeCode) return (STPieceShape.I);
            if ((byte)3 == shapeCode) return (STPieceShape.S);
            if ((byte)4 == shapeCode) return (STPieceShape.Z);
            if ((byte)5 == shapeCode) return (STPieceShape.L);
            if ((byte)6 == shapeCode) return (STPieceShape.J);
            if ((byte)7 == shapeCode) return (STPieceShape.T);
            if ((byte)0 == shapeCode) return (STPieceShape.None);
            return (STPieceShape.None);
        }


        public byte GetByteCodeValue()
        {
            if (STPieceShape.O == this.mShape) return ((byte)1);
            if (STPieceShape.I == this.mShape) return ((byte)2);
            if (STPieceShape.S == this.mShape) return ((byte)3);
            if (STPieceShape.Z == this.mShape) return ((byte)4);
            if (STPieceShape.L == this.mShape) return ((byte)5);
            if (STPieceShape.J == this.mShape) return ((byte)6);
            if (STPieceShape.T == this.mShape) return ((byte)7);
            if (STPieceShape.None == this.mShape) return ((byte)0);
            return ((byte)0);
        }


        public int GetX()
        {
            return(this.mX);
        }

        public int GetY()
        {
            return(this.mY);
        }

        public int GetOrientation()
        {
            int totalOrientations = 0;
            totalOrientations = STPiece.GetMaximumOrientationsOfShape(this.mShape);

            if (0 == totalOrientations)
            {
                this.mOrientation = 0;
                return(0);
            }

            this.mOrientation = 
                1 + ((((this.mOrientation - 1) % totalOrientations) 
                          + totalOrientations) % totalOrientations);

            return(this.mOrientation);
        }



        public void SetShape( STPieceShape shape )
        {
            this.mShape = shape;
        }

        public void SetX( int x )
        {
            this.mX = x;
        }

        public void SetY( int y )
        {
            this.mY = y;
        }

        public void SetOrientation( int orientation )
        {
            int totalOrientations = 0;
            totalOrientations = STPiece.GetMaximumOrientationsOfShape(this.mShape);

            if (0 == totalOrientations)
            {
                this.mOrientation = 0;
                return;
            }

            orientation = 
                1 + ((((orientation - 1) % totalOrientations) 
                          + totalOrientations) % totalOrientations);

            this.mOrientation = orientation;
        }




        public static void GetCellOffsetXYForShape 
        (
            STPieceShape shape, // O,I,S,Z,L,J,T; None
            int orientation, // 1,2,3,4; 0 == none
            int cellIndex, // 1,2,3,4; 0 == none
            ref int x,
            ref int y
        )
        {
            x = 0;
            y = 0;

            // force cellIndex to range 1..4
            cellIndex = 1 + (((cellIndex - 1) % 4 + 4) % 4);


            // force orientation to range 1..totalOrientations
            int totalOrientations = 0;
            totalOrientations = STPiece.GetMaximumOrientationsOfShape( shape );

            if (0 == totalOrientations)
            {
                return;
            }

            orientation =
                1 + ((((orientation - 1) % totalOrientations)
                          + totalOrientations) % totalOrientations);


            if (STPieceShape.None == shape)
            {
                return;
            }

            if (STPieceShape.O == shape)
            {
                // orientation 1,2,3,4 : (-1, -1),  ( 0, -1),  ( 0,  0),  (-1,  0)
                if (1 == cellIndex) { x = -1; y = -1; }
                else if (2 == cellIndex) { x = 0; y = -1; }
                else if (3 == cellIndex) { x = 0; y = 0; }
                else if (4 == cellIndex) { x = -1; y = 0; }
            }

            if (STPieceShape.I == shape)
            {
                // orientation 1,3: (-2,  0),  (-1,  0),  ( 0,  0),  ( 1,  0)
                // orientation 2,4: ( 0, -2),  ( 0, -1),  ( 0,  0),  ( 0,  1)
                if ((1 == orientation) || (3 == orientation))
                {
                    if (1 == cellIndex) { x = -2; y = 0; }
                    else if (2 == cellIndex) { x = -1; y = 0; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 0; }
                }
                else
                {
                    if (1 == cellIndex) { x = 0; y = -2; }
                    else if (2 == cellIndex) { x = 0; y = -1; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
            }

            if (STPieceShape.S == shape)
            {
                // orientation 1,3: (-1, -1),  ( 0, -1),  ( 0,  0),  ( 1,  0)
                // orientation 2,4: ( 1, -1),  ( 0,  0),  ( 1,  0),  ( 0,  1)
                if ((1 == orientation) || (3 == orientation))
                {
                    if (1 == cellIndex) { x = -1; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = -1; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 0; }
                }
                else
                {
                    if (1 == cellIndex) { x = 1; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 1; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
            }

            if (STPieceShape.Z == shape)
            {
                // orientation 1,3: ( 0, -1),  ( 1, -1),  (-1,  0),  ( 0,  0)
                // orientation 2,4: ( 0, -1),  ( 0,  0),  ( 1,  0),  ( 1,  1)
                if ((1 == orientation) || (3 == orientation))
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = 1; y = -1; }
                    else if (3 == cellIndex) { x = -1; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 0; }
                }
                else
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 1; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 1; }
                }
            }


            if (STPieceShape.L == shape)
            {
                // orientation 1: (-1, -1),  (-1,  0),  ( 0,  0),  ( 1,  0)
                // orientation 2: ( 0, -1),  ( 1, -1),  ( 0,  0),  ( 0,  1)
                // orientation 3: (-1,  0),  ( 0,  0),  ( 1,  0),  ( 1,  1)
                // orientation 4: ( 0, -1),  ( 0,  0),  (-1,  1),  ( 0,  1)
                if (1 == orientation)
                {
                    if (1 == cellIndex) { x = -1; y = -1; }
                    else if (2 == cellIndex) { x = -1; y = 0; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 0; }
                }
                else if (2 == orientation)
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = 1; y = -1; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
                else if (3 == orientation)
                {
                    if (1 == cellIndex) { x = -1; y = 0; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 1; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 1; }
                }
                else
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = -1; y = 1; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
            }


            if (STPieceShape.J == shape)
            {
                // orientation 1: ( 1, -1),  (-1,  0),  ( 0,  0),  ( 1,  0)
                // orientation 2: ( 0, -1),  ( 0,  0),  ( 0,  1),  ( 1,  1)
                // orientation 3: (-1,  0),  ( 0,  0),  ( 1,  0),  (-1,  1)
                // orientation 4: (-1, -1),  ( 0, -1),  ( 0,  0),  ( 0,  1)
                if (1 == orientation)
                {
                    if (1 == cellIndex) { x = 1; y = -1; }
                    else if (2 == cellIndex) { x = -1; y = 0; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 0; }
                }
                else if (2 == orientation)
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 0; y = 1; }
                    else if (4 == cellIndex) { x = 1; y = 1; }
                }
                else if (3 == orientation)
                {
                    if (1 == cellIndex) { x = -1; y = 0; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 1; y = 0; }
                    else if (4 == cellIndex) { x = -1; y = 1; }
                }
                else
                {
                    if (1 == cellIndex) { x = -1; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = -1; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
            }


            if (STPieceShape.T == shape)
            {
                // orientation 1: ( 0, -1),  (-1,  0),  ( 0,  0),  ( 1,  0)
                // orientation 2: ( 0, -1),  ( 0,  0),  ( 1,  0),  ( 0,  1)
                // orientation 3: (-1,  0),  ( 0,  0),  ( 1,  0),  ( 0,  1)
                // orientation 4: ( 0, -1),  (-1,  0),  ( 0,  0),  ( 0,  1)
                if (1 == orientation)
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = -1; y = 0; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 1; y = 0; }
                }
                else if (2 == orientation)
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 1; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
                else if (3 == orientation)
                {
                    if (1 == cellIndex) { x = -1; y = 0; }
                    else if (2 == cellIndex) { x = 0; y = 0; }
                    else if (3 == cellIndex) { x = 1; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
                else
                {
                    if (1 == cellIndex) { x = 0; y = -1; }
                    else if (2 == cellIndex) { x = -1; y = 0; }
                    else if (3 == cellIndex) { x = 0; y = 0; }
                    else if (4 == cellIndex) { x = 0; y = 1; }
                }
            }

        }



        public static int GetTotalCellsForShape ( STPieceShape shape ) 
        { 
            return (4); 
        }



        public int GetTotalCells ( ) 
        { 
            return (4); 
        }





        public static bool GetCellOffsetXYOccupiedForShape
          (
            STPieceShape shape, // O,I,S,Z,L,J,T; None
            int orientation, // 1,2,3,4; 0 == none
            int x,
            int y
          )
        {
            if (STPieceShape.None == shape)
            {
                return(false);
            }


            // force orientation to range 1..totalOrientations
            int totalOrientations = 0;
            totalOrientations = STPiece.GetMaximumOrientationsOfShape( shape );

            if (0 == totalOrientations)
            {
                return(false);
            }

            orientation =
                1 + ((((orientation - 1) % totalOrientations)
                          + totalOrientations) % totalOrientations);
                       

            // Loop through all cells and test against supplied cell.
            int totalCells = 0;
            totalCells = STPiece.GetTotalCellsForShape( shape );
            int cellIndex = 0;
            for ( cellIndex = 1; cellIndex <= totalCells; cellIndex++ )
            {
                int cellX = 0;
                int cellY = 0;
                STPiece.GetCellOffsetXYForShape( shape, orientation, cellIndex, ref cellX, ref cellY );
                if ( (x == cellX) && (y == cellY) )
                {
                    return(true);
                }
            }
            return(false);
        }


        public void Translate ( int dx, int dy )
        {
            this.mX += dx;
            this.mY += dy;
        }


        public bool IsValid ( )
        {
            if (STPieceShape.None == this.mShape) return (false);
            if ((this.mOrientation < 1) || (this.mOrientation > 4)) return (false);
            return (true);
        }






        public void Rotate()
        {
            // only rotate if currently valid
            if (false == this.IsValid()) return;

            // First, retrieve maximum non-redundant orientation values
            int totalOrientations = 0;
            totalOrientations = STPiece.GetMaximumOrientationsOfShape( this.mShape );
            if (0 == totalOrientations) return;

            // Add one to current orientation value
            int newOrientation = (this.mOrientation + 1);

            // Force newOrientation to range 1..totalOrientations
            newOrientation =
                1 + ((((newOrientation - 1) % totalOrientations)
                          + totalOrientations) % totalOrientations);

            this.mOrientation = newOrientation;
        }







        // essentially multiple Rotate() commands
        public void RotateByCount( int count )
        {
            int step  = 0;
            int total = 0;

            total = count;  // Copy number (can be negative, zero, or positive)
            total %= 4; // Modulo 4 (-3,-2,-1,0,1,2,3)
            total += 4; // Add    4 ( 1, 2, 3,4,5,6,7)
            total %= 4; // Modulo 4 ( 1, 2, 3,0,1,2,3)

            for (step = 0; step < total; step++)
            {
                this.Rotate( );
            }
        }







        public void GetTranslatedCellXY
        (
            int cellIndex, // 1,2,3,4; 0 == none
            ref int x,
            ref int y
        )
        {
            x = 0;
            y = 0;
            if (cellIndex < 1) return;
            if (cellIndex > 4) return;
            int cellX = 0;
            int cellY = 0;
            STPiece.GetCellOffsetXYForShape(this.mShape, this.mOrientation, cellIndex, ref cellX, ref cellY );
            x = this.mX + cellX;
            y = this.mY + cellY;
        }



        public void GetTranslatedBoundingRectangle
        (
            ref int minX,
            ref int minY,
            ref int maxX,
            ref int maxY
        )
        {
            minX = 0;
            minY = 0;
            maxX = 0;
            maxY = 0;

            if (false == this.IsValid()) return;

            int totalCells = this.GetTotalCells( );
            if (totalCells < 1) return;

            int x = 0;
            int y = 0;

            // start bounding limits using first cell
            this.GetTranslatedCellXY( 1, ref x, ref y ); // first cell
            minX = x;
            maxX = x;
            minY = y;
            maxY = y;

            // expand bounding limits with other cells
            int cellIndex = 0;
            for ( cellIndex = 1; cellIndex <= totalCells; cellIndex++ )
            {
                this.GetTranslatedCellXY( cellIndex, ref x, ref y );
                if (x < minX) minX = x;
                if (y < minY) minY = y;
                if (x > maxX) maxX = x;
                if (y > maxY) maxY = y;
            }
        }


    }
}
