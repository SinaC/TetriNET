// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.


namespace CPF.StandardTetris
{
    public class STGameState
    {
        // This class contains the complete state information for a game.
        // All fields are public.  It is the responsibility of owners of
        // instances of this object to maintain the integrity of such
        // instances.

        // Core Game State

        // Board and Piece
        public STBoard   mSTBoardCurrent;
        public STPiece   mSTPieceCurrent;

        // Core Game State Variables
        public bool      mGameOver;
        public double    mIterationCountdownSeconds;
        public long      mCurrentPiecePointValue; // starts at 24+(3*(level-1))
        public long      mCompletedRows;

        // Piece sequence generator
        public STPieceSequence mSTPieceSequence;



        // User Options

        public bool      mPaused;
        public bool      mShowNextPiece;
        public bool      mAI;
        public bool      mSpawnFromVideoCapture;
        public bool      mOutputToRS232;
        public bool      mAutoRestart;
        public bool      mAutoWriteFile;

        // User Options
        // -2, -3, -4,... : Slow Mode (delay proportional to index)
        // -1 : Normal, Clipped at 0.20 sec/row
        //  0 : Normal
        // +1 : Fast Mode (still render bound)
        // +2 : Very Fast Mode (still render bound)
        // +3, +4, +5,... : Multiple moves per rendered frame
        public int       mGameSpeedAdjustment;
        public bool      mShadowMode;
        public bool      mHintMode;
        public bool      mMonochromeColorMode;



        // Statistics for User Consideration Only

        // Updated at piece spawning or row completion
        public long[]    mPieceHistogram; // Count of each piece type
        public long[]    mHeightHistogram; // Height after each landing
        public double    mTotalElapsedTimeSeconds;
        public long      mScore;
        // Only updated when game ends
        public long      mHistoricHighScore;
        public long      mHistoricHighRows;
        public long      mHistoricHighPieces;
        public long      mHistoricCumulativeRows;  // Used to get average: rows / games
        public long      mHistoricTotalGames;
        public long[]    mHistoricRows; // Past games [0]==most recent



        // Cached Derived or Copied Values

        // Next Piece (Not always used or available; Only 'kind' aspect is relevant)
        public STPiece   mSTPieceNext;

        // Best Move (determined by relevant AI upon piece spawning)
        public STPiece   mSTPieceBestMove;



        // state of animation of a an AI-executed move

        public bool      mAnimateAIMovesEnable;
        public int       mAnimateAIMovesStartingY;
        public int       mAnimateAIMovesFinalSafeY;
        public int       mAnimateAITotalInitialCommands;
        public int       mAnimateAICommandsExecuted;
        public double    mAnimateAICommandsPerRow;
        public int       mAnimateAIMovesPendingRotation;
        public int       mAnimateAIMovesPendingTranslation;





        // RUN-TIME STATE INFORMATION (DO NOT STORE IN A FILE)

        // video capture related
        public int mPreviousClassification;

        public int mSelectionState;
        public int mSelectionX1;
        public int mSelectionY1;
        public int mSelectionX2;
        public int mSelectionY2;

        public bool mCalibrationModeFlag; // false == OFF
        public int mCalibrationModeShapeCode;  // 1..7 shape code

        // file list
        public int mFirstItem;
        public int mRelativeItem;
        public bool mShowFileList;
        public bool mLoadFlag;

        // miscellaneous
        public int mRenderFrameNumber;
        public int mShowInstructionPage;
        public bool mShowConsole;
        public float mReportedFrameRate;



        public STGameState ( )
        {
            // Core Game State

            // Board and Piece
            mSTBoardCurrent = new STBoard();
            mSTPieceCurrent = new STPiece();

            // Core Game State Variables
            mGameOver = false;
            mIterationCountdownSeconds = 0.0;
            mCurrentPiecePointValue = 0; // starts at 24+(3*(level-1))
            mCompletedRows = 0;

            // Piece Sequence Generator
            mSTPieceSequence = new STPieceSequence();

            // User Options

            mPaused = false;
            mShowNextPiece = false;
            mAI = false;
            mSpawnFromVideoCapture = false;
            mOutputToRS232 = false;
            mAutoRestart = false;
            mAutoWriteFile = false;

            // Game Speed Adjustment
            // -2, -3, -4,... : Slow Mode (delay proportional to index)
            // -1 : Normal, Clipped at 0.20 sec/row
            //  0 : Normal
            // +1 : Fast Mode (still render bound)
            // +2 : Very Fast Mode (still render bound)
            // +3, +4, +5,... : Multiple moves per rendered frame
            mGameSpeedAdjustment = 0;

            mShadowMode = false;
            mHintMode = false;
            mMonochromeColorMode = false;



            // Statistics for User Consideration Only

            // Updated at piece spawning or row completion
            mPieceHistogram = new long[8]; // Count of each piece type
            mHeightHistogram = new long[202]; // Height after each landing
            mTotalElapsedTimeSeconds = 0.0;
            mScore = 0;

            // Only updated when game ends
            mHistoricHighScore = 0;
            mHistoricHighRows = 0;
            mHistoricHighPieces = 0;
            mHistoricCumulativeRows = 0;  // Used to get average: rows / games
            mHistoricTotalGames = 0;
            mHistoricRows = new long[20]; // Past games [0]==most recent



            // Cached Derived or Copied Values

            // Next Piece (Not always used or available; Only 'kind' aspect is relevant)
            mSTPieceNext = new STPiece();

            // Best Move (determined by relevant AI upon piece spawning)
            mSTPieceBestMove = new STPiece();


            // state of animation of a an AI-executed move
            mAnimateAIMovesEnable = true;
            mAnimateAIMovesStartingY = 0;
            mAnimateAIMovesFinalSafeY = 0;
            mAnimateAITotalInitialCommands = 0;
            mAnimateAICommandsExecuted = 0;
            mAnimateAICommandsPerRow = 0;
            mAnimateAIMovesPendingRotation = 0;
            mAnimateAIMovesPendingTranslation = 0;



            // RUN-TIME STATE INFORMATION (DO NOT STORE IN A FILE)

            // video capture related
            mPreviousClassification = (-1);

            mSelectionState = 0;
            mSelectionX1 = 0;
            mSelectionY1 = 0;
            mSelectionX2 = 0;
            mSelectionY2 = 0;

            mCalibrationModeFlag = false; // false == OFF
            mCalibrationModeShapeCode = 0;  // 1..7 shape

            // file list
            mFirstItem = 0;
            mRelativeItem = 0;
            mShowFileList = false;
            mLoadFlag = false;

            // miscellaneous
            mRenderFrameNumber = 0;
            mShowInstructionPage = 0;
            mShowConsole = false;
            mReportedFrameRate = 0.0f;
        }
    }
}
