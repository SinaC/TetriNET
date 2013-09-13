// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;


namespace CPF.StandardTetris
{
    public class STGameFile
    {

        public static String GetDateStringYYYYMMDD_HHMMSSMMM ( )
        {
            DateTime dateTime = DateTime.Now;

            String stringYYYYMMDD_HHMMSSMMM = "";

            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d4}", dateTime.Year );
            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d2}", dateTime.Month );
            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d2}", dateTime.Day );
            stringYYYYMMDD_HHMMSSMMM += "_";
            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d2}", dateTime.Hour );
            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d2}", dateTime.Minute );
            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d2}", dateTime.Second );
            stringYYYYMMDD_HHMMSSMMM += String.Format( "{0:d3}", dateTime.Millisecond );

            return (stringYYYYMMDD_HHMMSSMMM);
        }


        public static String GetDateStringYYYY_MMM_DD_HH_MM_SS_MMM ( )
        {
            DateTime dateTime = DateTime.Now;

            String month = "";

            switch (dateTime.Month)
            {
                case 1: month = "Jan"; break;
                case 2: month = "Feb"; break;
                case 3: month = "Mar"; break;
                case 4: month = "Apr"; break;
                case 5: month = "May"; break;
                case 6: month = "Jun"; break;
                case 7: month = "Jul"; break;
                case 8: month = "Aug"; break;
                case 9: month = "Sep"; break;
                case 10: month = "Oct"; break;
                case 11: month = "Nov"; break;
                case 12: month = "Dec"; break;
            }


            String stringYYYY_MMM_DD_HH_MM_SS_MMM = "";


            stringYYYY_MMM_DD_HH_MM_SS_MMM += String.Format( "{0:d4}", dateTime.Year );
            stringYYYY_MMM_DD_HH_MM_SS_MMM += " ";
            stringYYYY_MMM_DD_HH_MM_SS_MMM += month;
            stringYYYY_MMM_DD_HH_MM_SS_MMM += " ";
            stringYYYY_MMM_DD_HH_MM_SS_MMM += String.Format( "{0:d2}", dateTime.Day );
            stringYYYY_MMM_DD_HH_MM_SS_MMM += " ";
            stringYYYY_MMM_DD_HH_MM_SS_MMM += String.Format( "{0:d2}", dateTime.Hour );
            stringYYYY_MMM_DD_HH_MM_SS_MMM += ":";
            stringYYYY_MMM_DD_HH_MM_SS_MMM += String.Format( "{0:d2}", dateTime.Minute );
            stringYYYY_MMM_DD_HH_MM_SS_MMM += ":";
            stringYYYY_MMM_DD_HH_MM_SS_MMM += String.Format( "{0:d2}", dateTime.Second );
            stringYYYY_MMM_DD_HH_MM_SS_MMM += ".";
            stringYYYY_MMM_DD_HH_MM_SS_MMM += String.Format( "{0:d3}", dateTime.Millisecond );

            return (stringYYYY_MMM_DD_HH_MM_SS_MMM);
        }



        public static String GetDefaultFileName ( )
        {
            // File name is "tetris_state_YYYYMMDD_HHMMSSMMM.txt"

            string fileName = "";
            fileName += "tetris_state_";
            fileName += STGameFile.GetDateStringYYYYMMDD_HHMMSSMMM( );
            fileName += ".txt";

            return (fileName);
        }







        public static bool WriteGameStateToFile
        (
            String filePath, // e.g., C:\ or C:\foo
            String fileName, // e.g., tetris_state_YYYYMMDD_HHMMSSMMM.txt
            STGameState gameState
        )
        {

            if (true == filePath.EndsWith( "\\" ))
            {
                filePath = filePath.Substring( 0, (filePath.Length - 1) );
            }


            String filePathAndName = "";
            filePathAndName = filePath + "\\" + fileName;



            STFileWriter file = new STFileWriter( );

            bool successfullyOpenedFile = false;

            successfullyOpenedFile = file.Open( filePathAndName );

            if (false == successfullyOpenedFile)
            {
                return (false);
            }



            // Simply print out game state information to the text file.
            // The order of fields doesn't matter.  Fields can be added
            // or removed.  It is the consumer's responsibility to 
            // acquire any fields and compensate for "missing" fields.
            // NOTE: Not all of these fields are intended to be restored
            // on loading.  Many are DERIVED values, or are session-dependent,
            // or should be cleared upon loading to give the user the 
            // chance to alter settings.  For example, don't restore the
            // pause/unpaused state, or the video capture state.



            file.WriteText( "fileName                 \"" + fileName + "\"\r\n" );
            file.WriteText( "localDateAndTime         \"" + GetDateStringYYYY_MMM_DD_HH_MM_SS_MMM( ) + "\"\r\n" );
            file.WriteText( "standardTetrisVersion    \"" + STGame.GetApplicationName( ) + "\"\r\n" );


            file.WriteText( "\r\n" );


            file.WriteText( "boardWidth  " + gameState.mSTBoardCurrent.GetWidth( ) + "\r\n" );
            file.WriteText( "boardHeight " + gameState.mSTBoardCurrent.GetHeight( ) + "\r\n" );

            file.WriteText( "boardCurrent\r\n" );

            int width = 0;
            int height = 0;
            width = gameState.mSTBoardCurrent.GetWidth( );
            height = gameState.mSTBoardCurrent.GetHeight( );

            int x = 0;
            int y = 0;
            for (y = height; y >= 1; y--) // top-down
            {
                for (x = 1; x <= width; x++)
                {
                    int cellShapeCode = 0;
                    cellShapeCode = (int)gameState.mSTBoardCurrent.GetCell( x, y );
                    file.WriteText( " " + String.Format( "{0:d}", cellShapeCode ) );
                }
                file.WriteText( "\r\n" );
            }
            file.WriteText( "\r\n" );




            file.WriteText( "pieceCurrent\r\n" );
            file.WriteText( "shape " + (int)gameState.mSTPieceCurrent.GetByteCodeValue( ) + "\r\n" );
            file.WriteText( "x " + gameState.mSTPieceCurrent.GetX( ) + "\r\n" );
            file.WriteText( "y " + gameState.mSTPieceCurrent.GetY( ) + "\r\n" );
            file.WriteText( "orientation " + gameState.mSTPieceCurrent.GetOrientation( ) + "\r\n" );
            file.WriteText( "\r\n" );




            int intValue = 0;

            if (true == gameState.mGameOver) intValue = 1;
            file.WriteText( "gameOver " + intValue + "\r\n" );

            file.WriteText( "iterationCountdownSeconds " + gameState.mIterationCountdownSeconds + "\r\n" );
            file.WriteText( "currentPiecePointValue " + gameState.mCurrentPiecePointValue + "\r\n" );
            file.WriteText( "completedRows " + gameState.mCompletedRows + "\r\n" );
            file.WriteText( "\r\n" );





            file.WriteText( "pieceSelectionSource " + (int)gameState.mSTPieceSequence.DirectGetPieceSelectionSource( ) + "\r\n" );
            file.WriteText( "pieceSelectionShapeCurrent " + (int)gameState.mSTPieceSequence.DirectGetCurrentPieceShape( ) + "\r\n" );
            file.WriteText( "pieceSelectionShapeNext " + (int)gameState.mSTPieceSequence.DirectGetNextPieceShape( ) + "\r\n" );

            file.WriteText( "cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator " + gameState.mSTPieceSequence.DirectGetSeedUsedMostRecentlyToInitializeRandomNumberGenerator( ) + "\r\n" );


            file.WriteText( "currentInternalRandomNumberGeneratorStateForPieceSequence " + gameState.mSTPieceSequence.DirectGetCurrentRandomNumberGeneratorInternalStateValue( ) + "\r\n" );


            int alternateSZState = 0;
            if (true == gameState.mSTPieceSequence.DirectGetAlternateSZState( )) alternateSZState = 1;
            file.WriteText( "alternatingSZState " + alternateSZState + "\r\n" );

            file.WriteText( "pieceSequenceTotalQueueElements " + gameState.mSTPieceSequence.DirectGetTotalQueueElements( ) + "\r\n" );

            file.WriteText( "pieceSequenceQueueElements " + "\r\n" );

            int totalQueueElements = 0;
            totalQueueElements = gameState.mSTPieceSequence.DirectGetTotalQueueElements( );

            int indexQueueElement = 0;
            for (indexQueueElement = 0; indexQueueElement < totalQueueElements; indexQueueElement++)
            {
                int queueElementValue = 0;
                queueElementValue = gameState.mSTPieceSequence.DirectGetQueueElementByIndex( indexQueueElement );
                file.WriteText( " " + String.Format( "{0:d}", queueElementValue ) );
                if (31 == (indexQueueElement % 32))
                {
                    file.WriteText( "\r\n" );
                }
            }
            file.WriteText( "\r\n" );
            file.WriteText( "\r\n" );


            // User options

            intValue = 0;
            if (true == gameState.mPaused) intValue = 1;
            file.WriteText( "paused " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mShowNextPiece) intValue = 1;
            file.WriteText( "showNextPiece " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mAI) intValue = 1;
            file.WriteText( "aiActive " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mSpawnFromVideoCapture) intValue = 1;
            file.WriteText( "spawnFromVideoCapture " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mOutputToRS232) intValue = 1;
            file.WriteText( "outputToRS232 " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mAutoRestart) intValue = 1;
            file.WriteText( "autoRestart " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mAutoWriteFile) intValue = 1;
            file.WriteText( "autoWriteFile " + intValue + "\r\n" );

            file.WriteText( "gameSpeedAdjustment " + gameState.mGameSpeedAdjustment + "\r\n" );

            intValue = 0;
            if (true == gameState.mShadowMode) intValue = 1;
            file.WriteText( "shadowMode " + intValue + "\r\n" );

            intValue = 0;
            if (true == gameState.mHintMode) intValue = 1;
            file.WriteText( "hintMode " + intValue + "\r\n" );
            
            intValue = 0;
            if (true == gameState.mMonochromeColorMode) intValue = 1;
            file.WriteText( "monochromeColorMode " + intValue + "\r\n" );


            // Currently-selected AI
            // (be sure to eliminate the double-quotes around the name when reading back in)
            file.WriteText( "currentStrategyName " + "\"" + STStrategyManager.GetCurrentStrategyName( ) + "\"" + "\r\n" );






            // Statistics for User Consideration Only

            file.WriteText( "\r\n" );

            file.WriteText( "pieceHistogramLength " + (int)gameState.mPieceHistogram.Length + "\r\n" );
            file.WriteText( "pieceHistogram " + "\r\n" );
            int i = 0;
            int n = 0;
            n = gameState.mPieceHistogram.Length;
            for (i = 0; i < n; i++)
            {
                file.WriteText( gameState.mPieceHistogram[i] + "\r\n" );
            }
            file.WriteText( "\r\n" );
            file.WriteText( "\r\n" );







            // Height histogram 
            // (find highest index with a non-zero count, and only show indices
            // up to that index on their own lines, with all subsequent zero 
            // values condensed with 32 such values on a single line)

            file.WriteText( "heightHistogramLength " + (int)gameState.mHeightHistogram.Length + "\r\n" );

            file.WriteText( "heightHistogram " + "\r\n" );

            int maxIndexWithNonZeroCount = (-1);
            n = gameState.mHeightHistogram.Length;
            for (i = 0; i < n; i++)
            {
                if (gameState.mHeightHistogram[i] > 0)
                {
                    maxIndexWithNonZeroCount = i;
                }
            }

            for (i = 0; i <= maxIndexWithNonZeroCount; i++)
            {
                file.WriteText( gameState.mHeightHistogram[i] + "\r\n" );
            }

            int k = 0;
            for (i = (maxIndexWithNonZeroCount+1); i < n; i++)
            {
                file.WriteText( gameState.mHeightHistogram[i] + " " );
                k++;
                if (0 == (k % 32))
                {
                    file.WriteText( "\r\n" );
                }
            }

            file.WriteText( "\r\n" );
            file.WriteText( "\r\n" );




            file.WriteText( "totalElapsedTimeSeconds " + gameState.mTotalElapsedTimeSeconds + "\r\n" );

            file.WriteText( "score " + gameState.mScore + "\r\n" );

            file.WriteText( "historicHighScore " + gameState.mHistoricHighScore + "\r\n" );
            file.WriteText( "historicHighRows " + gameState.mHistoricHighRows + "\r\n" );
            file.WriteText( "historicHighPieces " + gameState.mHistoricHighPieces + "\r\n" );
            file.WriteText( "historicCumulativeRows " + gameState.mHistoricCumulativeRows + "\r\n" );
            file.WriteText( "historicTotalGames " + gameState.mHistoricTotalGames + "\r\n" );

            file.WriteText( "\r\n" );

            file.WriteText( "historicRowsLength " + (int)gameState.mHistoricRows.Length + "\r\n" );
            file.WriteText( "historicRows " + "\r\n" );
            n = gameState.mHistoricRows.Length;
            for (i = 0; i < n; i++)
            {
                file.WriteText( gameState.mHistoricRows[i] + "\r\n" );
            }
            file.WriteText( "\r\n" );
            file.WriteText( "\r\n" );



            // Derived or copied values

            file.WriteText( "pieceNext\r\n" );
            file.WriteText( "shape " + (int)gameState.mSTPieceNext.GetByteCodeValue( ) + "\r\n" );
            file.WriteText( "x " + gameState.mSTPieceNext.GetX( ) + "\r\n" );
            file.WriteText( "y " + gameState.mSTPieceNext.GetY( ) + "\r\n" );
            file.WriteText( "orientation " + gameState.mSTPieceNext.GetOrientation( ) + "\r\n" );
            file.WriteText( "\r\n" );

            file.WriteText( "pieceBestMove\r\n" );
            file.WriteText( "shape " + (int)gameState.mSTPieceBestMove.GetByteCodeValue( ) + "\r\n" );
            file.WriteText( "x " + gameState.mSTPieceBestMove.GetX( ) + "\r\n" );
            file.WriteText( "y " + gameState.mSTPieceBestMove.GetY( ) + "\r\n" );
            file.WriteText( "orientation " + gameState.mSTPieceBestMove.GetOrientation( ) + "\r\n" );
            file.WriteText( "\r\n" );


            // state of animation of a an AI-executed move
            intValue = 0;
            if (true == gameState.mAnimateAIMovesEnable) intValue = 1;
            file.WriteText( "animateAIMovesEnable " + intValue + "\r\n" );
            file.WriteText( "animateAIMovesStartingY " + gameState.mAnimateAIMovesStartingY + "\r\n" );
            file.WriteText( "animateAIMovesFinalSafeY " + gameState.mAnimateAIMovesFinalSafeY + "\r\n" );
            file.WriteText( "animateAITotalInitialCommands " + gameState.mAnimateAITotalInitialCommands + "\r\n" );
            file.WriteText( "animateAICommandsExecuted " + gameState.mAnimateAICommandsExecuted + "\r\n" );
            file.WriteText( "animateAICommandsPerRow " + gameState.mAnimateAICommandsPerRow + "\r\n" );
            file.WriteText( "animateAIMovesPendingRotation " + gameState.mAnimateAIMovesPendingRotation + "\r\n" );
            file.WriteText( "animateAIMovesPendingTranslation " + gameState.mAnimateAIMovesPendingTranslation + "\r\n" );


            // Derived values

            // Scaled height histogram

            file.WriteText( "\r\n" );
            file.WriteText( "\r\n" );

            long sumOfCounts = 0;
            n = gameState.mHeightHistogram.Length;
            for (i = 0; i < n; i++)
            {
                sumOfCounts += gameState.mHeightHistogram[i];
            }

            if (sumOfCounts > 0)
            {
                file.WriteText( "scaledHistogramOfPileHeights " + "\r\n" );

                double fraction = 0.0;

                for (i = 0; i <= maxIndexWithNonZeroCount; i++)
                {
                    fraction = ((double)gameState.mHeightHistogram[i] / (double)sumOfCounts);
                    file.WriteText( "" + fraction + "\r\n" );
                }
            }



            file.WriteText( "\r\n" );
            file.WriteText( "\r\n" );


            file.Close( );

            return (true);
        }











        public static bool ReadGameStateFromFile
        (
            String filePathAndName,
            STGameState gameState
        )
        {

            STFileReader file = new STFileReader( );

            bool successfullyOpenedFile = false;

            successfullyOpenedFile =
                file.ReadFileAndGetClusterStrings( filePathAndName );

            if (false == successfullyOpenedFile)
            {
                return (false);
            }

            int totalStrings = 0;
            totalStrings = file.GetTotalStrings( );

            int indexString = 0;

            int offset = 0;

            int intValue = 0;

            long longValue = 0;



            // BOARD 

            int boardWidth = 0;
            file.FindTextAndInteger32( "boardWidth", ref boardWidth );

            int boardHeight = 0;
            file.FindTextAndInteger32( "boardHeight", ref boardHeight );

            int x = 0;
            int y = 0;
            if ((boardWidth > 0) && (boardHeight > 0))
            {
                gameState.mSTBoardCurrent.SetBoardDimensions( boardWidth, boardHeight );

                indexString = file.FindString( "boardCurrent" );
                if (indexString >= 0)
                {
                    for (y = boardHeight; y >= 1; y--) // top-down
                    {
                        for (x = 1; x <= boardWidth; x++)
                        {
                            // string at (index + 1) corresponds to
                            //   ( x = 0, y = boardHeight )

                            offset = 
                                (1 + ((boardWidth * (boardHeight - y)) + (x - 1)));

                            intValue = 
                                file.GetStringAsInteger32ByIndex( indexString + offset );

                            gameState.mSTBoardCurrent.SetCell( x, y, (byte)intValue );
                        }
                    }
                }
            }




            int shape = 0;
            int orientation = 0;

            // CURRENT PIECE

            gameState.mSTPieceCurrent.Clear( );

            indexString = file.FindString( "pieceCurrent" );

            if (indexString >= 0)
            {
                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "shape", ref shape );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "x", ref x );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "y", ref y );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "orientation", ref orientation );

                gameState.mSTPieceCurrent.SetShape( (STPiece.STPieceShape) shape );
                gameState.mSTPieceCurrent.SetX( x );
                gameState.mSTPieceCurrent.SetY( y );
                gameState.mSTPieceCurrent.SetOrientation( orientation );
            }



            // BASIC GAME STATE

            bool gameOver = false;
            file.FindTextAndInteger32( "gameOver", ref intValue );
            if (0 != intValue) { gameOver = true; }
            gameState.mGameOver = gameOver;

            double iterationCountdownSeconds = 0.0;
            file.FindTextAndDouble( "iterationCountdownSeconds", ref iterationCountdownSeconds );
            gameState.mIterationCountdownSeconds = iterationCountdownSeconds;

            long currentPiecePointValue = 0;
            file.FindTextAndInteger64( "currentPiecePointValue", ref currentPiecePointValue );
            gameState.mCurrentPiecePointValue = currentPiecePointValue;

            long completedRows = 0;
            file.FindTextAndInteger64( "completedRows", ref completedRows );
            gameState.mCompletedRows = completedRows;



            // PIECE SELECTION STATE

            int pieceSelectionSource = 0;
            file.FindTextAndInteger32( "pieceSelectionSource", ref pieceSelectionSource );
            gameState.mSTPieceSequence.DirectSetPieceSelectionSource( (STPieceSequence.STPieceSelectionSource)pieceSelectionSource );

            int pieceSelectionShapeCurrent = 0;
            file.FindTextAndInteger32( "pieceSelectionShapeCurrent", ref pieceSelectionShapeCurrent );
            gameState.mSTPieceSequence.DirectSetCurrentPieceShape((STPiece.STPieceShape)pieceSelectionShapeCurrent );

            int pieceSelectionShapeNext = 0;
            file.FindTextAndInteger32( "pieceSelectionShapeNext", ref pieceSelectionShapeNext );
            gameState.mSTPieceSequence.DirectSetNextPieceShape( (STPiece.STPieceShape)pieceSelectionShapeNext );

            long cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator = 0;
            file.FindTextAndInteger64( "cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator", ref cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator );
            gameState.mSTPieceSequence.DirectSetSeedUsedMostRecentlyToInitializeRandomNumberGenerator( cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator );

            long currentInternalRandomNumberGeneratorStateForPieceSequence = 0;
            file.FindTextAndInteger64( "currentInternalRandomNumberGeneratorStateForPieceSequence", ref currentInternalRandomNumberGeneratorStateForPieceSequence );
            gameState.mSTPieceSequence.DirectSetCurrentRandomNumberGeneratorInternalStateValue( currentInternalRandomNumberGeneratorStateForPieceSequence );

            bool alternatingSZState = false;
            file.FindTextAndInteger32( "alternatingSZState", ref intValue );
            if (0 != intValue) { alternatingSZState = true; }
            gameState.mSTPieceSequence.DirectSetAlternateSZState( alternatingSZState );

            gameState.mSTPieceSequence.DirectQueueClear( );

            int pieceSequenceTotalQueueElements = 0;
            file.FindTextAndInteger32( "pieceSequenceTotalQueueElements", ref pieceSequenceTotalQueueElements );

            indexString = file.FindString( "pieceSequenceQueueElements" );
            if ((pieceSequenceTotalQueueElements > 0) && (indexString >= 0))
            {
                for (offset = 0; offset < pieceSequenceTotalQueueElements; offset++)
                {
                    intValue = file.GetStringAsInteger32ByIndex( (indexString + 1) + offset );
                    gameState.mSTPieceSequence.DirectAddQueueElement( intValue );
                }
            }




            // USER OPTIONS

            
            // DO NOT RESTORE PAUSED STATE
            // bool paused = false;
            // file.FindTextAndInteger32( "paused", ref intValue );
            // if (0 != intValue) { paused = true; }
            // gameState.mPaused = paused;
            gameState.mPaused = true; // FORCE TO BE PAUSED UPON LOAD


            bool showNextPiece = false;
            file.FindTextAndInteger32( "showNextPiece", ref intValue );
            if (0 != intValue) { showNextPiece = true; }
            gameState.mShowNextPiece = showNextPiece;

            bool aiActive = false;
            file.FindTextAndInteger32( "aiActive", ref intValue );
            if (0 != intValue) { aiActive = true; }
            gameState.mAI = aiActive;

            bool spawnFromVideoCapture = false;
            file.FindTextAndInteger32( "spawnFromVideoCapture", ref intValue );
            if (0 != intValue) { spawnFromVideoCapture = true; }
            gameState.mSpawnFromVideoCapture = spawnFromVideoCapture;

            bool outputToRS232 = false;
            file.FindTextAndInteger32( "outputToRS232", ref intValue );
            if (0 != intValue) { outputToRS232 = true; }
            gameState.mOutputToRS232 = outputToRS232;

            bool autoRestart = false;
            file.FindTextAndInteger32( "autoRestart", ref intValue );
            if (0 != intValue) { autoRestart = true; }
            gameState.mAutoRestart = autoRestart;

            bool autoWriteFile = false;
            file.FindTextAndInteger32( "autoWriteFile", ref intValue );
            if (0 != intValue) { autoWriteFile = true; }
            gameState.mAutoWriteFile = autoWriteFile;

            int gameSpeedAdjustment = 0;
            file.FindTextAndInteger32( "gameSpeedAdjustment", ref gameSpeedAdjustment );
            gameState.mGameSpeedAdjustment = gameSpeedAdjustment;

            bool shadowMode = false;
            file.FindTextAndInteger32( "shadowMode", ref intValue );
            if (0 != intValue) { shadowMode = true; }
            gameState.mShadowMode = shadowMode;

            bool hintMode = false;
            file.FindTextAndInteger32( "hintMode", ref intValue );
            if (0 != intValue) { hintMode = true; }
            gameState.mHintMode = hintMode;

            bool monochromeColorMode = false;
            file.FindTextAndInteger32( "monochromeColorMode", ref intValue );
            if (0 != intValue) { monochromeColorMode = true; }
            gameState.mMonochromeColorMode = monochromeColorMode;




            String currentStrategyName = "";
            indexString = file.FindString( "currentStrategyName" );
            if (indexString >= 0)
            {
                int doubleQuoteCount = 0;
                for 
                    (
                    offset = 1; 
                    ((offset < 64) && (doubleQuoteCount < 2) && ((indexString + offset) < totalStrings)); 
                    offset++
                    )
                {
                    String stringTemp = "";
                    file.GetStringByIndex( (indexString + offset), ref stringTemp );
                    if (offset > 1)
                    {
                        currentStrategyName += " ";
                    }
                    if (true == stringTemp.Contains( "\"" ))
                    {
                        doubleQuoteCount++;
                        stringTemp = stringTemp.Replace( "\"", "" );
                    }
                    currentStrategyName += stringTemp;
                }
            }
            STStrategyManager.SetCurrentStrategyByName( currentStrategyName );





            // PIECE HISTOGRAM

            for ( offset = 0; offset < gameState.mPieceHistogram.Length; offset++ )
            {
                gameState.mPieceHistogram[offset] = 0;
            }

            int pieceHistogramLength = 0;
            file.FindTextAndInteger32( "pieceHistogramLength", ref pieceHistogramLength );

            if (pieceHistogramLength >= gameState.mPieceHistogram.Length)
            {
                indexString = file.FindString( "pieceHistogram" );
                if (indexString >= 0)
                {
                    for (offset = 0; offset < gameState.mPieceHistogram.Length; offset++)
                    {
                        longValue = file.GetStringAsInteger64ByIndex( (indexString + 1) + offset );

                        gameState.mPieceHistogram[offset] = longValue;
                    }
                }
            }





            // HEIGHT HISTOGRAM

            for (offset = 0; offset < gameState.mHeightHistogram.Length; offset++)
            {
                gameState.mHeightHistogram[offset] = 0;
            }

            int heightHistogramLength = 0;
            file.FindTextAndInteger32( "heightHistogramLength", ref heightHistogramLength );

            if (heightHistogramLength >= gameState.mHeightHistogram.Length)
            {
                indexString = file.FindString( "heightHistogram" );
                if (indexString >= 0)
                {
                    for (offset = 0; offset < gameState.mHeightHistogram.Length; offset++)
                    {
                        longValue = file.GetStringAsInteger64ByIndex( (indexString + 1) + offset );

                        gameState.mHeightHistogram[offset] = longValue;
                    }
                }
            }




            // OTHER STATISTICS

            double totalElapsedTimeSeconds = 0.0;
            file.FindTextAndDouble( "totalElapsedTimeSeconds", ref totalElapsedTimeSeconds );
            gameState.mTotalElapsedTimeSeconds = totalElapsedTimeSeconds;

            long score = 0;
            file.FindTextAndInteger64( "score", ref score );
            gameState.mScore = score;

            long historicHighScore = 0;
            file.FindTextAndInteger64( "historicHighScore", ref historicHighScore );
            gameState.mHistoricHighScore = historicHighScore;

            long historicHighRows = 0;
            file.FindTextAndInteger64( "historicHighRows", ref historicHighRows );
            gameState.mHistoricHighRows = historicHighRows;

            long historicHighPieces = 0;
            file.FindTextAndInteger64( "historicHighPieces", ref historicHighPieces );
            gameState.mHistoricHighPieces = historicHighPieces;

            long historicCumulativeRows = 0;
            file.FindTextAndInteger64( "historicCumulativeRows", ref historicCumulativeRows );
            gameState.mHistoricCumulativeRows = historicCumulativeRows;

            long historicTotalGames = 0;
            file.FindTextAndInteger64( "historicTotalGames", ref historicTotalGames );
            gameState.mHistoricTotalGames = historicTotalGames;



            // HISTORIC ROWS LIST

            for (offset = 0; offset < gameState.mHistoricRows.Length; offset++)
            {
                gameState.mHistoricRows[offset] = 0;
            }

            int historicRowsLength = 0;
            file.FindTextAndInteger32( "historicRowsLength", ref historicRowsLength );

            if (historicRowsLength >= gameState.mHistoricRows.Length)
            {
                indexString = file.FindString( "historicRows" );
                if (indexString >= 0)
                {
                    for (offset = 0; offset < gameState.mHistoricRows.Length; offset++)
                    {
                        longValue = file.GetStringAsInteger64ByIndex( (indexString + 1) + offset );

                        gameState.mHistoricRows[offset] = longValue;
                    }
                }
            }






            // NEXT PIECE

            gameState.mSTPieceNext.Clear( );

            indexString = file.FindString( "pieceNext" );

            if (indexString >= 0)
            {
                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "shape", ref shape );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "x", ref x );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "y", ref y );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "orientation", ref orientation );

                gameState.mSTPieceNext.SetShape( (STPiece.STPieceShape)shape );
                gameState.mSTPieceNext.SetX( x );
                gameState.mSTPieceNext.SetY( y );
                gameState.mSTPieceNext.SetOrientation( orientation );
            }






            // BEST-MOVE PIECE

            gameState.mSTPieceBestMove.Clear( );

            indexString = file.FindString( "pieceBestMove" );

            if (indexString >= 0)
            {
                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "shape", ref shape );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "x", ref x );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "y", ref y );

                file.FindTextAndInteger32AtOrAfterIndex
                    ( indexString, "orientation", ref orientation );

                gameState.mSTPieceBestMove.SetShape( (STPiece.STPieceShape)shape );
                gameState.mSTPieceBestMove.SetX( x );
                gameState.mSTPieceBestMove.SetY( y );
                gameState.mSTPieceBestMove.SetOrientation( orientation );
            }





            // move animation state

            bool animateAIMovesEnable = false;
            file.FindTextAndInteger32( "animateAIMovesEnable", ref intValue );
            if (0 != intValue) { animateAIMovesEnable = true; }
            gameState.mAnimateAIMovesEnable = animateAIMovesEnable;

            int animateAIMovesStartingY = 0;
            file.FindTextAndInteger32( "animateAIMovesStartingY", ref animateAIMovesStartingY );
            gameState.mAnimateAIMovesStartingY = animateAIMovesStartingY;

            int animateAIMovesFinalSafeY = 0;
            file.FindTextAndInteger32( "animateAIMovesFinalSafeY", ref animateAIMovesFinalSafeY );
            gameState.mAnimateAIMovesFinalSafeY = animateAIMovesFinalSafeY;

            int animateAITotalInitialCommands = 0;
            file.FindTextAndInteger32( "animateAITotalInitialCommands", ref animateAITotalInitialCommands );
            gameState.mAnimateAITotalInitialCommands = animateAITotalInitialCommands;

            int animateAICommandsExecuted = 0;
            file.FindTextAndInteger32( "animateAICommandsExecuted", ref animateAICommandsExecuted );
            gameState.mAnimateAICommandsExecuted = animateAICommandsExecuted;

            double animateAICommandsPerRow = 0.0;
            file.FindTextAndDouble( "animateAICommandsPerRow", ref animateAICommandsPerRow );
            gameState.mAnimateAICommandsPerRow = animateAICommandsPerRow;

            int animateAIMovesPendingRotation = 0;
            file.FindTextAndInteger32( "animateAIMovesPendingRotation", ref animateAIMovesPendingRotation );
            gameState.mAnimateAIMovesPendingRotation = animateAIMovesPendingRotation;

            int animateAIMovesPendingTranslation = 0;
            file.FindTextAndInteger32( "animateAIMovesPendingTranslation", ref animateAIMovesPendingTranslation );
            gameState.mAnimateAIMovesPendingTranslation = animateAIMovesPendingTranslation;


            return (true);

        }





    }
}
