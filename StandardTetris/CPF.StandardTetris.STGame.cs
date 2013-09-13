// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;


namespace CPF.StandardTetris
{
    public class STGame
    {


        private STGameState   mGameState = new STGameState();

        public STGameState GetGameState ( )
        {
            return (mGameState);
        }


        // Constructor and Destructor
        public STGame()
        {
        }



        // Core game state

        // Board and piece

        // NOTE: Clients of the STGame cannot access the current board
        //       and piece directly.  Clients can request copies.

        public void GetCopyOfCurrentBoard ( STBoard board )
        {
            board.CopyFrom( this.mGameState.mSTBoardCurrent );
        }

        public void GetCopyOfCurrentPiece( STPiece piece )
        {
            piece.CopyFrom( this.mGameState.mSTPieceCurrent );
        }


        // Core game state variables

        public bool   GameIsFinished( )
        { 
            return( this.mGameState.mGameOver ); 
        }

        public double GetIterationCountdownSeconds( )
        { 
            return (this.mGameState.mIterationCountdownSeconds); 
        }

        public long   GetCurrentPiecePointValue( )
        { 
            return (this.mGameState.mCurrentPiecePointValue); 
        }

        public long GetCompletedRows ( )
        { 
            return (this.mGameState.mCompletedRows); 
        }

        public long GetCurrentLevel ( )
        {
            if (this.mGameState.mCompletedRows <= 0)
            {
                return (1);
            }
            else if ((this.mGameState.mCompletedRows >= 1) && (this.mGameState.mCompletedRows <= 90))
            {
                return (1 + ((this.mGameState.mCompletedRows - 1) / 10));
            }
            return (10);
        }

        
        
        // User Options
        // (NOTE: The user can only change the game state via input events.)

        public bool GameIsPaused( )
        { 
            return (this.mGameState.mPaused); 
        }

        public bool GameIsShowNextPiece( )
        { 
            return (this.mGameState.mShowNextPiece); 
        }

        public bool GameIsAI( )
        { 
            return (this.mGameState.mAI); 
        }

        public bool GameIsSpawnFromVideoCapture( )
        { 
            return (this.mGameState.mSpawnFromVideoCapture); 
        }

        public bool GameIsCalibrationMode ( )
        {
            return (this.mGameState.mCalibrationModeFlag);
        }

        public bool GameIsOutputToRS232 ( )
        { 
            return (this.mGameState.mOutputToRS232); 
        }

        public bool GameIsAutoRestart( )
        { 
            return (this.mGameState.mAutoRestart); 
        }

        public int GetGameSpeedAdjustment( )
        { 
            return (this.mGameState.mGameSpeedAdjustment); 
        }

        public bool GetGameShadowMode( )
        { 
            return (this.mGameState.mShadowMode); 
        }

        public bool GameIsHintMode( )
        { 
            return (this.mGameState.mHintMode); 
        }

        public bool GameIsMonochromeMode ( )
        {
            return (this.mGameState.mMonochromeColorMode);
        }

        public bool GameIsAutoWriteFile( )
        { 
            return (this.mGameState.mAutoWriteFile); 
        }

        public bool GameIsAnimateMoves( )
        { 
            return (this.mGameState.mAnimateAIMovesEnable);
        }



        
        // Input events

        // Clients of the game object can call these event functions based on user
        // input (key pressing), AI decisions, or other sources.


        // Core game controls

        public void InputEventLeft( )
        {
            if (true == this.mGameState.mOutputToRS232) 
            {
                STRS232.MomentaryRelay_LEFT();
            }
            this.PrivateTranslatePiece( -1 );
        }

        public void InputEventRight( )
        {
            if (true == this.mGameState.mOutputToRS232)
            {
                STRS232.MomentaryRelay_RIGHT( );
            }
            this.PrivateTranslatePiece( 1 );
        }

        public void InputEventRotate( )
        {
            if (true == this.mGameState.mOutputToRS232)
            {
                STRS232.MomentaryRelay_ROTATE( );
            }
            this.PrivateRotatePiece();
        }

        public void InputEventDrop( )
        {
            if (true == this.mGameState.mOutputToRS232)
            {
                STRS232.MomentaryRelay_DROP( );
            }
            this.PrivateDropPiece();
        }





        // Basic Game Management

        public void InputEventReset  ( )
        {
            if (true == this.mGameState.mOutputToRS232) STRS232.MomentaryRelay_RESET( );
            this.PrivateGameReset();
        }

        public void InputEventPause  ( )
        {
            this.mGameState.mPaused = true;
        }

        public void InputEventResume ( )
        {
            this.mGameState.mPaused = false;
        }









        public void InputEventShowNextPieceOn ( )
        {
            this.mGameState.mShowNextPiece = true;
        }

        public void InputEventShowNextPieceOff ( )
        {
            this.mGameState.mShowNextPiece = false;
        }







        public void InputEventAIStart ( )
        {
            this.mGameState.mAI = true;
            this.ClearAllStatistics();
            this.InputEventReset( ); // Starting AI should start clean
        }

        public void InputEventAIStop ( )
        {
            this.mGameState.mAI = false;
            this.ClearAllStatistics();
            this.InputEventReset( ); // Stopping AI should stop clean
        }







        public void InputEventVideoStart ( )
        {
            this.mGameState.mShadowMode = false; // HACK: Stop shadow mode
            this.mGameState.mGameSpeedAdjustment = 0;
            this.mGameState.mSpawnFromVideoCapture = true;

            // Switch piece sequence generator to queue mode.
            this.mGameState.mSTPieceSequence.ServerQueueReset();
            this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
            ( 
                STPieceSequence.STPieceSelectionSource.Queue 
            );

            // Clobber current piece, board, etc.
            this.PrivateGameReset();
            this.ClearAllStatistics();
        }

        public void InputEventVideoStop ( )
        {
            this.mGameState.mSpawnFromVideoCapture = false;

            // Switch piece sequence generator to "random" mode.
            this.mGameState.mSTPieceSequence.ServerQueueReset();
            this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
            (
                STPieceSequence.STPieceSelectionSource.Random
            );

            // Clobber current piece, board, etc.
            this.PrivateGameReset();
            this.ClearAllStatistics();
        }







        public void InputEventRS232Start ( )
        {
            this.mGameState.mOutputToRS232 = true;
        }

        public void InputEventRS232Stop ( )
        {
            this.mGameState.mOutputToRS232 = false;
        }







        public void InputEventAutoRestartOn ( )
        {
            this.mGameState.mAutoRestart = true;
        }

        public void InputEventAutoRestartOff ( )
        {
            this.mGameState.mAutoRestart = false;
        }







        public void InputEventGameSpeedIncrease ( )
        {
            this.mGameState.mGameSpeedAdjustment++;
        }

        public void InputEventGameSpeedDecrease ( )
        {
            this.mGameState.mGameSpeedAdjustment--;
        }




        
        
        
        // Access to Statistics

        public long GetScore( ) 
        { 
            return( this.mGameState.mScore ); 
        }

        public double GetTotalGameTime( )
        { 
            return( this.mGameState.mTotalElapsedTimeSeconds ); 
        }

        public long GetPieceShapeHistogramBinValue( int shapeIndex )
        {
            // 1==O,2==I,3==S,4==Z,5==L,6==J,7==T;   0==NONE
            if ((shapeIndex >= 1) && (shapeIndex <= 7))
            {
                return( this.mGameState.mPieceHistogram[ shapeIndex ] );
            }
            return(0);
        }

        public long    GetPieceShapeHistogramSum( )
        {
            return
            ( 
                  this.mGameState.mPieceHistogram[ 1 ]  // O
                + this.mGameState.mPieceHistogram[ 2 ]  // I
                + this.mGameState.mPieceHistogram[ 3 ]  // S
                + this.mGameState.mPieceHistogram[ 4 ]  // Z
                + this.mGameState.mPieceHistogram[ 5 ]  // L
                + this.mGameState.mPieceHistogram[ 6 ]  // J
                + this.mGameState.mPieceHistogram[ 7 ]  // T
            );
        }

        public long GetHistoricTotalGames ( )
        { 
            return( this.mGameState.mHistoricTotalGames ); 
        }

        public long GetHistoricAverageRows( )
        {
            if (this.mGameState.mHistoricTotalGames <= 0) 
            {
                return(0);
            }
            return( this.mGameState.mHistoricCumulativeRows / this.mGameState.mHistoricTotalGames );
        }

        public long GetHistoricHighScore ( ) 
        { 
            return( this.mGameState.mHistoricHighScore  ); 
        }

        public long GetHistoricHighRows  ( ) 
        { 
            return( this.mGameState.mHistoricHighRows ); 
        }

        public long GetHistoricHighPieces( ) 
        { 
            return( this.mGameState.mHistoricHighPieces ); 
        }

        public long GetHistoricCumulativeRows( ) 
        { 
            return( this.mGameState.mHistoricCumulativeRows ); 
        }

        public long GetHeightHistogramBinValue( int height )
        {
            if (height <    0) return(0);
            if (height >= 200) return(0);
            return( this.mGameState.mHeightHistogram[ height ] );
        }

        public long GetHistoricRowsBinValue( int index )
        {
            if (index <   0) return(0);
            if (index >= 20) return(0);
            return( this.mGameState.mHistoricRows[ index ] );
        }

        public void ClearHistoricRowsHistogram( )
        {
            int i = 0;
            int n = 0;
            n = this.mGameState.mHistoricRows.Length;
            for ( i = 0; i < n; i++ )
            {
                this.mGameState.mHistoricRows[i] = 0;
            }
        }

        
        



        // Call ConditionalAdvanceGameTimeByDelta() each time the time interval
        // is updated (typically once per rendering frame).
        // Call ConditionalAdvanceGameIteration() directly after 
        // ConditionalAdvanceGameTimeByDelta(), at least once, or possibly 
        // multiple times per frame, to potentially force multiple game iterations
        // that do not depend on real time.


        
        // The following function is only useful for rendering purposes, since this
        // board is likely to include a piece that is still falling.


        
        // The following is used by external data sources to add pieces to the 
        // piece sequence FIFO (if enabled).


        
        
        
        // Piece Sequence

        public STPieceSequence GetPieceSequenceObject( )
        { 
            return( this.mGameState.mSTPieceSequence ); 
        }

        public STPieceSequence.STPieceSelectionSource GetPieceSequenceSourceType( ) 
        { 
            return( this.mGameState.mSTPieceSequence.ClientCheckPieceSelectionSource() ); 
        }

        public void SetPieceSequenceSourceType( STPieceSequence.STPieceSelectionSource pieceSequenceSource )
        { 
            this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource( pieceSequenceSource );
        }


        
        
        // Miscellaneous

        public int GetBoardWidth( )
        { 
            return( this.mGameState.mSTBoardCurrent.GetWidth() ); 
        }

        public int GetBoardHeight( )
        { 
            return( this.mGameState.mSTBoardCurrent.GetHeight() );
        }




        // The following methods are intended for use by the game object for 
        // internal processing.



        public void InputEventGameBoardIncrease ( )
        {
          int width  = 0;
          int height = 0;
          int length = 0;

          width  = this.mGameState.mSTBoardCurrent.GetWidth();
          height = this.mGameState.mSTBoardCurrent.GetHeight();

          length = width;
          if (length > (height/2))  length = (height/2);

          length++;

          if (length <   4)  length =   4;
          if (length > 200)  length = 200;

          width  = length;
          height = (2 * length);

          this.mGameState.mSTBoardCurrent.SetBoardDimensions( width, height );
          this.mGameState.mSTBoardCurrent.ClearCells();

          this.InputEventReset();
          this.ClearAllStatistics();
        }


        public void InputEventGameBoardDecrease ( )
        {
          int width  = 0;
          int height = 0;
          int length = 0;

          width  = this.mGameState.mSTBoardCurrent.GetWidth();
          height = this.mGameState.mSTBoardCurrent.GetHeight();

          length = width;
          if (length > (height/2))  length = (height/2);

          length--;

          if (length <   4)  length =   4;
          if (length > 200)  length = 200;

          width  = length;
          height = (2 * length);

          this.mGameState.mSTBoardCurrent.SetBoardDimensions( width, height );
          this.mGameState.mSTBoardCurrent.ClearCells();

          this.InputEventReset();
          this.ClearAllStatistics();
        }



        public void InputEventGameBoardIncreaseWidth ( )
        {
            int width  = 0;
            int height = 0;

            width  = this.mGameState.mSTBoardCurrent.GetWidth();
            height = this.mGameState.mSTBoardCurrent.GetHeight();

            width++;

            if (width <   4)  width =   4;
            if (width > 200)  width = 200;

            this.mGameState.mSTBoardCurrent.SetBoardDimensions( width, height );
            this.mGameState.mSTBoardCurrent.ClearCells();
            this.InputEventReset();
            this.ClearAllStatistics();
        }

        public void InputEventGameBoardDecreaseWidth ( )
        {
            int width  = 0;
            int height = 0;

            width  = this.mGameState.mSTBoardCurrent.GetWidth();
            height = this.mGameState.mSTBoardCurrent.GetHeight();

            width--;

            if (width <   4)  width =   4;
            if (width > 200)  width = 200;

            this.mGameState.mSTBoardCurrent.SetBoardDimensions( width, height );
            this.mGameState.mSTBoardCurrent.ClearCells();
            this.InputEventReset();
            this.ClearAllStatistics();
        }


        public void InputEventGameBoardIncreaseHeight ( )
        {
            int width  = 0;
            int height = 0;

            width  = this.mGameState.mSTBoardCurrent.GetWidth();
            height = this.mGameState.mSTBoardCurrent.GetHeight();

            height++;

            if (height <   4)  height =   4;
            if (height > 200)  height = 200;

            this.mGameState.mSTBoardCurrent.SetBoardDimensions( width, height );
            this.mGameState.mSTBoardCurrent.ClearCells();
            this.InputEventReset();
            this.ClearAllStatistics();
        }


        public void InputEventGameBoardDecreaseHeight ( )
        {
            int width  = 0;
            int height = 0;

            width  = this.mGameState.mSTBoardCurrent.GetWidth();
            height = this.mGameState.mSTBoardCurrent.GetHeight();

            height--;

            if (height <   4)  height =   4;
            if (height > 200)  height = 200;

            this.mGameState.mSTBoardCurrent.SetBoardDimensions( width, height );
            this.mGameState.mSTBoardCurrent.ClearCells();
            this.InputEventReset();
            this.ClearAllStatistics();
        }








        public void InputEventSZPieceModeStart ( )
        {
            this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
                ( STPieceSequence.STPieceSelectionSource.AlternatingSAndZ );
            this.InputEventReset();
            this.ClearAllStatistics();
        }







        public void InputEventSZPieceModeStop  ( )
        {
            if (true == this.mGameState.mSpawnFromVideoCapture)
            {
                // Switch piece sequence source to queue
                this.mGameState.mSTPieceSequence.ServerQueueReset();
                this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
                    ( STPieceSequence.STPieceSelectionSource.Queue );
            }
            else
            {
                // Set piece sequence source to random
                this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
                    ( STPieceSequence.STPieceSelectionSource.Random );
            }
        }







        public void InputEventGameStateWriteToFile( )
        {
            STGameFile.WriteGameStateToFile( STEngine.GetApplicationPath(), STGameFile.GetDefaultFileName(), this.mGameState );
        }







        public void InputEventToggleAutoWriteFile( )
        {
            if (true == this.mGameState.mAutoWriteFile)
            {
                this.mGameState.mAutoWriteFile = false;
            }
            else
            {
                this.mGameState.mAutoWriteFile = true;
            }
        }







        public void   LoadGameStateFromFile( String filePathAndName )
        {
            // Do a hard-reset
            this.InputEventHardReset();

            // Pause the game
            if (false == this.GameIsPaused())
            {
                this.InputEventPause();
            }

            // Load Game State from specified file.
            STGameFile.ReadGameStateFromFile( filePathAndName, this.mGameState );

            // Make sure game is still paused
            if (false == this.GameIsPaused( ))
            {
                this.InputEventPause( );
            }
        }







        public void   InputEventShadowModeCycle ( )
        {
            if (false == this.mGameState.mShadowMode)
            {
                this.mGameState.mShadowMode = true;
            }
            else
            {
                this.mGameState.mShadowMode = false;
            }
        }





        public void  InputEventSoftReset( )
        {
            // The goal is to start the game over with the same deterministic initial
            // conditions.  Thus, even when a game is over we can play the exact same
            // game again.
            // The key is to restore the initial conditions of the piece sequence 
            // generator.
            STPieceSequence.STPieceSelectionSource pieceSequenceSource = 
                STPieceSequence.STPieceSelectionSource.Random;
            long randomSeedMostRecentlyUsedToInitializeRandomNumberGenerator = 0;


            // Cache the random seed and the next piece
            pieceSequenceSource =
                this.mGameState.mSTPieceSequence.DirectGetPieceSelectionSource();

            randomSeedMostRecentlyUsedToInitializeRandomNumberGenerator = 
                this.mGameState.mSTPieceSequence.DirectGetSeedUsedMostRecentlyToInitializeRandomNumberGenerator( );


            // Do a regular reset...
            this.InputEventReset();
            this.ClearAllStatistics();


            // Restore the random seed
            this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
                ( pieceSequenceSource );

            this.mGameState.mSTPieceSequence.ClientRequestSelectionGeneratorReset
                ( randomSeedMostRecentlyUsedToInitializeRandomNumberGenerator );
        }




        


        public void   InputEventHardReset( )
        {
            // Do a regular reset...
            this.InputEventReset();

            // ...and clear other flags and statistics.
            this.mGameState.mSTBoardCurrent.SetBoardDimensions( 10, 20 );
            this.mGameState.mSTBoardCurrent.ClearCells();
            this.mGameState.mSTPieceCurrent.Clear();

            this.mGameState.mGameOver = false;
            this.mGameState.mCurrentPiecePointValue = 0;
            this.mGameState.mCompletedRows = 0;

            // Set piece sequence source to RANDOM
            this.mGameState.mSTPieceSequence.ClientSelectPieceSelectionSource
                ( STPieceSequence.STPieceSelectionSource.Random );
            this.SeedPieceSequenceGeneratorWithCurrentTime( );

            this.mGameState.mPaused                = false;
            this.mGameState.mShowNextPiece         = false;
            this.mGameState.mAI                    = false;
            this.mGameState.mSpawnFromVideoCapture = false;
            this.mGameState.mOutputToRS232         = false;
            this.mGameState.mAutoRestart           = false;
            this.mGameState.mAutoWriteFile         = false;
            this.mGameState.mGameSpeedAdjustment   = 0;
            this.mGameState.mShadowMode            = false;
            this.mGameState.mHintMode              = false;
            this.mGameState.mAnimateAIMovesEnable  = false;
            this.mGameState.mMonochromeColorMode   = false;

            this.ClearAllStatistics();
        }







        public void   InputEventAddRowOfJunk( )
        {
            this.mGameState.mSTBoardCurrent.LiftPileByOneRowAndAddRandomJunk();
        }









        public void   InputEventHintModeStart ( )
        {
            this.mGameState.mHintMode = true;

            this.PrivateUpdateBestMovePiece( );
        }



        public void   InputEventHintModeStop  ( )
        {
            this.mGameState.mHintMode = false;

            // Clear the current "best move" piece
            this.mGameState.mSTPieceBestMove.Clear( );
        }








        public void  InputEventSelectNextAI( )
        {
            STStrategyManager.SelectNextStrategy( );

            // If AI is active, clear the historic row histogram, etc...
            if (true == this.mGameState.mAI)
            {
                this.ClearAllStatistics( );
                this.InputEventReset( ); // Switching AI should switch clean
            }
        }







        public void   InputEventToggleMoveAnimation( )
        {
            if (false == this.mGameState.mAnimateAIMovesEnable)
            {
                this.mGameState.mAnimateAIMovesEnable = true;
            }
            else
            {
                // Set final safe Y to some absurd level so that any remaining
                // moves are forced to occur.
                this.mGameState.mAnimateAIMovesFinalSafeY = 
                    (1 + this.mGameState.mSTBoardCurrent.GetHeight());
                // Do any remaining pending moves...
                this.PrivateAIAnimationProcessing();
                this.mGameState.mAnimateAIMovesEnable = false;
            }

            // In either case, clobber the current state.
            this.mGameState.mAnimateAIMovesStartingY          = 0;
            this.mGameState.mAnimateAIMovesFinalSafeY         = 0;
            this.mGameState.mAnimateAITotalInitialCommands    = 0;
            this.mGameState.mAnimateAICommandsExecuted        = 0;
            this.mGameState.mAnimateAICommandsPerRow          = (0.0);
            this.mGameState.mAnimateAIMovesPendingRotation    = 0;
            this.mGameState.mAnimateAIMovesPendingTranslation = 0;
        }












        public void   ConditionalAdvanceGameTimeByDelta( double deltaSeconds )
        {
            // All this function does is advance the time, conditionally...
            if (true == this.mGameState.mPaused)
            {
                return;
            }

            if (true == this.mGameState.mGameOver)
            {
                return;
            }

            // Advance total elapsed time
            this.mGameState.mTotalElapsedTimeSeconds += deltaSeconds;

            // Decrease remaining countdown
            this.mGameState.mIterationCountdownSeconds -= deltaSeconds;
        }







        public void   ConditionalAdvanceGameIteration( bool forceMove )
        {
            if (true == this.mGameState.mPaused)
            {
                return;
            }

            if (true == this.mGameState.mAI)
            {
                this.PrivateAIProcessing();
            }

            if (true == this.mGameState.mAutoRestart)
            {
                this.PrivateAutoRestartProcessing();
            }

            if (true == this.mGameState.mGameOver)
            {
                return;
            }


            if ((this.mGameState.mIterationCountdownSeconds <= 0.0) || (true == forceMove))
            {
                // If we do not currently have a piece, attempt to spawn a piece...
                if (false == this.mGameState.mSTPieceCurrent.IsValid())
                {
                    // Attempt to spawn a piece.
                    this.PrivateSpawnPiece();

                    // If the spawn occurred, reset the iteration countdown, otherwise
                    // the countdown remains expired, forcing us to try spawning repeatedly
                    // until we succeed.
                    if (true == this.mGameState.mSTPieceCurrent.IsValid())
                    {
                        this.mGameState.mIterationCountdownSeconds = this.PrivateGetCountdownInitialValue();
                    }
                }
                else
                {
                    // We have a piece.  Attempt a free-fall iteration.
                    // Unconditionally reset the countdown.
                    this.PrivateFreeFallPiece();
                    this.mGameState.mIterationCountdownSeconds = this.PrivateGetCountdownInitialValue();
                }
            }
        }







        public void  GetCopyOfNextPiece( STPiece nextPiece )
        {
            nextPiece.CopyFrom( this.mGameState.mSTPieceNext );
        }







        public void GetCopyOfBestPiece( STPiece bestPiece )
        {
            bestPiece.CopyFrom( this.mGameState.mSTPieceBestMove );
        }







        public void   SpawnSpecifiedPieceShape( STPiece.STPieceShape pieceShape )
        {
            // Add piece to queue part of this.mGameState.mSTPieceSequence
            this.mGameState.mSTPieceSequence.ServerQueueSubmitPiece( pieceShape );
        }








        public void PrivateGameReset( )
        {
            int i = 0;
            int n = 0;

            // Game Reset does NOT clobber all parts of the game object!
            // For example, the following are not cleared:
            //   (1) Paused flag;
            //   (2) Historical statistics;
            // However, the following are cleared, for example:
            //   (1) Current board;
            //   (2) Current piece;
            //   (3) Rows Completed;
            //   (4) Score;
            //   (5) Game-Over flag;
            // And the following values are initialized:
            //   (1) Iteration countdown time;


            this.mGameState.mSTBoardCurrent.ClearCells();
            this.mGameState.mSTPieceCurrent.Clear();


            this.mGameState.mGameOver = false;
            this.mGameState.mIterationCountdownSeconds = 0.05;
            this.mGameState.mCurrentPiecePointValue = 0; // starts at 24+(3*(level-1))
            this.mGameState.mCompletedRows = 0;

            // Piece sequence generator
            // Reset queue (even if we aren't using it now)
            this.mGameState.mSTPieceSequence.ServerQueueReset();
            this.SeedPieceSequenceGeneratorWithCurrentTime( );

            // UNCHANGED: this.mGameState.mPaused;
            // UNCHANGED: this.mGameState.mShowNextPiece;
            // UNCHANGED: this.mGameState.mAI;
            // UNCHANGED: this.mGameState.mSpawnFromVideoCapture;
            // UNCHANGED: this.mGameState.mOutputToRS232;
            // UNCHANGED: this.mGameState.mAutoRestart;
            // UNCHANGED: this.mGameState.mGameSpeedAdjustment;

            this.mGameState.mTotalElapsedTimeSeconds = 0.0;

            n = this.mGameState.mPieceHistogram.Length;  
            for ( i = 0; i < n; i++ )
            { 
                this.mGameState.mPieceHistogram [ i ] = 0;
            }

            n = this.mGameState.mHeightHistogram.Length;  
            for ( i = 0; i < n; i++ ) 
            {
                this.mGameState.mHeightHistogram[ i ] = 0;
            }

            this.mGameState.mScore = 0;

            // Only updated when game ends
            // UNCHANGED: this.mGameState.mHistoricHighScore;
            // UNCHANGED: this.mGameState.mHistoricHighRows;
            // UNCHANGED: this.mGameState.mHistoricHighPieces;
            // UNCHANGED: this.mGameState.mHistoricCumulativeRows;
            // UNCHANGED: this.mGameState.mHistoricTotalGames;

  // TO DO:          standard_tetris_time::SetReferenceTimeToNow();

            // Move Animation
            // UNCHANGED: this.mGameState.mAnimateAIMovesEnable;
            this.mGameState.mAnimateAIMovesStartingY          = 0;
            this.mGameState.mAnimateAIMovesFinalSafeY         = 0;
            this.mGameState.mAnimateAITotalInitialCommands    = 0;
            this.mGameState.mAnimateAICommandsExecuted        = 0;
            this.mGameState.mAnimateAICommandsPerRow          = 0.0;
            this.mGameState.mAnimateAIMovesPendingRotation    = 0;
            this.mGameState.mAnimateAIMovesPendingTranslation = 0;
        }








        public double  PrivateGetCountdownInitialValue ( )
        {
            // -2, -3, -4,... : Slow Mode (delay proportional to index)
            // -1 : Normal, Clipped at 0.20 sec/row
            //  0 : Normal
            // +1 : Fast Mode (still render bound)
            // +2 : Very Fast Mode (still render bound)
            // +3, +4, +5,... : Multiple moves per rendered frame

            if (0 == this.mGameState.mGameSpeedAdjustment)
            {
                // Normal Tetris Speed Rule
                return( 0.05 * (double)(11 - this.GetCurrentLevel()) );
            }

            if ((-1) == this.mGameState.mGameSpeedAdjustment)
            {
                // Normal Tetris Speed Rule, but clamped to 0.20
                double delay = ( 0.05 * (double)(11 - this.GetCurrentLevel()) );
                if (delay < 0.20) delay = 0.20;
                return( delay );
            }

            if (this.mGameState.mGameSpeedAdjustment <= (-2))
            {
                // Slowness is proportional to speed adjustment
                double delay = ((0.5) * (-(this.mGameState.mGameSpeedAdjustment)));
                return( delay );
            }

            if ((1) == this.mGameState.mGameSpeedAdjustment)
            {
                return( 0.0 ); // Render speed bound
            }

            if (this.mGameState.mGameSpeedAdjustment >= (2))
            {
                // Render speed bound...
                //   ...but main function also does multiple calls for even more speed
                return( 0.0 ); 
            }

            // We should never get here
            return( 0.5 ); 
        }












        // Might cause "game over" flag
        public void PrivateSpawnPiece( )
        {
            // Regardless of how we got here, clear the current piece.
            this.mGameState.mSTPieceCurrent.Clear();
            this.mGameState.mSTPieceNext.Clear();
            this.mGameState.mCurrentPiecePointValue = 0;

            // Clear the current "best move" piece
            this.mGameState.mSTPieceBestMove.Clear();


            // Don't spawn if the game is over!
            if (this.mGameState.mGameOver)
            {
                return;
            }


            // The ONLY source of the next piece is the piece sequence object.
            // It is possible that the "next piece" and even the current piece
            // are currently unavailable.  If the information is not available,
            // we should leave the current piece cleared, which will force the
            // spawning code to continue attempting to spawn.
            STPiece.STPieceShape currentShape = STPiece.STPieceShape.None;
            STPiece.STPieceShape nextShape = STPiece.STPieceShape.None;
            currentShape = this.mGameState.mSTPieceSequence.ClientPeekSelectedPieceCurrent();
            nextShape = this.mGameState.mSTPieceSequence.ClientPeekSelectedPieceNext();
            this.mGameState.mSTPieceSequence.ClientRequestSelectionUpdate();

            //nextShape = STPiece.STPieceShape.Z;
            //currentShape = STPiece.STPieceShape.Z;

            if (nextShape != STPiece.STPieceShape.None)
            {
                // Set Next Piece (NOTE: Might not be available)
                this.mGameState.mSTPieceNext.SetShape( nextShape );
                this.mGameState.mSTPieceNext.SetX( 0 ); // NOTE: Not used
                this.mGameState.mSTPieceNext.SetY( 0 ); // NOTE: Not used
                this.mGameState.mSTPieceNext.SetOrientation( 1 ); // NOTE: Not used
            }

            if (currentShape == STPiece.STPieceShape.None)
            {
                // No current piece means we should leave and allow this function to
                // be called in the future.
                return;
            }

            // Set current piece
            this.mGameState.mSTPieceCurrent.SetShape( currentShape );
            this.mGameState.mSTPieceCurrent.SetX( this.mGameState.mSTBoardCurrent.GetPieceSpawnX() );
            this.mGameState.mSTPieceCurrent.SetY( this.mGameState.mSTBoardCurrent.GetPieceSpawnY() );
            this.mGameState.mSTPieceCurrent.SetOrientation( 1 );

            // Set the point value for this piece
            this.mGameState.mCurrentPiecePointValue = 
                    24 + (3 * (this.GetCurrentLevel() - 1));

            int currentShapeIndex = 0;
            currentShapeIndex = (int)STPiece.GetByteCodeValueOfShape( currentShape );
            this.mGameState.mPieceHistogram[ currentShapeIndex ]++; // Update histogram


            // If goal (spawn location) is not acceptable, game is over.
            bool okayToSpawn = false;

            okayToSpawn = 
                this.mGameState.mSTBoardCurrent.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells
                (
                    this.mGameState.mSTPieceCurrent
                );

            if (false == okayToSpawn)
            {
                // ********************** GAME OVER! **************************
                this.mGameState.mGameOver = true;

                // Update historical statistics
                
                if (this.mGameState.mScore > this.mGameState.mHistoricHighScore)
                {
                    this.mGameState.mHistoricHighScore = this.mGameState.mScore;
                }

                if (this.mGameState.mCompletedRows > this.mGameState.mHistoricHighRows)
                {
                    this.mGameState.mHistoricHighRows  = this.mGameState.mCompletedRows;
                }

                long totalPieces = 0;
                totalPieces = this.GetPieceShapeHistogramSum();
                if (totalPieces > this.mGameState.mHistoricHighPieces)
                {
                    this.mGameState.mHistoricHighPieces = totalPieces;
                }

                this.mGameState.mHistoricCumulativeRows += this.mGameState.mCompletedRows; // Used for average: rows/games
                this.mGameState.mHistoricTotalGames++;

                int i = 0;
                int n = this.mGameState.mHistoricRows.Length;
                for ( i = (n-1); i >= 1; i-- )
                {
                    this.mGameState.mHistoricRows[i] = this.mGameState.mHistoricRows[i-1];
                }
                this.mGameState.mHistoricRows[0] = this.mGameState.mCompletedRows;
            }
            else
            {
                // Cache the current best move
                this.PrivateUpdateBestMovePiece( );
            }

        }












        // Succeeds if allowed
        public void   PrivateRotatePiece( )
        {
            // Leave if no current piece
            if (false == this.mGameState.mSTPieceCurrent.IsValid()) 
            {
                return;
            }

            // Copy the current piece to a temporary piece, rotate it, and determine if
            // it is entirely on the board and does not overlap any occupied cells.
            STPiece  tempPiece = new STPiece();
            tempPiece.CopyFrom( this.mGameState.mSTPieceCurrent );

            tempPiece.Rotate();

            bool okayToRotate = false;

            okayToRotate = 
                this.mGameState.mSTBoardCurrent.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells
                (
                    tempPiece
                );

            if (true == okayToRotate)
            {
                // Rotation acceptable; Rotate actual piece.
                this.mGameState.mSTPieceCurrent.Rotate();
            }
        }












        // Succeeds if allowed
        public void   PrivateTranslatePiece( int horizontalDirectionSign )
        {
            // Leave if no current piece
            if (false == this.mGameState.mSTPieceCurrent.IsValid()) 
            {
                return;
            }

            // Copy the current piece to a temporary piece, translate it, and determine
            // if it is entirely on the board and does not overlap any occupied cells.
            STPiece  tempPiece = new STPiece();
            tempPiece.CopyFrom( this.mGameState.mSTPieceCurrent );

            if (horizontalDirectionSign < 0)
            {
                tempPiece.Translate( -1, 0 );
            }
            else
            {
                tempPiece.Translate(  1, 0 );
            }

            bool okayToTranslate = false;

            okayToTranslate = 
                this.mGameState.mSTBoardCurrent.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells
                (
                    tempPiece
                );

            if (true == okayToTranslate)
            {
                // Translation acceptable; Translate actual piece.
                if (horizontalDirectionSign < 0)
                {
                    this.mGameState.mSTPieceCurrent.Translate( -1, 0 );
                }
                else
                {
                    this.mGameState.mSTPieceCurrent.Translate(  1, 0 );
                }
            }
        }












        // Decrements point value, possible landing
        public void   PrivateFreeFallPiece( )
        {
            // Leave if no current piece
            if (false == this.mGameState.mSTPieceCurrent.IsValid()) 
            {
                return;
            }

            // Copy the current piece to a temporary piece, translate it, and determine
            // if it is entirely on the board and does not overlap any occupied cells.
            STPiece  tempPiece = new STPiece();
            tempPiece.CopyFrom( this.mGameState.mSTPieceCurrent );

            tempPiece.Translate(  0, -1 );

            bool okayToFall = false;

            okayToFall = 
                this.mGameState.mSTBoardCurrent.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells
                (
                    tempPiece
                );

            if (true == okayToFall)
            {
                // Falling acceptable; Translate actual piece.
                this.mGameState.mSTPieceCurrent.Translate( 0, -1 );
                this.mGameState.mCurrentPiecePointValue--;
            }
            else
            {
                // Translation not acceptable; piece lands!
                this.PrivateTransferPieceToPile();
            }
        }












        // Translates piece as far as it will fall and lands it
        public void PrivateDropPiece( )
        {
            // Leave if no current piece
            if (false == this.mGameState.mSTPieceCurrent.IsValid()) 
            {
                return;
            }

            this.mGameState.mScore += this.mGameState.mCurrentPiecePointValue;
            this.mGameState.mCurrentPiecePointValue = 0;

            // Try increasingly larger dropping distances until failure.
            // Then transfer the piece to the pile...
            this.mGameState.mSTBoardCurrent.FullDropAndCommitPieceToBoard
            ( 
                this.mGameState.mSTPieceCurrent
            );

            // Collapse any completed rows
            this.PrivateCollapseAnyCompletedRows();

            // Clobber current piece
            this.mGameState.mSTPieceCurrent.Clear();

            // Clobber cached best move
            this.mGameState.mSTPieceBestMove.Clear();

            // Increment pile height bin
            int resultingPileHeight = 0;
            resultingPileHeight = this.mGameState.mSTBoardCurrent.GetPileMaxHeight();
            if ((resultingPileHeight >= 0) && (resultingPileHeight < this.mGameState.mHeightHistogram.Length))
            {
                this.mGameState.mHeightHistogram[ resultingPileHeight ]++;
            }
        }













        // Actually puts cells in to board, updates score, clears piece
        public void   PrivateTransferPieceToPile( )
        {
            // Leave if no current piece
            if (false == this.mGameState.mSTPieceCurrent.IsValid()) 
            {
                return;
            }

            if (this.mGameState.mCurrentPiecePointValue > 0)
            this.mGameState.mScore += this.mGameState.mCurrentPiecePointValue;
            this.mGameState.mCurrentPiecePointValue = 0;


            // Transfer piece to current board
            this.mGameState.mSTBoardCurrent.CommitPieceToBoard
            ( 
                this.mGameState.mSTPieceCurrent
            );

            // Collapse any completed rows
            this.PrivateCollapseAnyCompletedRows();

            // Clobber current piece
            this.mGameState.mSTPieceCurrent.Clear();

            // Clobber cached best move
            this.mGameState.mSTPieceBestMove.Clear();

            // Increment pile height bin
            int resultingPileHeight = 0;
            resultingPileHeight = this.mGameState.mSTBoardCurrent.GetPileMaxHeight( );
            if ((resultingPileHeight >= 0) && (resultingPileHeight < this.mGameState.mHeightHistogram.Length))
            {
                this.mGameState.mHeightHistogram[ resultingPileHeight ]++;
            }
        }












        public void   PrivateAIProcessing( )
        {
            // Leave if no current piece
            if (false == this.mGameState.mSTPieceCurrent.IsValid()) 
            {
                return;
            }


            // AI move animation
            if (true == (this.PrivateAIAnimationProcessing()))
            {
                return;
            }



            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // For the moment we do the sub-optimal but reliable thing of
            // waiting for the piece to fall to the first row before giving
            // the AI the chance to decide on a best move.  The border of
            // the board will not block rotations.  (It is still possible
            // for the pile to block rotations.)
            if ( this.mGameState.mSTPieceCurrent.GetY() != 
                (this.mGameState.mSTBoardCurrent.GetHeight() - 1) )
            {
                // Unless the piece is exactly one row below the top of the board,
                // just give up on doing the AI.
                return;
            }
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 
            // HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK HACK 



            // We don't care how the piece came to be on the board
            // (spawned internally or spawned by external source).
            // If there is a piece, we immediately find the best move
            // and execute it (including a drop).
            int bestRotationDelta    = 0;
            int bestTranslationDelta = 0;

            // COPY THE BOARD AND PIECE BEFORE CALLING AI!
            // We don't want AI messing up board or piece under innocent or
            // cheating circumstances.
            STBoard  copyOfCurrentBoard = new STBoard();
            STPiece  copyOfCurrentPiece = new STPiece();

            copyOfCurrentBoard.CopyFrom( this.mGameState.mSTBoardCurrent );
            copyOfCurrentPiece.CopyFrom( this.mGameState.mSTPieceCurrent );

            STStrategyManager.GetBestMoveOncePerPiece
            (
                copyOfCurrentBoard,
                copyOfCurrentPiece,
                this.mGameState.mShowNextPiece,  // Showing "Next Piece" ?
                this.mGameState.mSTPieceNext.GetShape(), // Next piece shape
                ref bestRotationDelta,
                ref bestTranslationDelta
            );


            if (false == this.mGameState.mAnimateAIMovesEnable)
            {
                // ROTATE
                int rotateCount = 0;
                for ( rotateCount = 0; rotateCount < bestRotationDelta; rotateCount++ )
                {
                    this.InputEventRotate();
                }

                // TRANSLATE
                int translateCount = 0;
                if (bestTranslationDelta < 0)
                {
                    for ( translateCount = 0; translateCount > bestTranslationDelta; translateCount-- )
                    {
                        this.InputEventLeft();
                    }
                }
                if (bestTranslationDelta > 0)
                {
                    for ( translateCount = 0; translateCount < bestTranslationDelta; translateCount++ )
                    {
                        this.InputEventRight();
                    }
                }

                // DROP
                this.InputEventDrop();
            }
            else
            {
                // Set up move to be executed in the future.
                this.mGameState.mAnimateAIMovesPendingRotation = bestRotationDelta;
                this.mGameState.mAnimateAIMovesPendingTranslation = bestTranslationDelta;

                this.mGameState.mAnimateAITotalInitialCommands = bestRotationDelta;
                if (bestTranslationDelta > 0)
                { 
                    this.mGameState.mAnimateAITotalInitialCommands += bestTranslationDelta;
                }
                else
                {
                    this.mGameState.mAnimateAITotalInitialCommands -= bestTranslationDelta;
                }

                this.mGameState.mAnimateAICommandsExecuted = 0;

                this.mGameState.mAnimateAIMovesStartingY = this.mGameState.mSTPieceCurrent.GetY();

                this.mGameState.mAnimateAIMovesFinalSafeY = 
                    (this.mGameState.mSTBoardCurrent.GetPileMaxHeight() + 4); // +3 + paranoia!

                int fallRows = 0;
                fallRows = (this.mGameState.mAnimateAIMovesStartingY -
                    this.mGameState.mAnimateAIMovesFinalSafeY);

                // Worst-case scenario: Execute all moves now!
                this.mGameState.mAnimateAICommandsPerRow = 
                    (double)(1.0 + this.mGameState.mAnimateAITotalInitialCommands);

                // If we can fall free for some rows, then compute the number of
                // commands per row we must execute, with a slight over-estimate.
                if (fallRows > 0)
                {
                    this.mGameState.mAnimateAICommandsPerRow = 
                        (double)(1 + this.mGameState.mAnimateAITotalInitialCommands)
                        / (double)(fallRows);
                }

                // Attempt processing immediately!
                // REASON: If the game is very fast, there may only be one cycle
                // before the piece falls by one row, which could terminate the
                // game.  Thus, we have to be able to act immediately, as if the
                // animation effect were turned off.  Otherwise, the performance
                // of the AI would depend on whether or not the animation effect
                // was on, which is bad.
                this.PrivateAIAnimationProcessing();
            }
        }












        public bool PrivateAIAnimationProcessing( )
        {
            if (false == this.mGameState.mAnimateAIMovesEnable)  
            {
                return(false);
            }

            if ((0 == this.mGameState.mAnimateAIMovesPendingRotation) &&
                (0 == this.mGameState.mAnimateAIMovesPendingTranslation))
            {
                return(false);
            }


            int currentY = 0;
            currentY = this.mGameState.mSTPieceCurrent.GetY();


            // If we reached the final Y, force the execution of all remaining
            // commands.
            if (currentY <= this.mGameState.mAnimateAIMovesFinalSafeY)
            {
                // Execute all moves now!
                while (this.mGameState.mAnimateAIMovesPendingRotation > 0)
                {
                    this.InputEventRotate();
                    this.mGameState.mAnimateAIMovesPendingRotation--;
                    this.mGameState.mAnimateAICommandsExecuted++;
                }

                while (this.mGameState.mAnimateAIMovesPendingTranslation != 0)
                {
                    if (this.mGameState.mAnimateAIMovesPendingTranslation < 0)
                    {
                        this.InputEventLeft();
                        this.mGameState.mAnimateAIMovesPendingTranslation++;
                        this.mGameState.mAnimateAICommandsExecuted++;
                    }
                    else if (this.mGameState.mAnimateAIMovesPendingTranslation > 0)
                    {
                        this.InputEventRight();
                        this.mGameState.mAnimateAIMovesPendingTranslation--;
                        this.mGameState.mAnimateAICommandsExecuted++;
                    }
                }

                return(true);
            }


            // Compute the theoretical number of commands that we should execute
            // up to this point in time.
            int droppedRows = 0;
            droppedRows = (this.mGameState.mAnimateAIMovesStartingY - currentY);

            int goalExecutedCommands = 0;
            goalExecutedCommands = 
                (int)(this.mGameState.mAnimateAICommandsPerRow * (double)droppedRows);

            // Execute Rotations until either rotations are exhausted, or
            // the total number of executed commands reaches the desired level.
            while 
                (
                ((this.mGameState.mAnimateAICommandsExecuted) < (goalExecutedCommands)) &&
                (this.mGameState.mAnimateAIMovesPendingRotation > 0) 
                )
            {
                this.InputEventRotate();
                this.mGameState.mAnimateAIMovesPendingRotation--;
                this.mGameState.mAnimateAICommandsExecuted++;
            }

            while 
                (
                ((this.mGameState.mAnimateAICommandsExecuted) < (goalExecutedCommands)) &&
                (this.mGameState.mAnimateAIMovesPendingTranslation != 0) 
                )
            {
                if (this.mGameState.mAnimateAIMovesPendingTranslation < 0)
                {
                    this.InputEventLeft();
                    this.mGameState.mAnimateAIMovesPendingTranslation++;
                    this.mGameState.mAnimateAICommandsExecuted++;
                }
                else if (this.mGameState.mAnimateAIMovesPendingTranslation > 0)
                {
                    this.InputEventRight();
                    this.mGameState.mAnimateAIMovesPendingTranslation--;
                    this.mGameState.mAnimateAICommandsExecuted++;
                }
            }

            return(true);
        }












        public void   PrivateAutoRestartProcessing( )
        {
            if (false == this.mGameState.mAutoRestart)
            {
                return;
            }

            if (false == this.mGameState.mGameOver)
            {
                return;
            }

            // Write file if auto-write file is enabled.
            if (true == this.mGameState.mAutoWriteFile)
            {
                this.InputEventGameStateWriteToFile();
            }

            // Reset the game.
            this.InputEventReset();
        }













        // Affects total rows
        public void   PrivateCollapseAnyCompletedRows( )
        {
            int totalRowsCollapsed = 0;

            totalRowsCollapsed = 
                this.mGameState.mSTBoardCurrent.CollapseAnyCompletedRows();

            if (totalRowsCollapsed > 0)
            {
                // We completed one or more rows!
                this.mGameState.mCompletedRows += totalRowsCollapsed;
            }
        }












        public void  PrivateUpdateBestMovePiece( )
        {
            // Clear the current "best move" piece
            this.mGameState.mSTPieceBestMove.Clear();

            if (false == this.mGameState.mHintMode) 
            {
                return;
            }


            // Set some basic initial conditions
            this.mGameState.mSTPieceBestMove.SetShape( this.mGameState.mSTPieceCurrent.GetShape() );
            this.mGameState.mSTPieceBestMove.SetX( this.mGameState.mSTBoardCurrent.GetPieceSpawnX() );
            this.mGameState.mSTPieceBestMove.SetY( this.mGameState.mSTBoardCurrent.GetPieceSpawnY() - 1);
            this.mGameState.mSTPieceBestMove.SetOrientation( 1 );

            int bestRotationDelta    = 0;
            int bestTranslationDelta = 0;

            STStrategyManager.GetBestMoveOncePerPiece
            (
                this.mGameState.mSTBoardCurrent,
                this.mGameState.mSTPieceBestMove,
                GameIsShowNextPiece(),
                this.mGameState.mSTPieceNext.GetShape(),
                ref bestRotationDelta,
                ref bestTranslationDelta
            );

            // Apply movement to piece
            this.mGameState.mSTPieceBestMove.RotateByCount( bestRotationDelta );
            this.mGameState.mSTPieceBestMove.Translate( bestTranslationDelta, 0 );

            // Drop piece
            this.mGameState.mSTBoardCurrent.DropPieceAsFarAsPossibleButDoNotModifyBoard
            ( 
                this.mGameState.mSTPieceBestMove
            );
        }










        public void GetGeneratorSeed
        (
            ref long randomSeedMostRecentlyUsedToInitializeRandomNumberGenerator
        )
        {
            randomSeedMostRecentlyUsedToInitializeRandomNumberGenerator =
                this.mGameState.mSTPieceSequence.DirectGetSeedUsedMostRecentlyToInitializeRandomNumberGenerator( );
        }












        public void SetGeneratorSeed
        (
            long seedValue
        )
        {
            this.mGameState.mSTPieceSequence.ClientRequestSelectionGeneratorReset( seedValue );
        }












        public void  SeedPieceSequenceGeneratorWithCurrentTime( )
        {
            // Seed the sequence generator with the time
            long seedValue = 0;

            // Simply use the raw counter value (64-bit value) as the seed.
            STPrecisionTimer.Kernel32_QueryPerformanceCounter( out seedValue );

            // Seed the generator
            this.SetGeneratorSeed( seedValue );
        }












        public void ClearAllStatistics( )
        {
            this.ClearHistoricRowsHistogram();

            int i = 0;
            int n = 0;
            
            n = this.mGameState.mPieceHistogram.Length;
            for (i = 0; i < n; i++)
            {
                this.mGameState.mPieceHistogram[i] = 0;
            }

            n = this.mGameState.mHeightHistogram.Length;
            for (i = 0; i < n; i++)
            {
                this.mGameState.mHeightHistogram[i] = 0;
            }
            
            n = this.mGameState.mHistoricRows.Length;
            for (i = 0; i < n; i++)
            {
                this.mGameState.mHistoricRows[i] = 0;
            }

            this.mGameState.mTotalElapsedTimeSeconds = 0.0;
            this.mGameState.mHistoricHighScore      = 0;
            this.mGameState.mHistoricHighRows       = 0;
            this.mGameState.mHistoricHighPieces     = 0;
            this.mGameState.mHistoricCumulativeRows = 0;
            this.mGameState.mHistoricTotalGames     = 0;
        }


        



















public static String GetApplicationName()
{
    return( "Standard Tetris" );
}





public void ClearPreviousClassification()
{
    this.mGameState.mPreviousClassification = 0;
}


public bool GetCalibrationModeFlagValue()
{
    return (this.mGameState.mCalibrationModeFlag);
}


public void SetCalibrationModeFlagValue( bool flag )
{
    this.mGameState.mCalibrationModeFlag = flag;
}


        public void SetCalibrationModeShapeCode ( int value )
{
    this.mGameState.mCalibrationModeShapeCode = value;
}








// instruction control
public int  InstructionGetState()
{
    return( this.mGameState.mShowInstructionPage ); 
}


public void InstructionsShow()
{
    this.mGameState.mShowInstructionPage = 1;
}


public void InstructionsHide()
{
this.mGameState.mShowInstructionPage = 0;
}


public void InstructionsNextPage()
{
this.mGameState.mShowInstructionPage++; 
}


public void InstructionsPreviousPage()
{
if (this.mGameState.mShowInstructionPage > 1)
  {
  this.mGameState.mShowInstructionPage--;
  }
}




public void ConsoleShow()
{
    this.mGameState.mShowConsole = true;
}

public void ConsoleHide()
{
    this.mGameState.mShowConsole = false;
}






public void SetReportedFrameRate
(
    float reportedFrameRate
)
{
    this.mGameState.mReportedFrameRate = reportedFrameRate;
}

public float GetReportedFrameRate()
{
    return( this.mGameState.mReportedFrameRate ); 
}














    }
}
