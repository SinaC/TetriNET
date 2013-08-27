// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using CPF.GRV10;




namespace CPF.StandardTetris
{
    public class STGameDrawing
    {


        private static Color[] mPieceColors =
        {
            Color.FromArgb( 255,   0,   0,   0 ), // 0 : BLACK
            Color.FromArgb( 255, 255,   0,   0 ), // 1 : RED
            Color.FromArgb( 255, 255, 160,   0 ), // 2 : ORANGE
            Color.FromArgb( 255, 255, 255,   0 ), // 3 : YELLOW
            Color.FromArgb( 255,   0, 255,   0 ), // 4 : GREEN
            Color.FromArgb( 255,   0, 255, 255 ), // 5 : CYAN
            Color.FromArgb( 255,   0,   0, 255 ), // 6 : BLUE
            Color.FromArgb( 255, 255,   0, 255 ), // 7 : MAGENTA
            Color.FromArgb( 255,  64,  64,  64 )  // 8 : DARK GRAY
        };



        public static Color GetCellValueColorARGB
        (
            byte cellValue, // 0, 1..7, 8, ...
            bool monochromeMode
        )
        {
            if ((byte)0 == cellValue)
            {
                return (Color.FromArgb( 255, 0, 0, 0 ));
            }

            if (cellValue >= (byte)8)
            {
                if (true == monochromeMode)
                {
                    return (Color.FromArgb( 255, 255, 255, 255 ));
                }
                return (Color.FromArgb( 255, 64, 64, 64 ));
            }

            if (true == monochromeMode)
            {
                return (Color.FromArgb( 255, 255, 255, 255 ));
            }

            return (STGameDrawing.mPieceColors[(int)cellValue]);
        }





        public static void DrawPieceWithCurrentColor
        (
            GR gr,
            float originX,
            float originY,
            float cellWidth,
            float cellHeight,
            STPiece piece
        )
        {
            if (false == piece.IsValid( ))
            {
                return;
            }

            int cellIndex = 0;
            int totalCells = 0;

            totalCells = piece.GetTotalCells( );

            for (cellIndex = 1; cellIndex <= totalCells; cellIndex++)
            {
                int boardX = 0;
                int boardY = 0;

                piece.GetTranslatedCellXY( cellIndex, ref boardX, ref boardY );

                float cellMinX = 0.0f;
                float cellMaxX = 0.0f;
                float cellMinY = 0.0f;
                float cellMaxY = 0.0f;

                cellMinX = originX + (cellWidth * (float)(boardX - 1));
                cellMaxX = originX + (cellWidth * (float)(boardX));
                cellMinY = originY + (cellHeight * (float)(boardY - 1));
                cellMaxY = originY + (cellHeight * (float)(boardY));

                gr.glBegin( GR.GL_QUADS );
                gr.glVertex2f( cellMinX, cellMaxY );
                gr.glVertex2f( cellMinX, cellMinY );
                gr.glVertex2f( cellMaxX, cellMinY );
                gr.glVertex2f( cellMaxX, cellMaxY );
                gr.glEnd( );
            }
        }








        public static void DrawBoardAndNextPiece
        (
            GR gr,
            float drawingLeft,
            float drawingBottom,
            float drawingRight,
            float drawingTop,
            STBoard board,
            STPiece piece,
            bool shadowMode,
            bool monochromeMode,
            bool showNextPiece,
            STPiece.STPieceShape nextPieceShape, // None == No next piece
            STPiece pieceBestMove  // IN: Best move
        )
        {
            float x1 = 0.0f;
            float y1 = 0.0f;
            float x2 = 0.0f;
            float y2 = 0.0f;
            float red = 0.0f;
            float green = 0.0f;
            float blue = 0.0f;



            int boardWidthCells = 0;
            int boardHeightCells = 0;
            boardWidthCells = board.GetWidth( );
            boardHeightCells = board.GetHeight( );



            float targetAspectRatio = 0.0f;
            targetAspectRatio = (1.0f / 2.0f);
            if (boardHeightCells > 0)
            {
                targetAspectRatio = ((float)boardWidthCells / (float)boardHeightCells);
            }



            float fullDrawingWidth = 0.0f;
            float fullDrawingHeight = 0.0f;
            fullDrawingWidth = (drawingRight - drawingLeft);
            fullDrawingHeight = (drawingTop - drawingBottom);



            float targetDrawingWidth = 0.0f;
            float targetDrawingHeight = 0.0f;
            if (fullDrawingWidth > (targetAspectRatio * fullDrawingHeight))
            {
                // Fit to height
                targetDrawingWidth = (fullDrawingHeight * targetAspectRatio);
                targetDrawingHeight = fullDrawingHeight;
                x1 = drawingLeft + ((fullDrawingWidth - targetDrawingWidth) / 2.0f);
                y1 = drawingBottom;
                x2 = x1 + targetDrawingWidth;
                y2 = y1 + targetDrawingHeight;
            }
            else
            {
                // Fit to width
                targetDrawingWidth = fullDrawingWidth;
                targetDrawingHeight = (fullDrawingWidth / targetAspectRatio);
                x1 = drawingLeft;
                y1 = drawingBottom + ((fullDrawingHeight - targetDrawingHeight) / 2.0f);
                x2 = x1 + targetDrawingWidth;
                y2 = y1 + targetDrawingHeight;
            }



            // Grid Cell Pixel Dimensions
            float cellDeltaX = 0.0f;
            float cellDeltaY = 0.0f;
            if (true == showNextPiece)
            {
                cellDeltaX = ((x2 - x1) / (float)(boardWidthCells + 2));
                cellDeltaY = ((y2 - y1) / (float)(boardHeightCells + 4));
            }
            else
            {
                cellDeltaX = ((x2 - x1) / (float)(boardWidthCells));
                cellDeltaY = ((y2 - y1) / (float)(boardHeightCells));
            }



            // Board Location within 1:2 Region
            float xb1 = 0.0f;
            float yb1 = 0.0f;
            float xb2 = 0.0f;
            float yb2 = 0.0f;
            if (true == showNextPiece)
            {
                xb1 = x1 + (1.0f * cellDeltaX);
                xb2 = x1 + ((float)(boardWidthCells + 1) * cellDeltaX);
                yb1 = y1 + (0.5f * cellDeltaY);
                yb2 = y1 + ((0.5f + (float)(boardHeightCells)) * cellDeltaY);
            }
            else
            {
                xb1 = x1 + (0.0f * cellDeltaX);
                xb2 = x1 + ((float)(boardWidthCells) * cellDeltaX);
                yb1 = y1 + (0.0f * cellDeltaY);
                yb2 = y1 + ((float)(boardHeightCells) * cellDeltaY);
            }



            // Next Piece Location (only applicable if flag set)
            float xp1 = 0.0f;
            float yp1 = 0.0f;
            float xp2 = 0.0f;
            float yp2 = 0.0f;
            if (true == showNextPiece)
            {
                xp1 = x1 + ((float)(((boardWidthCells - 4) / 2) + 1) * cellDeltaX);
                xp2 = x1 + ((float)(((boardWidthCells - 4) / 2) + 5) * cellDeltaX);
                yp1 = y1 + (((float)(boardHeightCells + 1) + 0.5f) * cellDeltaY);
                yp2 = y1 + (((float)(boardHeightCells + 3) + 0.5f) * cellDeltaY);
            }



            // Draw black region for full 1:2 Region
            //gr.glColor3f( 0.0f, 0.0f, 0.0f );
            //gr.glBegin( GR.GL_QUADS );
            //gr.glVertex2f( x1, y2 );
            //gr.glVertex2f( x1, y1 );
            //gr.glVertex2f( x2, y1 );
            //gr.glVertex2f( x2, y2 );
            //gr.glEnd( );




            // Draw "Best Move" Piece 

            if (true == pieceBestMove.IsValid( ))
            {
                red = 0.30f;
                green = 0.45f;
                blue = 0.55f;

                gr.glColor3f( red, green, blue );

                STGameDrawing.DrawPieceWithCurrentColor
                (
                    gr,
                    xb1,
                    yb1,
                    cellDeltaX,
                    cellDeltaY,
                    pieceBestMove
                );
            }





            // Draw Piece Shadow  (Do this BEFORE drawing the piece!)

            if (true == shadowMode)
            {
                if (true == piece.IsValid( ))
                {
                    // Form a "shadow" piece, which is simply a copy of the current
                    // piece, but dropped on to the pile.
                    STPiece shadowPiece = new STPiece( );
                    shadowPiece.CopyFrom( piece );
                    board.DropPieceAsFarAsPossibleButDoNotModifyBoard( shadowPiece );

                    red = 0.25f;
                    green = 0.25f;
                    blue = 0.30f;

                    gr.glColor3f( red, green, blue );

                    STGameDrawing.DrawPieceWithCurrentColor
                    (
                        gr,
                        xb1,
                        yb1,
                        cellDeltaX,
                        cellDeltaY,
                        shadowPiece
                    );
                }
            }








            // Draw Piece

            if (true == piece.IsValid( ))
            {
                // Get color for this piece
                Color color =
                STGameDrawing.GetCellValueColorARGB
                (
                    piece.GetByteCodeValue( ),
                    monochromeMode
                );

                gr.glColor3f( ((float)color.R) / 255.0f, ((float)color.G) / 255.0f, ((float)color.B) / 255.0f );

                STGameDrawing.DrawPieceWithCurrentColor
                (
                    gr,
                    xb1,
                    yb1,
                    cellDeltaX,
                    cellDeltaY,
                    piece
                );
            }





            // Draw Board


            // Draw occupied cells

            float x = 0.0f;
            float y = 0.0f;

            int cellXIndex = 0;
            int cellYIndex = 0;

            for (cellYIndex = 1; cellYIndex <= boardHeightCells; cellYIndex++)
            {
                y = yb1 + (cellDeltaY * (float)(cellYIndex - 1));
                for (cellXIndex = 1; cellXIndex <= boardWidthCells; cellXIndex++)
                {
                    x = xb1 + (cellDeltaX * (float)(cellXIndex - 1));

                    byte cellValue = board.GetCell( cellXIndex, cellYIndex );

                    if ((byte)0 != cellValue)
                    {
                        Color color =
                            STGameDrawing.GetCellValueColorARGB
                            (
                                cellValue,
                                monochromeMode
                            );

                        gr.glColor3f( ((float)color.R) / 255.0f, ((float)color.G) / 255.0f, ((float)color.B) / 255.0f );

                        gr.glBegin( GR.GL_QUADS );
                        gr.glVertex2f( x, y + cellDeltaY );
                        gr.glVertex2f( x, y );
                        gr.glVertex2f( x + cellDeltaX, y );
                        gr.glVertex2f( x + cellDeltaX, y + cellDeltaY );
                        gr.glEnd( );
                    }
                }
            }



            // Draw Grid
            int maxDimensionCells = 0;
            maxDimensionCells = boardWidthCells;
            if (boardHeightCells > maxDimensionCells)
            {
                maxDimensionCells = boardHeightCells;
            }

            float intensity = 0.0f;
            intensity = 0.3f * (1.0f - ((float)(maxDimensionCells) / 100.0f));
            if (intensity < 0.0f) intensity = 0.0f;
            if (intensity > 1.0f) intensity = 1.0f;

            float alpha = 0.0f;
            alpha = 0.6f * (1.0f - ((float)(maxDimensionCells) / 200.0f));
            if (alpha < 0.0f) alpha = 0.0f;
            if (alpha > 1.0f) alpha = 1.0f;

            gr.glColor4f( intensity, intensity, intensity, alpha );


            gr.glEnable( GR.GL_BLEND );
            gr.glBlendFunc( GR.GL_SRC_ALPHA, GR.GL_ONE_MINUS_SRC_ALPHA );
            gr.glBegin( GR.GL_LINES );

            // Vertical Lines
            for (cellXIndex = 0; cellXIndex <= boardWidthCells; cellXIndex++)
            {
                x = xb1 + (cellDeltaX * (float)(cellXIndex));
                gr.glVertex2f( x, yb1 );
                gr.glVertex2f( x, yb2 );
            }

            // Horizontal Lines
            for (cellYIndex = 0; cellYIndex <= boardHeightCells; cellYIndex++)
            {
                y = yb1 + (cellDeltaY * (float)(cellYIndex));
                gr.glVertex2f( xb1, y );
                gr.glVertex2f( xb2, y );
            }

            gr.glEnd( );
            gr.glDisable( GR.GL_BLEND );




            // Border Lines
            gr.glColor3f( 0.5f, 0.5f, 0.5f );
            gr.glBegin( GR.GL_LINES );

            cellXIndex = 0;
            x = xb1 + (cellDeltaX * (float)(cellXIndex));
            gr.glVertex2f( x, yb1 );
            gr.glVertex2f( x, yb2 );

            cellXIndex = boardWidthCells;
            x = xb1 + (cellDeltaX * (float)(cellXIndex));
            gr.glVertex2f( x, yb1 );
            gr.glVertex2f( x, yb2 );

            cellYIndex = 0;
            y = yb1 + (cellDeltaY * (float)(cellYIndex));
            gr.glVertex2f( xb1, y );
            gr.glVertex2f( xb2, y );

            cellYIndex = boardHeightCells;
            y = yb1 + (cellDeltaY * (float)(cellYIndex));
            gr.glVertex2f( xb1, y );
            gr.glVertex2f( xb2, y );

            gr.glEnd( );




            // Draw "Next Piece" if applicable

            if (true == showNextPiece)
            {
                // Draw occupied cells of "Next Piece"
                for (cellYIndex = 1; cellYIndex <= 2; cellYIndex++)
                {
                    y = yp1 + (cellDeltaY * (float)(cellYIndex - 1));

                    for (cellXIndex = 1; cellXIndex <= 4; cellXIndex++)
                    {
                        x = xp1 + (cellDeltaX * (float)(cellXIndex - 1));

                        bool cellOccupied =
                            STPiece.GetCellOffsetXYOccupiedForShape
                            (
                                nextPieceShape,
                                1, // Orientation ("1" is default)
                                (cellXIndex - 3), // Access using X offset (-3)
                                (cellYIndex - 2)  // Access using Y offset (-2)
                            );

                        if (true == cellOccupied)
                        {
                            Color color =
                                STGameDrawing.GetCellValueColorARGB // Returns WHITE for unknown
                                (
                                    STPiece.GetByteCodeValueOfShape( nextPieceShape ),
                                    monochromeMode
                                );

                            gr.glColor3f( ((float)color.R) / 255.0f, ((float)color.G) / 255.0f, ((float)color.B) / 255.0f );

                            gr.glBegin( GR.GL_QUADS );
                            gr.glVertex2f( x, y + cellDeltaY );
                            gr.glVertex2f( x, y );
                            gr.glVertex2f( x + cellDeltaX, y );
                            gr.glVertex2f( x + cellDeltaX, y + cellDeltaY );
                            gr.glEnd( );
                        }
                    }
                }


                // Draw Grid

                gr.glColor3f( 0.0f, 0.0f, 0.0f );
                gr.glBegin( GR.GL_LINES );

                // Vertical Lines
                for (cellXIndex = 0; cellXIndex <= 4; cellXIndex++)
                {
                    x = xp1 + (cellDeltaX * (float)(cellXIndex));
                    gr.glVertex2f( x, yp1 );
                    gr.glVertex2f( x, yp2 );
                }

                // Horizontal Lines
                for (cellYIndex = 0; cellYIndex <= 2; cellYIndex++)
                {
                    y = yp1 + (cellDeltaY * (float)(cellYIndex));
                    gr.glVertex2f( xp1, y );
                    gr.glVertex2f( xp2, y );
                }

                gr.glEnd( );
            }

        }



















        public static void DrawGameBoard
          (
            GR gr,
          float xmin,
          float ymin,
          float xmax,
          float ymax,
            STGame game
          )
        {
            STGameState gameState = game.GetGameState( );

            if (true == gameState.mCalibrationModeFlag)
            {
                return;
            }


            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor
              (
              (int)(xmin) - 4,
              (int)(ymin) - 4,
              (int)((xmax - xmin) + 1) + 8,
              (int)((ymax - ymin) + 1) + 8
              );



            STBoard copyOfCurrentBoard = new STBoard();
            game.GetCopyOfCurrentBoard( copyOfCurrentBoard );
            STPiece copyOfCurrentPiece = new STPiece();
            game.GetCopyOfCurrentPiece( copyOfCurrentPiece );

            STPiece nextPiece = new STPiece();
            game.GetCopyOfNextPiece( nextPiece );
            STPiece.STPieceShape nextPieceShape = STPiece.STPieceShape.None;
            nextPieceShape = nextPiece.GetShape( );

            STPiece pieceBestMove = new STPiece();
            game.GetCopyOfBestPiece( pieceBestMove );


            DrawBoardAndNextPiece
              (
              gr,
              xmin, // float left,
              ymin, // float bottom,
              xmax, // float right,
              ymax, // float top,
              copyOfCurrentBoard,
              copyOfCurrentPiece,
              game.GetGameShadowMode( ), // shadow mode
              gameState.mMonochromeColorMode,
              game.GameIsShowNextPiece( ), // show next piece
              nextPieceShape,
              pieceBestMove
              );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }












        public static void DrawCalibrationBoard
          (
            GR gr,
          float xMin,
          float yMin,
          float xMax,
          float yMax,
            STGame game
          )
        {
            STGameState gameState = game.GetGameState( );

            if (false == gameState.mCalibrationModeFlag)
            {
                return;
            }

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor
              (
              (int)(xMin) - 4,
              (int)(yMin) - 4,
              (int)((xMax - xMin) + 1) + 8,
              (int)((yMax - yMin) + 1) + 8
              );

            // Get a copy of the current board, just to get a board with the 
            // proper dimensions -- and then clear the copy.
            STBoard tempBoard = new STBoard( );
            game.GetCopyOfCurrentBoard( tempBoard );
            tempBoard.ClearCells( );

            STPiece trainingPiece = new STPiece( );
            trainingPiece.SetShape( STPiece.GetShapeCorrespondingToByteCode( (byte)gameState.mCalibrationModeShapeCode ) );
            trainingPiece.SetX( tempBoard.GetPieceSpawnX( ) );
            trainingPiece.SetY( tempBoard.GetPieceSpawnY( ) );
            trainingPiece.SetOrientation( 1 );

            tempBoard.CommitPieceToBoard( trainingPiece );

            STPiece dummyPiece = new STPiece( );

            DrawBoardAndNextPiece
              (
              gr,
              xMin, // float left,
              yMin, // float bottom,
              xMax, // float right,
              yMax, // float top,
              tempBoard,
              dummyPiece, // No shadow, so piece is not necessary
              false, // shadowMode
              false, // monochromeMode
              game.GameIsShowNextPiece( ), // show next piece
              trainingPiece.GetShape( ), // For training purpose, make the next piece the same as the piece        
              dummyPiece // bestMovePiece
              );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }


















        public static void DrawStatusPane
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax,
                STGame game,
                STConsole console
            )
        {
            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            // Draw Text
            STOpenGLFont.FontPrint( gr, x, y, "level    " + game.GetCurrentLevel( ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "rows     " + game.GetCompletedRows( ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "score    " + game.GetScore( ) );

            y += dy;
            y += dy;

            if (true == game.GameIsFinished( ))
            {
                gr.glColor3f( 1.0f, 1.0f, 1.0f );
                STOpenGLFont.FontPrint( gr, x, y, "GAME OVER" );
                gr.glColor3f( 1.0f, 1.0f, 1.0f );
            }
            y += dy;

            if (true == game.GameIsPaused( ))
            {
                gr.glColor3f( 0.8f, 0.8f, 0.8f );
                STOpenGLFont.FontPrint( gr, x, y, "PAUSED" );
                gr.glColor3f( 1.0f, 1.0f, 1.0f );
            }
            y += dy;

            gr.glColor3f( 0.5f, 0.5f, 0.5f );
            STOpenGLFont.FontPrint( gr, x, y += dy, "quit           esc" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "new game       enter" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "rotate         up" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "left           left" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "right          right" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "drop           down" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "instructions   i" );

            y += dy;

            String text = "";


            text = "pause          p ";
            if (true == game.GameIsPaused( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );


            text = "next           n ";
            if (true == game.GameIsShowNextPiece( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );



            text = "AI             a ";
            if (true == game.GameIsAI( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );



            text = "rs232          t ";
            if (true == game.GameIsOutputToRS232( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );


            text = "video          v ";
            if (true == game.GameIsSpawnFromVideoCapture( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );



            text = "calibrate      c ";
            if (true == game.GameIsCalibrationMode())
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );



            text = "auto reset     u ";
            if (true == game.GameIsAutoRestart( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );



            text = "auto file      f ";
            if (true == game.GameIsAutoWriteFile( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );


            text = "animate        x ";
            if (true == game.GameIsAnimateMoves( ))
            {
                text += "[*]";
            }
            else
            {
                text += "[ ]";
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, text );


            if (true == game.GameIsSpawnFromVideoCapture( ))
            {
                // Draw statistics pane in remaining space!
                DrawStatisticsPane
                (
                    gr,
                    xmin,
                    ymin,
                    xmax,
                    (int)(y + 2 * dy), // Squish so that statistics appears below status text!
                    game,
                    console
                );
            }

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }











        public static void DrawConsolePane
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax,
            STConsole console
            )
        {
            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            // Draw Text of Console
            int totalLines = 0;
            totalLines = console.GetTotalLines( );

            int lineIndex = 0;
            for (lineIndex = 0; lineIndex < totalLines; lineIndex++)
            {
                STOpenGLFont.FontPrint( gr, x, y += dy, console.GetLineByIndex( lineIndex ) );
            }

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }









        private static long previousRowCount = 0;
        private static double previousTime = 0.0;
        private static long deltaRowCount = 0;
        private static double deltaTime = 0.0;




        public static void DrawStatisticsPane
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax,
                STGame game,
                STConsole console
            )
        {
            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            gr.glColor3f( 0.5f, 0.5f, 0.5f );
            STOpenGLFont.FontPrint( gr, x, y, "O         " + game.GetPieceShapeHistogramBinValue( 1 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "I         " + game.GetPieceShapeHistogramBinValue( 2 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "S         " + game.GetPieceShapeHistogramBinValue( 3 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "Z         " + game.GetPieceShapeHistogramBinValue( 4 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "L         " + game.GetPieceShapeHistogramBinValue( 5 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "J         " + game.GetPieceShapeHistogramBinValue( 6 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "T         " + game.GetPieceShapeHistogramBinValue( 7 ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "sum       " + game.GetPieceShapeHistogramSum( ) );

            y += dy;

            STOpenGLFont.FontPrint( gr, x, y += dy, "rows      " + game.GetCompletedRows( ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "time         " + String.Format( "{0:f3}", game.GetTotalGameTime( ) ) + " s" );

            long currentRowCount = 0;
            double currentTime = 0.0;
            currentTime = game.GetTotalGameTime( );
            currentRowCount = game.GetCompletedRows( );

            double rowsPerSecond = 0.0;
            if (currentTime > 0.001)
            {
                rowsPerSecond = (double)(currentRowCount) / currentTime;
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, "rows/s       " + String.Format( "{0:f3}", rowsPerSecond ) );




            if (currentTime < 1.0)
            {
                previousRowCount = 0;
                previousTime = 0.0;
                deltaRowCount = 0;
                deltaTime = 0.0;
            }

            if (currentTime >= (previousTime + 1.0))
            {
                deltaRowCount = (currentRowCount - previousRowCount);
                deltaTime = (currentTime - previousTime);
                previousRowCount = currentRowCount;
                previousTime = currentTime;
            }

            double rowsPerSecondPreviousSecond = 0.0;
            if (deltaTime > 0.001)
            {
                rowsPerSecondPreviousSecond = (double)(deltaRowCount) / deltaTime;
            }
            STOpenGLFont.FontPrint( gr, x, y += dy, "rows/s (1s)  " + String.Format( "{0:f3}", rowsPerSecondPreviousSecond ) );



            y += dy;

            String text = "";
            text = "board          " + game.GetBoardWidth( ) + " x " + game.GetBoardHeight( );
            STOpenGLFont.FontPrint( gr, x, y += dy, text );
            STOpenGLFont.FontPrint( gr, x, y += dy, "speed bias    " + game.GetGameSpeedAdjustment( ) );
            STOpenGLFont.FontPrint( gr, x, y += dy, "frame rate    " + String.Format( "{0:f3}", game.GetReportedFrameRate( ) ) + " f/s" );

            y += dy;


            if (true == game.GameIsAI( ))
            {
                text = "AI " + STStrategyManager.GetCurrentStrategyName( );
                STOpenGLFont.FontPrint( gr, x, y += dy, text );
                y += dy;

                STOpenGLFont.FontPrint( gr, x, y += dy, "total games     " + game.GetHistoricTotalGames( ) );
                STOpenGLFont.FontPrint( gr, x, y += dy, "average rows    " + game.GetHistoricAverageRows( ) );
                STOpenGLFont.FontPrint( gr, x, y += dy, "high rows       " + game.GetHistoricHighRows( ) );
            }


            y += dy;

            long n = 0;
            n = game.GetHistoricTotalGames( );
            if (n > 0)
            {
                if (n > 10)
                {
                    n = 10;
                }
                STOpenGLFont.FontPrint( gr, x, y += dy, "previous games" );
                long i = 0;
                for (i = (n - 1); i >= 0; i--)
                {
                    text = "rows #" + (n - i) + " " + game.GetHistoricRowsBinValue( (int)i );
                    STOpenGLFont.FontPrint( gr, x, y += dy, text );
                }
            }

            gr.glDisable( GR.GL_SCISSOR_TEST );


            // Draw Console in remaining space
            DrawConsolePane
            (
                gr,
                (int)(x),
                ymin,
                xmax,
                (int)(y),
                console
            );
        }











        public static void DrawInstructions_ControlsPageA
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax
            )
        {
            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                           Controls                                        " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "rotate          up                      " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "left            left                    " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "right           right                   " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "drop            down, space             " );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "pause           p            [toggle]   " );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "new game        enter                   New random piece sequence" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "restart game    shift-r                 Start game with same piece sequence" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "total reset     shift-enter             Conditions similar to app startup" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "write file      shift-w                 Saves game to c:\\tetris_state_*.txt" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "load  file      shift-L                 Load a previously-saved game" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "instructions    i                                                          " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "shadow mode     s            [toggle]   Draws shadow where piece will land" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "hint mode       shift-h      [toggle]   Show current AI best move" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "color scheme    shift-k      [toggle]   Change color scheme" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "animate move    x            [toggle]   AI moves in slow-motion" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "junk row        shift-j                 Add random junk row under pile" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "S/Z pieces      z            [toggle]   Pieces alternate between S and Z" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                     [ Hit any key to exit menu ]            Down/Page-Down" );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }











        public static void DrawInstructions_ControlsPageB
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax
            )
        {
            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                           Controls (continued)                            " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "AI              a            [toggle]   Current AI will play" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "switch AI       shift-a      [cycle ]   Switch to next AI type" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "auto-restart    u            [toggle]   New game after current game ends" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "auto-write      f            [toggle]   Game written to file upon ending" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "speed up        +            [slider]   More  game iterations per render   " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "slow down       -            [slider]   Fewer game iterations per render   " );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "larger board    page-up      [slider]   Make board larger  (1 2 W H ratio)" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "smaller board   page-down    [slider]   Make board smaller (1 2 W H ratio)" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "more width      control-right[slider]   Increase Board Width" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "less width      control-left [slider]   Decrease Board Width" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "more height     control-up   [slider]   Increase Board Height" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "less height     control-down [slider]   Decrease Board Height" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "video capture   v            [toggle]   Web camera supplies pieces" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "calibrate       c            [toggle]   1..7   Show piece to test camera" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "video b/w       b            [toggle]   Process video as grayscale" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "rs-232 output   t            [toggle]   Movements transmitted on COM1" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "debug console   shift-q      [toggle]   Show read-only debugging console" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "Up/Page-Up           [ Hit any key to exit menu ]            Down/Page-Down" );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }











        public static void DrawInstructions_Credits
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax
            )
        {
            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                           Credits                                         " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "All design and programming by Colin Fahey.                                 " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "      official web site:   http://colinfahey.com                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "This application contains adaptations of the following algorithms from     " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "other sources:                                                             " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "      best real-time, 1-piece Tetris AI in the public domain:              " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "          Pierre Dellacherie (France, 2003)                                " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "          dellache@club-internet.fr                                        " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "      another real-time, 1-piece Tetris AI in the public domain:           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "          Roger Espel Llima (France?)                                      " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "                                                                           " );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "---------------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "Up/Page-Up           [ Hit any key to exit menu ]                          " );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }









        public static void DrawFileList
          (
            GR gr,
            int xmin,
            int ymin,
            int xmax,
            int ymax,
            STGame game
          )
        {
            STGameState gameState = game.GetGameState( );


            // Screen was tuned to 640 x 480 size, so center the rendering of
            // instruction screens if the window dimensions are bigger.
            int width = (xmax - xmin);
            int height = (ymax - ymin);
            if (width > 640)
            {
                xmin = ((width - 640) / 2);
                xmax = xmin + 640;
            }
            if (height > 480)
            {
                ymin = ((height - 480) / 2);
                ymax = ymin + 480;
            }


            float x = 0.0f;
            float y = 0.0f;
            float dy = (-14.0f);

            x = (float)(xmin);
            y = (float)(ymax);

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( xmin, ymin, ((xmax - xmin) + 1), ((ymax - ymin) + 1) );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );




            String directoryPath = "";
            directoryPath = STEngine.GetFileList().GetDirectoryPath();

            STOpenGLFont.FontPrint( gr, x, y, "-----------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "file list" );
            STOpenGLFont.FontPrint( gr, x, y += dy, directoryPath );
            STOpenGLFont.FontPrint( gr, x, y += dy, "-----------------------------------------------------------------------" );

            STOpenGLFont.FontPrint( gr, x, y += dy, " " );

            int totalFiles = 0;
            totalFiles = STEngine.GetFileList().GetTotalItems( );


            if (0 == totalFiles)
            {
                STOpenGLFont.FontPrint( gr, x, y += dy, "   [no tetris_state_*.txt files were found.]" );
                STOpenGLFont.FontPrint( gr, x, y += dy, " " );
            }
            else
            {
                int itemsPerPage = 20;

                gameState.mRelativeItem %= itemsPerPage; // Modulo items per page
                gameState.mRelativeItem += itemsPerPage; // Fix negative case
                gameState.mRelativeItem %= itemsPerPage; // Modulo items per page

                if (gameState.mFirstItem > (totalFiles - 1))
                {
                    gameState.mFirstItem = (totalFiles - 1);
                }

                if (gameState.mFirstItem < 0)
                {
                    gameState.mFirstItem = 0;
                }

                if ((gameState.mRelativeItem + gameState.mFirstItem) > (totalFiles - 1))
                {
                    gameState.mRelativeItem--;
                }

                int pageRelativeItemIndex = 0;

                for 
                    (
                        pageRelativeItemIndex = 0;
                        pageRelativeItemIndex < itemsPerPage;
                        pageRelativeItemIndex++
                    )
                {
                    int indexList = 0;

                    indexList = (pageRelativeItemIndex + gameState.mFirstItem);

                    if ((indexList >= 0) && (indexList < totalFiles))
                    {
                        String fileName = "";
                        fileName = STEngine.GetFileList().GetItemNameByIndex( indexList );
                        if (pageRelativeItemIndex == gameState.mRelativeItem)
                        {
                            if (true == gameState.mLoadFlag)
                            {
                                string filePathAndName = "";
                                filePathAndName = STEngine.GetFileList().GetItemFullPathAndNameByIndex( indexList );
                                gameState.mShowFileList = false;
                                gameState.mLoadFlag = false;
                                if (filePathAndName.Length > 0)
                                {
                                    game.LoadGameStateFromFile( filePathAndName );
                                }
                            }
                            gr.glColor3f( 1.0f, 1.0f, 0.0f );
                        }
                        else
                        {
                            gr.glColor3f( 1.0f, 1.0f, 1.0f );
                        }

                        STOpenGLFont.FontPrint( gr, x, y += dy, "  [" + (indexList + 1) + "] \"" + fileName + "\"" );
                        gr.glColor3f( 1.0f, 1.0f, 1.0f );
                    }
                    else
                    {
                        STOpenGLFont.FontPrint( gr, x, y += dy, " " );
                    }
                }
            }



            STOpenGLFont.FontPrint( gr, x, y += dy, "-----------------------------------------------------------------------" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "   next file:   down                  previous file:   up     " );
            STOpenGLFont.FontPrint( gr, x, y += dy, "   next page:   page-down             previous page:   page-up" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "   load     :   enter" );
            STOpenGLFont.FontPrint( gr, x, y += dy, "   exit menu:   any other key" );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }











        public static void DrawInstructions
            (
                GR gr,
                int xmin,
                int ymin,
                int xmax,
                int ymax,
                STGame game
            )
        {
            STGameState gameState = game.GetGameState( );


            // Screen was tuned to 640 x 480 size, so center the rendering of
            // instruction screens if the window dimensions are bigger.
            int width = (xmax - xmin);
            int height = (ymax - ymin);
            if (width > 640)
            {
                xmin = ((width - 640) / 2);
                xmax = xmin + 640;
            }
            if (height > 480)
            {
                ymin = ((height - 480) / 2);
                ymax = ymin + 480;
            }


            if (gameState.mShowInstructionPage < 1)
            {
                gameState.mShowInstructionPage = 1;
            }
            if (gameState.mShowInstructionPage > 3)
            {
                gameState.mShowInstructionPage = 3;
            }

            switch (gameState.mShowInstructionPage)
            {
                case 1:
                    {
                        DrawInstructions_ControlsPageA( gr, xmin, ymin, xmax, ymax );
                    }
                    break;
                case 2:
                    {
                        DrawInstructions_ControlsPageB( gr, xmin, ymin, xmax, ymax );
                    }
                    break;
                case 3:
                    {
                        DrawInstructions_Credits( gr, xmin, ymin, xmax, ymax );
                    }
                    break;
            }
        }












        public static void DrawScreen
            (
            int clientWidth,
            int clientHeight,
            int clientRelativeCursorX,
            int clientRelativeCursorY,
            GR gr,
            STGame game,
            STConsole console
            )
        {
            STGameState gameState = game.GetGameState( );


            // Master Frame has desired aspect ratio within the client area
            float masterFrameAspectRatio = (3.0f / 2.0f);
            float masterFrameWidth = 0.0f;
            float masterFrameHeight = 0.0f;
            float masterFrameX = 0.0f;
            float masterFrameY = 0.0f;

            // Master Sheet is a scaled down version of the frame (just to
            // have some border), centered within the frame area.
            float masterSheetToFrameScale = 0.95f;
            float masterSheetWidth = 0.0f;
            float masterSheetHeight = 0.0f;
            float masterSheetX = 0.0f;
            float masterSheetY = 0.0f;

            // Stats Frame is the first horizontal 1/3rd of the sheet.
            float statsFrameWidth = 0.0f;
            float statsFrameHeight = 0.0f;
            float statsFrameX = 0.0f;
            float statsFrameY = 0.0f;

            // Stats Sheet is a scaled down version of the Stats Frame
            // (just to have some border), centered within the Stats Frame area.
            float statsSheetToFrameScale = 0.95f;
            float statsSheetWidth = 0.0f;
            float statsSheetHeight = 0.0f;
            float statsSheetX = 0.0f;
            float statsSheetY = 0.0f;

            // Board Frame is the second horizontal 1/3rd of the sheet.
            float boardFrameWidth = 0.0f;
            float boardFrameHeight = 0.0f;
            float boardFrameX = 0.0f;
            float boardFrameY = 0.0f;

            // Board Sheet is a scaled down version of the Board Frame
            // (just to have some border), centered within the Board Frame area.
            float boardSheetToFrameScale = 0.95f;
            float boardSheetWidth = 0.0f;
            float boardSheetHeight = 0.0f;
            float boardSheetX = 0.0f;
            float boardSheetY = 0.0f;

            // Video Frame is the third horizontal 1/3rd of the sheet.
            float videoFrameWidth = 0.0f;
            float videoFrameHeight = 0.0f;
            float videoFrameX = 0.0f;
            float videoFrameY = 0.0f;

            // Video Sheet is a scaled down version of the Video Frame
            // (just to have some border), centered within the Video Frame area.
            float videoSheetToFrameScale = 0.95f;
            float videoSheetWidth = 0.0f;
            float videoSheetHeight = 0.0f;
            float videoSheetX = 0.0f;
            float videoSheetY = 0.0f;



            // You MUST do this every frame, because sometimes, after
            // a WM_SIZE, the client rectangle doesn't seem correct
            // by the time we get here.
            gr.glViewport( 0, 0, clientWidth, clientHeight );

            gr.glMatrixMode( GR.GL_PROJECTION );
            gr.glLoadIdentity( );

            gr.glOrtho
              (
              0.0f, (float)(clientWidth),
              0.0f, (float)(clientHeight),
              -1.0f, 1.0f
              );

            gr.glMatrixMode( GR.GL_MODELVIEW );
            gr.glLoadIdentity( );



            // Determine location and extent of Master Frame
            masterFrameAspectRatio = (3.0f / 2.0f);
            masterFrameWidth = 0.0f;
            masterFrameHeight = 0.0f;
            masterFrameX = 0.0f;
            masterFrameY = 0.0f;

            if ((float)clientWidth >= (masterFrameAspectRatio * (float)clientHeight))
            {
                // Fit to height
                masterFrameWidth = ((float)clientHeight * masterFrameAspectRatio);
                masterFrameHeight = (float)clientHeight;
                masterFrameX = 0.0f + (((float)clientWidth - masterFrameWidth) / 2.0f);
                masterFrameY = 0.0f;
            }
            else
            {
                // Fit to width
                masterFrameWidth = (float)clientWidth;
                masterFrameHeight = ((float)clientWidth / masterFrameAspectRatio);
                masterFrameX = 0.0f;
                masterFrameY = 0.0f + (((float)clientHeight - masterFrameHeight) / 2.0f);
            }





            // Determine the location and extent of the Master Sheet.
            // Master Sheet is a scaled down version of the frame (just to
            // have some border), centered within the frame area.
            masterSheetToFrameScale = 0.95f;
            masterSheetWidth = (masterSheetToFrameScale * masterFrameWidth);
            masterSheetHeight = (masterSheetToFrameScale * masterFrameHeight);
            masterSheetX = masterFrameX + ((masterFrameWidth - masterSheetWidth) / 2.0f);
            masterSheetY = masterFrameY + ((masterFrameHeight - masterSheetHeight) / 2.0f);


            // Determine the location and extent of the Stats Frame.
            // Stats Frame is the first horizontal 1/3rd of the sheet.
            // NOTE: the Master Sheet has a 3:2 aspect ratio, so this
            // frame will have a 1:2 aspect ratio.
            statsFrameWidth = (masterSheetWidth / 3.0f);
            statsFrameHeight = masterSheetHeight;
            statsFrameX = masterSheetX;
            statsFrameY = masterSheetY;


            // Stats Sheet is a scaled down version of the Stats Frame
            // (just to have some border), centered within the Stats Frame area.
            statsSheetToFrameScale = 0.95f;
            statsSheetWidth = (statsSheetToFrameScale * statsFrameWidth);
            statsSheetHeight = (statsSheetToFrameScale * statsFrameHeight);
            statsSheetX = statsFrameX + ((statsFrameWidth - statsSheetWidth) / 2.0f);
            statsSheetY = statsFrameY + ((statsFrameHeight - statsSheetHeight) / 2.0f);

            // Board Frame is the second horizontal 1/3rd of the sheet.
            boardFrameWidth = (masterSheetWidth / 3.0f);
            boardFrameHeight = masterSheetHeight;
            boardFrameX = masterSheetX + (masterSheetWidth / 3.0f);
            boardFrameY = masterSheetY;

            // Board Sheet is a scaled down version of the Board Frame
            // (just to have some border), centered within the Board Frame area.
            boardSheetToFrameScale = 0.95f;
            boardSheetWidth = (boardSheetToFrameScale * boardFrameWidth);
            boardSheetHeight = (boardSheetToFrameScale * boardFrameHeight);
            boardSheetX = boardFrameX + ((boardFrameWidth - boardSheetWidth) / 2.0f);
            boardSheetY = boardFrameY + ((boardFrameHeight - boardSheetHeight) / 2.0f);

            // Video Frame is the third horizontal 1/3rd of the sheet.
            videoFrameWidth = (masterSheetWidth / 3.0f);
            videoFrameHeight = masterSheetHeight;
            videoFrameX = masterSheetX + (2.0f * masterSheetWidth / 3.0f); ;
            videoFrameY = masterSheetY;

            // Video Sheet is a scaled down version of the Video Frame
            // (just to have some border), centered within the Video Frame area.
            videoSheetToFrameScale = 0.95f;
            videoSheetWidth = (videoSheetToFrameScale * videoFrameWidth);
            videoSheetHeight = (videoSheetToFrameScale * videoFrameHeight);
            videoSheetX = videoFrameX + ((videoFrameWidth - videoSheetWidth) / 2.0f);
            videoSheetY = videoFrameY + ((videoFrameHeight - videoSheetHeight) / 2.0f);




            float x1 = 0.0f;
            float y1 = 0.0f;
            float x2 = 0.0f;
            float y2 = 0.0f;

            if (0 != gameState.mShowInstructionPage)
            {
                x1 = 16;
                y1 = 16;
                x2 = x1 + (clientWidth - 32.0f);
                y2 = y1 + (clientHeight - 32.0f);

                STGameDrawing.DrawInstructions( gr, (int)x1, (int)y1, (int)x2, (int)y2, game );
            }
            else if (true == gameState.mShowFileList)
            {
                x1 = 16;
                y1 = 16;
                x2 = x1 + (clientWidth - 32.0f);
                y2 = y1 + (clientHeight - 32.0f);

                STGameDrawing.DrawFileList( gr, (int)x1, (int)y1, (int)x2, (int)y2, game );
            }
            else
            {
                if (false == gameState.mShowConsole)
                {
                    // VIDEO CAPTURE GUI
                    if (true == game.GameIsSpawnFromVideoCapture( ))
                    {
                        STUserInterface.HandleVideoCaptureGUI
                          (
                          gr,
                          videoSheetX,
                          videoSheetY,
                          videoSheetWidth,
                          videoSheetHeight,
                          game,
                          clientWidth,
                          clientHeight,
                          clientRelativeCursorX,
                          clientRelativeCursorY
                          );
                    }
                    else
                    {
                        x1 = videoSheetX;
                        y1 = videoSheetY;
                        x2 = x1 + (videoSheetWidth - 1.0f);
                        y2 = y1 + (videoSheetHeight - 1.0f);

                        // Draw stats in entire remaining client area
                        if ((clientWidth - x1) > 200)
                        {
                          x1 = (clientWidth - 200);
                        }
                        STGameDrawing.DrawStatisticsPane
                          (
                          gr,
                          (int)x1, // x1
                          (int)(0), // y1
                          (int)(clientWidth), // x2
                          (int)(y2), // y2
                          game,
                          console
                          );

                    }



                    x1 = boardSheetX;
                    y1 = boardSheetY;
                    x2 = x1 + (boardSheetWidth - 1.0f);
                    y2 = y1 + (boardSheetHeight - 1.0f);
                    if (clientWidth > 640)
                    {
                        x1 = 216;
                        x2 = (clientWidth - 216);
                    }
                    if (clientHeight > 480)
                    {
                        y1 = 64;
                        y2 = (clientHeight - 64);
                    }


                    // DRAW GAME BOARD
                    if (false == gameState.mCalibrationModeFlag)
                    {
                        STGameDrawing.DrawGameBoard( gr, x1, y1, x2, y2, game );
                    }

                    // DRAW TRAINING-MODE BOARD
                    if (true == gameState.mCalibrationModeFlag)
                    {
                        STGameDrawing.DrawCalibrationBoard( gr, x1, y1, x2, y2, game );
                    }

                    x1 = statsSheetX;
                    y1 = statsSheetY;
                    x2 = x1 + (statsSheetWidth - 1.0f);
                    y2 = y1 + (statsSheetHeight - 1.0f);

                    x1 = 16;
                    y1 = 0;
                    STGameDrawing.DrawStatusPane( gr, (int)x1, (int)y1, (int)x2, (int)y2, game, console );
                }
                else // SHOW CONSOLE
                {
                    
                    // Draw Board in Stats Sheet area
                    x1 = statsSheetX;
                    y1 = statsSheetY;
                    x2 = x1 + (statsSheetWidth - 1.0f);
                    y2 = y1 + (statsSheetHeight - 1.0f);

                    // DRAW GAME BOARD
                    if (false == gameState.mCalibrationModeFlag)
                    {
                        STGameDrawing.DrawGameBoard( gr, x1, y1, x2, y2, game );
                    }

                    // DRAW TRAINING-MODE BOARD
                    if (true == gameState.mCalibrationModeFlag)
                    {
                        STGameDrawing.DrawCalibrationBoard( gr, x1, y1, x2, y2, game );
                    }

                    // Draw console in remainder of screen
                    x1 = boardSheetX;
                    y1 = 0.0f;
                    x2 = (float)(clientWidth) - 1.0f;
                    y2 = y1 + (videoSheetHeight - 1.0f);

                    STGameDrawing.DrawConsolePane( gr, (int)x1, (int)y1, (int)x2, (int)y2, console );
                }
            }

        }



    }
}
