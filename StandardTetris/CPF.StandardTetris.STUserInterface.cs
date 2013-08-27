// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using CPF.GRV10;



namespace CPF.StandardTetris
{
    public class STUserInterface
    {




        public static void HandleKeyPress
            (
            GR gr,
                IntPtr hwnd,
                int wParam,
                int lParam,
                STGame game,
            Keys keyCode,
            bool shiftKeyState,
            bool controlKeyState
            )
        {
            STGameState gameState = game.GetGameState( );



            // Priority of key press handling:
            //   (1) Instructions;    ESCAPE exits instructions;
            //   (2) File Menu;       ESCAPE exits file menu;
            //   (3) Calibrate;       ESCAPE cancels calibrate mode;
            //   (4) Video Capture;   ESCAPE quits application;
            //   (5) Normal;          ESCAPE quits application;








            // INSTRUCTIONS
            if (0 != game.InstructionGetState( ))
            {
                switch (keyCode)
                {
                    case Keys.Down:
                    case Keys.Next: // Page-Down
                    case Keys.Right:
                        {
                            // Next page
                            game.InstructionsNextPage( );
                        }
                        break;

                    case Keys.Up:
                    case Keys.Prior: // Page-Up
                    case Keys.Left:
                        {
                            // Previous page
                            game.InstructionsPreviousPage( );
                        }
                        break;

                    default:
                        {
                            // User hit a key, but it wasn't relevant, so exit menu.
                            game.InstructionsHide( );
                            // NOTE: Don't resume! : game.InputEventResume();
                        }
                        break;
                }
                return;
            }
            else if (keyCode == Keys.I)
            {
                game.InstructionsShow( );
                game.InputEventPause( );
                return;
            }





            // FILE LIST
            if (true == gameState.mShowFileList)
            {
                switch (keyCode)
                {
                    case Keys.Next: // Page-Down
                        {
                            // Next page
                            gameState.mFirstItem += 20;
                        }
                        break;

                    case Keys.Prior: // Page-Up
                        {
                            // Previous page
                            gameState.mFirstItem -= 20;
                        }
                        break;

                    case Keys.Down:
                        {
                            // Next Item
                            gameState.mRelativeItem++;
                            if (gameState.mRelativeItem > 19)
                            {
                                gameState.mFirstItem++;
                                gameState.mRelativeItem = 19;
                            }
                        }
                        break;

                    case Keys.Up:
                        {
                            // Previous Item
                            gameState.mRelativeItem--;
                            if (gameState.mRelativeItem < 0)
                            {
                                gameState.mFirstItem--;
                                gameState.mRelativeItem = 0;
                            }
                        }
                        break;

                    case Keys.Return:
                        {
                            // Load item
                            gameState.mLoadFlag = true;
                        }
                        break;

                    default:
                        {
                            // User hit a key, but it wasn't relevant, so exit menu.
                            gameState.mShowFileList = false;
                            // NOTE: Don't resume. : game.InputEvent_Resume();
                        }
                        break;
                }
                return;
            }
            else if ((keyCode == Keys.L) && (true == shiftKeyState))
            {
                // SHIFT-L will read a text file in to the game state.
                game.InputEventPause( );
                gameState.mShowFileList = true;
                gameState.mFirstItem = 0;
                gameState.mRelativeItem = 0;
                gameState.mLoadFlag = false;

                STEngine.GetFileList().ScanDirectory( STEngine.GetApplicationPath( ) );

                return;
            }







            // Calibrate Mode
            // (NOTE: See how normal mode enters calibrate mode by pressing 'C'.)
            if (true == game.GetCalibrationModeFlagValue( ))
            {
                if ((Keys.Escape == keyCode) || (Keys.C == keyCode))
                {
                    game.SetCalibrationModeFlagValue( false );
                    game.InputEventResume( );
                    return;
                }

                if (keyCode == Keys.V)
                {
                    if (false == game.GameIsSpawnFromVideoCapture( ))
                    {
                        // Set up sane conditions
                        game.InputEventReset( );
                        game.InputEventShowNextPieceOff( );
                        game.InputEventAutoRestartOff( );
                        // Initialize Video Capture
                        STEngine.GetVideoProcessing( ).Initialize( gr, hwnd );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                        game.InputEventVideoStart( );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    }
                    else
                    {
                        STEngine.GetVideoProcessing( ).Terminate( );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                        game.InputEventVideoStop( );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    }
                    return;
                }

                switch (keyCode)
                {
                    // training mode piece selection
                    case Keys.D0: game.SetCalibrationModeShapeCode( 0 ); break;
                    case Keys.D1: game.SetCalibrationModeShapeCode( 1 ); break;
                    case Keys.D2: game.SetCalibrationModeShapeCode( 2 ); break;
                    case Keys.D3: game.SetCalibrationModeShapeCode( 3 ); break;
                    case Keys.D4: game.SetCalibrationModeShapeCode( 4 ); break;
                    case Keys.D5: game.SetCalibrationModeShapeCode( 5 ); break;
                    case Keys.D6: game.SetCalibrationModeShapeCode( 6 ); break;
                    case Keys.D7: game.SetCalibrationModeShapeCode( 7 ); break;
                    case Keys.D8: game.SetCalibrationModeShapeCode( 0 ); break;
                    case Keys.D9: game.SetCalibrationModeShapeCode( 0 ); break;
                }
                return;
            }

            
            
            
            
            // Video Capture
            // The following is not mutually-exclusive with normal game play.
            if (true == game.GameIsSpawnFromVideoCapture( ))
            {
                if (keyCode == Keys.Return)
                {
                    STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    game.ClearPreviousClassification( );
                    game.InputEventReset( );
                    System.Threading.Thread.Sleep( 200 );
                    STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    game.ClearPreviousClassification( );
                    game.InputEventReset( );
                    System.Threading.Thread.Sleep( 200 );
                    STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    game.ClearPreviousClassification( );
                    game.InputEventReset( );
                    System.Threading.Thread.Sleep( 200 );
                    STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    game.ClearPreviousClassification( );
                    game.InputEventReset( );
                    System.Threading.Thread.Sleep( 200 );
                }

                if (keyCode == Keys.V)
                {
                    if (false == game.GameIsSpawnFromVideoCapture( ))
                    {
                        // Set up sane conditions
                        game.InputEventReset( );
                        game.InputEventShowNextPieceOff( );
                        game.InputEventAutoRestartOff( );
                        // Initialize Video Capture
                        STEngine.GetVideoProcessing( ).Initialize( gr, hwnd );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                        game.InputEventVideoStart( );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    }
                    else
                    {
                        STEngine.GetVideoProcessing( ).Terminate( );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                        game.InputEventVideoStop( );
                        STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    }
                    return;
                }

            }






            // Console Mode
            if (true == gameState.mShowConsole)
            {
                if (keyCode == Keys.Delete)
                {
                    STEngine.GetConsole( ).ClearAllLines( );
                }
                else
                {
                    // Any key other than delete or P (pause) exits console mode.
                    if (keyCode != Keys.P)
                    {
                        gameState.mShowConsole = false;
                    }
                }
            }
            else
            {
                if ((keyCode == Keys.Q) && (true == shiftKeyState))
                {
                    // SHIFT-Q : Console
                    gameState.mShowConsole = true;
                }
            }




            // Normal Game Play
            // QUIT KEY:  ESCAPE
            if (keyCode == Keys.Escape)
            {
                Form form = (Form)STEngine.GetMainForm( );
                form.Close( );
                return;
            }

            // Enter Calibrate Mode
            if (keyCode == Keys.C)
            {
                game.SetCalibrationModeFlagValue( true );
                game.SetCalibrationModeShapeCode( 1 );
                game.InputEventPause( );
            }

            // Enable Video Capture
            if (keyCode == Keys.V)
            {
                if (false == game.GameIsSpawnFromVideoCapture( ))
                {
                    // Set up sane conditions
                    game.InputEventReset( );
                    game.InputEventShowNextPieceOff( );
                    game.InputEventAutoRestartOff( );
                    // Initialize Video Capture
                    STEngine.GetVideoProcessing( ).Initialize( gr, hwnd );
                    STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                    game.InputEventVideoStart( );
                    STEngine.GetVideoProcessing( ).ClearRegionStatus( );
                }
            }

            // Reset Game
            if (keyCode == Keys.Return)
            {
                if (true == shiftKeyState)
                {
                    game.InputEventHardReset( );
                }
                else
                {
                    game.InputEventReset( );
                }
            }

            if (keyCode == Keys.P)
            {
                if (true == game.GameIsPaused( ))
                {
                    game.InputEventResume( );
                }
                else
                {
                    game.InputEventPause( );
                }
            }

            if (keyCode == Keys.A)
            {
                if (true == shiftKeyState)
                {
                    STStrategyManager.SelectNextStrategy( );
                }
                else
                {
                    if (true == game.GameIsAI( ))
                    {
                        game.InputEventAIStop( );
                    }
                    else
                    {
                        game.InputEventAIStart( );
                    }
                }
            }

            if (keyCode == Keys.T)
            {
                if (game.GameIsOutputToRS232( ))
                {
                    game.InputEventRS232Stop( );
                    STRS232.TerminatePort( );
                }
                else
                {
                    STRS232.InitializePort( );
                    game.InputEventRS232Start( );
                }
            }

            if ((keyCode == Keys.Subtract) || (keyCode == Keys.OemMinus)) //  0xbd
            {
                if (false == shiftKeyState)
                {
                    game.InputEventGameSpeedDecrease( );
                }
            }
            if ((keyCode == Keys.Add) || (keyCode == Keys.Oemplus)) // 0xbb
            {
                if (false == shiftKeyState)
                {
                    game.InputEventGameSpeedIncrease( );
                }
            }

            if ((keyCode == Keys.W) && (true == shiftKeyState))
            {
                // SHIFT-W will write out a text file (c:\tetris_state.txt)
                game.InputEventGameStateWriteToFile( );
            }

            if (keyCode == Keys.Next) // Page-Down
            {
                // Page-Down: Decrease Board Size
                game.InputEventGameBoardDecrease( );
            }
            if (keyCode == Keys.Prior) // Page-Up
            {
                // Page-Up:  Increase Board Size
                game.InputEventGameBoardIncrease( );
            }


            if (true == controlKeyState)
            {
                if (keyCode == Keys.Up)
                {
                    game.InputEventGameBoardIncreaseHeight( );
                }
                if (keyCode == Keys.Left)
                {
                    game.InputEventGameBoardDecreaseWidth( );
                }
                if (keyCode == Keys.Right)
                {
                    game.InputEventGameBoardIncreaseWidth( );
                }
                if (keyCode == Keys.Down)
                {
                    game.InputEventGameBoardDecreaseHeight( );
                }
            }








            // COLOR SCHEME
            if ((keyCode == Keys.K) && (true == shiftKeyState))
            {
                if (false == game.GetGameState( ).mMonochromeColorMode)
                {
                    game.GetGameState( ).mMonochromeColorMode = true;
                }
                else
                {
                    game.GetGameState( ).mMonochromeColorMode = false;
                }
            }






            // Non Video-Capture Options
            if (false == game.GameIsSpawnFromVideoCapture( ))
            {
                // Only respond to user piece-control input if AI is not active.
                if (false == game.GameIsAI( ))
                {
                    if (false == controlKeyState)
                    {
                        if (keyCode == Keys.Up)
                        {
                            game.InputEventRotate( );
                        }
                        if (keyCode == Keys.Left)
                        {
                            game.InputEventLeft( );
                        }
                        if (keyCode == Keys.Right)
                        {
                            game.InputEventRight( );
                        }
                        if (keyCode == Keys.Down)
                        {
                            game.InputEventDrop( );
                        }
                        if (keyCode == Keys.Space)
                        {
                            game.InputEventDrop( );
                        }
                    }
                }

                if (keyCode == Keys.Z)
                {
                    if ((STPieceSequence.STPieceSelectionSource.AlternatingSAndZ) ==
                         game.GetPieceSequenceSourceType( ))
                    {
                        // Since we're in S/Z mode, stop.
                        game.InputEventSZPieceModeStop( );
                    }
                    else
                    {
                        // Start S/Z mode.
                        game.InputEventSZPieceModeStart( );
                    }
                }

                if (keyCode == Keys.S)
                {
                    // S will cycle the shadow mode.
                    game.InputEventShadowModeCycle( );
                }

                if ((keyCode == Keys.J) && (true == shiftKeyState))
                {
                    // SHIFT-J : Add line of random junk to bottom of the pile.
                    game.InputEventAddRowOfJunk( );
                }

                if ((keyCode == Keys.H) && (true == shiftKeyState))
                {
                    // SHIFT-H : Hint Mode
                    if (true == game.GameIsHintMode( ))
                    {
                        game.InputEventHintModeStop( );
                    }
                    else
                    {
                        game.InputEventHintModeStart( );
                    }
                }

                if (keyCode == Keys.N)
                {
                    if (true == game.GameIsShowNextPiece( ))
                    {
                        game.InputEventShowNextPieceOff( );
                    }
                    else
                    {
                        game.InputEventShowNextPieceOn( );
                    }
                }

                if (keyCode == Keys.X)
                {
                    game.InputEventToggleMoveAnimation( );
                }

                if (keyCode == Keys.U)
                {
                    if (true == game.GameIsAutoRestart( ))
                    {
                        game.InputEventAutoRestartOff( );
                    }
                    else
                    {
                        game.InputEventAutoRestartOn( );
                    }
                }
                if (keyCode == Keys.F)
                {
                    game.InputEventToggleAutoWriteFile( );
                }


                if ((keyCode == Keys.R) && (true == shiftKeyState))
                {
                    // SHIFT-R : Soft reset (game goes back to same random seed)
                    game.InputEventSoftReset( );
                }
            }
        }












        // Call with StandardTetrisTimeT::GetIntervalDurationSecondsFloat() in main()

        public static void PerformGameIterations
        (
            STGame game,
            double deltaTimeSeconds
        )
        {
            // First, update the game time.
            game.ConditionalAdvanceGameTimeByDelta( deltaTimeSeconds );


            int gameSpeed = 0;
            gameSpeed = game.GetGameSpeedAdjustment( );

            if (gameSpeed <= 2)
            {
                // Perform any unforced game iteration.
                game.ConditionalAdvanceGameIteration( false );
            }
            else
            {
                int totalIncrements = (8 * gameSpeed);
                int incrementCounter = 0;
                for
                    (
                        incrementCounter = 0;
                        incrementCounter < totalIncrements;
                        incrementCounter++
                    )
                {
                    game.ConditionalAdvanceGameIteration( true );
                }
            }



            // Update reported frame rate
            {
                float frameDurationSeconds = 0.0f;
                frameDurationSeconds = (float)deltaTimeSeconds;

                float framesPerSecond = 0.0f;
                if (frameDurationSeconds > 0.0001f)
                {
                    framesPerSecond = (1.0f / frameDurationSeconds);
                }
                if (framesPerSecond < 0.0f) 
                {
                    framesPerSecond = 0.0f;
                }
                game.SetReportedFrameRate( framesPerSecond );
            }
        }








        [System.Runtime.InteropServices.DllImport( "user32.dll" )]
        private static extern short GetAsyncKeyState ( System.Windows.Forms.Keys key );




        public static void HandleVideoCaptureGUI
          (
            GR gr,
            float videoSheetX,
            float videoSheetY,
            float videoSheetWidth,
            float videoSheetHeight,
            STGame game,
            int clientWidth,
            int clientHeight,
            int clientRelativeCursorX,
            int clientRelativeCursorY
          )
        {
            STGameState gameState = game.GetGameState( );


            if (false == game.GameIsSpawnFromVideoCapture( ))
            {
                return;
            }


            gr.glBindTexture
                ( 
                GR.GL_TEXTURE_2D, 
                STEngine.GetVideoProcessing( ).mTextureOpenGLHandleBGR256x256 
                );


            float x1 = 0.0f;
            float y1 = 0.0f;
            float x2 = 0.0f;
            float y2 = 0.0f;

            x1 = videoSheetX;
            y1 = videoSheetY;
            x2 = x1 + (videoSheetWidth - 1.0f);
            y2 = y1 + (videoSheetHeight - 1.0f);

            float u1 = 0.0f;
            float v1 = 0.0f;
            float u2 = 0.0f;
            float v2 = 0.0f;

            u1 = 0.0f;
            v1 = 0.0f;
            u2 = 0.5f;
            v2 = 1.0f;

            gr.glEnable( GR.GL_SCISSOR_TEST );
            gr.glScissor( (int)(x1), (int)(y1), (int)((x2 - x1) + 1), (int)((y2 - y1) + 1) );

            gr.glEnable( GR.GL_TEXTURE_2D );
            gr.glColor3f( 1.0f, 1.0f, 1.0f );

            gr.glBegin( GR.GL_QUADS );
            gr.glTexCoord2f( u1, v2 );
            gr.glVertex2f( x1, y2 );

            gr.glTexCoord2f( u1, v1 );
            gr.glVertex2f( x1, y1 );

            gr.glTexCoord2f( u2, v1 );
            gr.glVertex2f( x2, y1 );

            gr.glTexCoord2f( u2, v2 );
            gr.glVertex2f( x2, y2 );
            gr.glEnd( );

            gr.glDisable( GR.GL_TEXTURE_2D );
            gr.glDisable( GR.GL_SCISSOR_TEST );




            int xTexelMin = 0;
            int yTexelMin = 0;
            int xTexelMax = 0;
            int yTexelMax = 0;


            int xScreenMin = 0;
            int yScreenMin = 0;
            int xScreenMax = 0;
            int yScreenMax = 0;






            // Only listen to the mouse in training/calibration mode
            if (true == gameState.mCalibrationModeFlag)
            {
                if (0 != GetAsyncKeyState( Keys.LButton ))
                {
                    // Left button pressed
                    if (0 == gameState.mSelectionState)
                    {
                        gameState.mSelectionState = 1;
                        gameState.mSelectionX1 = clientRelativeCursorX;
                        gameState.mSelectionY1 = ((clientHeight - 1) - clientRelativeCursorY);
                        gameState.mSelectionX2 = clientRelativeCursorX;
                        gameState.mSelectionY2 = ((clientHeight - 1) - clientRelativeCursorY);
                    }
                    else
                    {
                        gameState.mSelectionX2 = clientRelativeCursorX;
                        gameState.mSelectionY2 = ((clientHeight - 1) - clientRelativeCursorY);
                    }
                }
                else
                {
                    // Left button released
                    if (0 == gameState.mSelectionState)
                    {
                        // Nothing to do...
                    }
                    else
                    {
                        gameState.mSelectionState = 0;
                    }
                }

                gr.glEnable( GR.GL_SCISSOR_TEST );
                gr.glScissor( 0, 0, clientWidth, clientHeight );

                gr.glColor3f( 1.0f, 0.0f, 0.0f );
                gr.glBegin( GR.GL_LINES );

                gr.glVertex2f( (float)clientRelativeCursorX - 8.0f, (float)((clientHeight - 1) - clientRelativeCursorY) );
                gr.glVertex2f( (float)clientRelativeCursorX + 8.0f, (float)((clientHeight - 1) - clientRelativeCursorY) );

                gr.glVertex2f( (float)clientRelativeCursorX, (float)((clientHeight - 1) - clientRelativeCursorY) - 8.0f );
                gr.glVertex2f( (float)clientRelativeCursorX, (float)((clientHeight - 1) - clientRelativeCursorY) + 8.0f );
                gr.glEnd( );
            }






            if (0 != ((GetAsyncKeyState( Keys.Shift )) & 0x8000))
            {
                if (0 != ((GetAsyncKeyState( Keys.Left )) & 0x8000))
                {
                    gameState.mSelectionX2--;
                }
                if (0 != ((GetAsyncKeyState( Keys.Right )) & 0x8000))
                {
                    gameState.mSelectionX2++;
                }
                if (0 != ((GetAsyncKeyState( Keys.Down )) & 0x8000))
                {
                    gameState.mSelectionY2--;
                }
                if (0 != ((GetAsyncKeyState( Keys.Up )) & 0x8000))
                {
                    gameState.mSelectionY2++;
                }
            }
            else
            {
                if (0 != ((GetAsyncKeyState( Keys.Left )) & 0x8000))
                {
                    gameState.mSelectionX1--;
                }
                if (0 != ((GetAsyncKeyState( Keys.Right )) & 0x8000))
                {
                    gameState.mSelectionX1++;
                }
                if (0 != ((GetAsyncKeyState( Keys.Down )) & 0x8000))
                {
                    gameState.mSelectionY1--;
                }
                if (0 != ((GetAsyncKeyState( Keys.Up )) & 0x8000))
                {
                    gameState.mSelectionY1++;
                }
            }







            xScreenMin = gameState.mSelectionX1;
            yScreenMin = gameState.mSelectionY1;
            xScreenMax = gameState.mSelectionX2;
            yScreenMax = gameState.mSelectionY2;



            xTexelMin = (int)(256.0f * (((float)xScreenMin - videoSheetX) / videoSheetHeight));
            yTexelMin = (int)(256.0f * (((float)yScreenMin - videoSheetY) / videoSheetHeight));
            xTexelMax = (int)(256.0f * (((float)xScreenMax - videoSheetX) / videoSheetHeight));
            yTexelMax = (int)(256.0f * (((float)yScreenMax - videoSheetY) / videoSheetHeight));

            int disregard = 0;

            if (xTexelMin < 0)
            {
                disregard = 1;
                xTexelMin = 0;
            }
            if (yTexelMin < 0)
            {
                disregard = 1;
                yTexelMin = 0;
            }
            if (xTexelMax < 0)
            {
                disregard = 1;
                xTexelMax = 0;
            }
            if (yTexelMax < 0)
            {
                disregard = 1;
                yTexelMax = 0;
            }

            if (xTexelMin > 255)
            {
                disregard = 1;
                xTexelMin = 255;
            }
            if (yTexelMin > 255)
            {
                disregard = 1;
                yTexelMin = 255;
            }
            if (xTexelMax > 255)
            {
                disregard = 1;
                xTexelMax = 255;
            }
            if (yTexelMax > 255)
            {
                disregard = 1;
                yTexelMax = 255;
            }

            if (xTexelMin > xTexelMax)
            {
                int swap = xTexelMin;
                xTexelMin = xTexelMax;
                xTexelMax = swap;
            }

            if (yTexelMin > yTexelMax)
            {
                int swap = yTexelMin;
                yTexelMin = yTexelMax;
                yTexelMax = swap;
            }


            // Only set region if in training mode!
            if ((true == gameState.mCalibrationModeFlag) && (0 == disregard))
            {
                STEngine.GetVideoProcessing( ).SetRegion( xTexelMin, yTexelMin, xTexelMax, yTexelMax );
            }


            STEngine.GetVideoProcessing( ).GetRegion( ref xTexelMin, ref yTexelMin, ref xTexelMax, ref yTexelMax );

            xScreenMin = (int)(videoSheetX + (videoSheetHeight * (float)xTexelMin / 256.0f));
            yScreenMin = (int)(videoSheetY + (videoSheetHeight * (float)yTexelMin / 256.0f));
            xScreenMax = (int)(videoSheetX + (videoSheetHeight * (float)xTexelMax / 256.0f));
            yScreenMax = (int)(videoSheetY + (videoSheetHeight * (float)yTexelMax / 256.0f));


            x1 = videoSheetX;
            y1 = videoSheetY;
            x2 = x1 + (videoSheetWidth - 1.0f);
            y2 = y1 + (videoSheetHeight - 1.0f);


            int currentClassification = STEngine.GetVideoProcessing( ).GetRegionClassification( );

            if (0 == currentClassification)
            {
                // If the previous classification was a PIECE, and the current classification
                // is something different, then submit the piece (which must have fallen
                // by a row by now).
                if ((gameState.mPreviousClassification >= 1) && (gameState.mPreviousClassification <= 7))
                {
                    game.SpawnSpecifiedPieceShape( STPiece.GetShapeCorrespondingToByteCode( (byte)gameState.mPreviousClassification ) );
                }
            }

            gameState.mPreviousClassification = currentClassification;


            Color color;

            color =
                STGameDrawing.GetCellValueColorARGB // Returns WHITE for unknown
                (
                    (byte)currentClassification, // 0..6
                    false // monochrome mode
                );

            float red = 0.0f;
            float green = 0.0f;
            float blue = 0.0f;

            red = (float)(color.R) / 255.0f;
            green = (float)(color.G) / 255.0f;
            blue = (float)(color.B) / 255.0f;


            gr.glColor3f( red, green, blue );

            gr.glBegin( GR.GL_LINES );

            gr.glVertex2f( (float)xScreenMin, (float)yScreenMin );
            gr.glVertex2f( (float)xScreenMin, (float)yScreenMax );

            gr.glVertex2f( (float)xScreenMax, (float)yScreenMin );
            gr.glVertex2f( (float)xScreenMax, (float)yScreenMax );

            gr.glVertex2f( (float)xScreenMin, (float)yScreenMin );
            gr.glVertex2f( (float)xScreenMax, (float)yScreenMin );

            gr.glVertex2f( (float)xScreenMin, (float)yScreenMax );
            gr.glVertex2f( (float)xScreenMax, (float)yScreenMax );

            // Horizontal divider
            gr.glVertex2f( (float)xScreenMin, (float)((yScreenMin + yScreenMax) / 2) );
            gr.glVertex2f( (float)xScreenMax, (float)((yScreenMin + yScreenMax) / 2) );

            // Vertical dividers
            gr.glVertex2f( (float)(xScreenMin + ((xScreenMax - xScreenMin) / 4)), (float)yScreenMin );
            gr.glVertex2f( (float)(xScreenMin + ((xScreenMax - xScreenMin) / 4)), (float)yScreenMax );

            gr.glVertex2f( (float)(xScreenMin + 2 * ((xScreenMax - xScreenMin) / 4)), (float)yScreenMin );
            gr.glVertex2f( (float)(xScreenMin + 2 * ((xScreenMax - xScreenMin) / 4)), (float)yScreenMax );

            gr.glVertex2f( (float)(xScreenMin + 3 * ((xScreenMax - xScreenMin) / 4)), (float)yScreenMin );
            gr.glVertex2f( (float)(xScreenMin + 3 * ((xScreenMax - xScreenMin) / 4)), (float)yScreenMax );

            gr.glEnd( );

            gr.glDisable( GR.GL_SCISSOR_TEST );
        }











    }
}
