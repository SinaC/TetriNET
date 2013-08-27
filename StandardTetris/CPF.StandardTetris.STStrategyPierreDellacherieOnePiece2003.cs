// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.
// This file contains an adapted form of an algorithm developed by the following
// person: Pierre Dellacherie ; France, 2003.



//#define DEBUGGING_PRINT_STATEMENTS



using System;
using System.Collections.Generic;
using System.Text;




namespace CPF.StandardTetris
{
    public class STStrategyPierreDellacherieOnePiece2003 : STStrategy
    {


        public override String GetStrategyName ( )
        {
            return ("Pierre Dellacherie (one-piece, 2003)");
        }


        public override void GetBestMoveOncePerPiece
        (
            STBoard board,
            STPiece piece,
            bool nextPieceFlag, // false == no next piece available or known
            STPiece.STPieceShape nextPieceShape, // None == no piece available or known
            ref int bestRotationDelta, // 0 or {0,1,2,3}
            ref int bestTranslationDelta // 0 or {...,-2,-1,0,1,2,...}
        )
        {
            bestRotationDelta = 0;
            bestTranslationDelta = 0;

            PrivateStrategy
            (
                false,
                board,
                piece,
                ref bestRotationDelta, // 0 or {0,1,2,3}
                ref bestTranslationDelta // 0 or {...,-2,-1,0,1,2,...}
            );
        }



        private double PrivateStrategy
        ( 
            bool flagCalledFromParentPly,  // True if called from a parent level
            STBoard board,
            STPiece piece,
            ref int bestRotationDelta,    // 0 or {0,1,2,3} 
            ref int bestTranslationDelta  // 0 or {...,-2,-1,0,1,2,...}
        )
        {
            if (false == piece.IsValid())
            {
                return(0.0);
            }



            int currentBestTranslationDelta  = 0;
            int currentBestRotationDelta = 0;
            double currentBestMerit = (-1.0e+20); // Really bad!
            int currentBestPriority = 0;

            int trialTranslationDelta = 0;
            int trialRotationDelta = 0;
            double trialMerit = 0.0;
            int trialPriority = 0;

            bool moveAcceptable = false;
            int count = 0;

            STBoard tempBoard = new STBoard();
            STPiece tempPiece = new STPiece();



            int maxOrientations = 0;
            maxOrientations =
                STPiece.GetMaximumOrientationsOfShape( piece.GetShape() );


#if DEBUGGING_PRINT_STATEMENTS
            STEngine.GetConsole().AddLine( " " );
            STEngine.GetConsole().AddLine( "STStrategyPierreDellacherieOnePiece2003" );
#endif


            for 
                ( 
                    trialRotationDelta = 0; 
                    trialRotationDelta < maxOrientations;
                    trialRotationDelta++ 
                )
            {
                // Make temporary copy of piece, and rotate the copy.
                tempPiece.CopyFrom( piece );
                for ( count = 0; count < trialRotationDelta; count++ )
                {
                    tempPiece.Rotate();
                }


                // Determine the translation limits for this rotated piece.
                bool moveIsPossible = false;
                int minDeltaX      = 0;
                int maxDeltaX      = 0;
                board.DetermineAccessibleTranslationsForPieceOrientation
                (
                    tempPiece,
                    ref moveIsPossible,
                    ref minDeltaX,    // left limit
                    ref maxDeltaX     // right limit
                );


                // Consider all allowed translations for the current rotation.
                if (true == moveIsPossible)
                {
                    for 
                        ( 
                            trialTranslationDelta = minDeltaX;
                            trialTranslationDelta <= maxDeltaX;
                            trialTranslationDelta++ 
                        )
                    {
                        // Evaluate this move

                        // Copy piece to temp and rotate and translate
                        tempPiece.CopyFrom( piece );
                        for ( count = 0; count < trialRotationDelta; count++ )
                        {
                            tempPiece.Rotate();
                        }
                        tempPiece.Translate( trialTranslationDelta, 0 );

                        moveAcceptable =
                            board.DetermineIfPieceIsWithinBoardAndDoesNotOverlapOccupiedCells
                            (
                                tempPiece
                            );

                        if (true == moveAcceptable)
                        {
                            // Because the piece can BE (not necessarily GET) at the goal
                            // horizontal translation and orientation, it's worth trying
                            // out a drop and evaluating the move.
                            tempBoard.CopyFrom( board );


                            tempBoard.FullDropAndCommitPieceToBoard
                            ( 
                                tempPiece
                            );


                            // Pierre Dellacherie (France) Board & Piece Evaluation Function
                            this.PrivateStrategyEvaluate
                            ( 
                                tempBoard,
                                tempPiece,
                                ref trialMerit,
                                ref trialPriority
                            );


#if DEBUGGING_PRINT_STATEMENTS
							STEngine.GetConsole().AddToLastLine
								(
                                " M: " + trialMerit
                                + " R: " + trialRotationDelta
                                + " dX: " + trialTranslationDelta
                                + " P: " + trialPriority
								);
#endif


                            // If this move is better than any move considered before,
                            // or if this move is equally ranked but has a higher priority,
                            // then update this to be our best move.
                            if 
                                ( 
                                (trialMerit > currentBestMerit)
                                || ((trialMerit == currentBestMerit) && (trialPriority > currentBestPriority)) 
                                )
                            {
                                currentBestMerit            = trialMerit;
                                currentBestPriority         = trialPriority;
                                currentBestTranslationDelta = trialTranslationDelta;
                                currentBestRotationDelta    = trialRotationDelta;
                            }
                        }
                    }
                }
            }


            // commit to this move
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta    = currentBestRotationDelta;

            return( currentBestMerit );
        }




        // The following evaluation function was adapted from Pascal code submitted by:
        // Pierre Dellacherie (France).  (E-mail : dellache@club-internet.fr)
        //
        // This amazing one-piece algorithm completes an average of roughly 600 000 
        // rows, and often attains 2 000 000 or 2 500 000 rows.  However, the algorithm
        // sometimes completes as few as 15 000 rows.  I am fairly certain that this
        // is NOT due to statistically abnormal patterns in the falling piece sequence.
        //
        // Pierre Dellacherie corresponded with me via e-mail to help me with the 
        // conversion of his Pascal code to C++.
        //
        // WARNING:
        //     If there is a single board and piece combination with the highest
        //     'rating' value, it is the best combination.  However, among
        //     board and piece combinations with EQUAL 'rating' values,
        //     the highest 'priority' value wins.
        //
        //     So, the complete rating is: { rating, priority }.


        void  PrivateStrategyEvaluate
        (
            STBoard board,
            STPiece piece,
            ref double rating,
            ref int priority
        )
        {
            rating = 0.0;
            priority = 0;

            if (false == piece.IsValid()) 
            {
                return;
            }



            int boardWidth = 0;
            int boardHeight = 0;
            boardWidth = board.GetWidth();
            boardHeight = board.GetHeight();





            int pieceMinX = 0;
            int pieceMinY = 0;
            int pieceMaxX = 0;
            int pieceMaxY = 0;
            piece.GetTranslatedBoundingRectangle
                ( ref pieceMinX, ref pieceMinY, ref pieceMaxX, ref pieceMaxY );


            // Landing Height (vertical midpoint)

            double landingHeight = 0.0;
            landingHeight = 0.5 * (double)( pieceMinY + pieceMaxY );





            int completedRows = 0;
            completedRows = board.GetTotalCompletedRows();

            int erodedPieceCellsMetric = 0;
            if (completedRows > 0)
            {
                // Count piece cells eroded by completed rows before doing collapse on pile.
                int pieceCellsEliminated = 0;
                pieceCellsEliminated = board.CountPieceCellsEliminated( piece );

                // Now it's okay to collapse completed rows
                board.CollapseAnyCompletedRows();

                // Weight eroded cells by completed rows
                erodedPieceCellsMetric = (completedRows * pieceCellsEliminated);
            }




            // Note that this evaluation of pile height is AFTER collapsing
            // any completed rows.
            int pileHeight = 0;
            pileHeight = board.GetPileMaxHeight();

            // Each empty row (above pile height) has two (2) "transitions"
            // (We could call ref_Board.GetTransitionCountForRow( y ) for
            // these unoccupied rows, but this is an optimization.)
            int boardRowTransitions = 0;
            boardRowTransitions = 2 * (boardHeight - pileHeight);

            // Only go up to the pile height, and later we'll account for the
            // remaining rows transitions (2 per empty row).
            int y = 0;
            for ( y = 1; y <= pileHeight; y++ )
            {
                boardRowTransitions += (board.GetTransitionCountForRow( y ));
            }




            int boardColumnTransitions = 0;
            int boardBuriedHoles = 0;
            int boardWells = 0;
            int x = 0;
            for ( x = 1; x <= boardWidth; x++ )
            {
                boardColumnTransitions += board.GetTransitionCountForColumn( x );
                boardBuriedHoles += board.GetBuriedHolesForColumn( x );
                boardWells += board.GetAllWellsForColumn( x );
            }





            // Final Rating


            rating = (0.0);
            rating += ((-1.0) * (landingHeight));
            rating += ((1.0) * ((double)(erodedPieceCellsMetric)));
            rating += ((-1.0) * ((double)(boardRowTransitions)));
            rating += ((-1.0) * ((double)(boardColumnTransitions)));
            rating += ((-4.0) * ((double)(boardBuriedHoles)));
            rating += ((-1.0) * ((double)(boardWells)));

            // EXPLANATION:
            //   [1] Punish landing height
            //   [2] Reward eroded piece cells
            //   [3] Punish row    transitions
            //   [4] Punish column transitions
            //   [5] Punish buried holes (cellars)
            //   [6] Punish wells






#if DEBUGGING_PRINT_STATEMENTS
				STEngine.GetConsole().AddLine
				( 
				" D:" + (21.0-landingHeight)
                + " R:" + erodedPieceCellsMetric 
                + " RC:" + (-boardRowTransitions)
                + " CC:" + (-boardColumnTransitions) 
                +" H:" + (-4*boardBuriedHoles)
                +" W:" + (-boardWells)
				);
#endif




            // PRIORITY:  
            //   Priority is further differentiation between possible moves.
            //   We further rate moves accoding to the following:
            //            * Reward deviation from center of board
            //            * Reward pieces to the left of center of the board
            //            * Punish rotation
            //   Priority is less important than the rating, but among equal
            //   ratings we select the option with the greatest priority.
            //   In principle we could simply factor priority in to the rating,
            //   as long as the priority was less significant than the smallest
            //   variations in rating, but for large board widths (>100), the
            //   risk of loss of precision in the lowest bits of the rating
            //   is too much to tolerate.  So, this priority is stored in a
            //   separate variable.

            int absoluteDistanceX = 0;
            absoluteDistanceX = (piece.GetX() - board.GetPieceSpawnX());
            if (absoluteDistanceX < 0)
            {
                absoluteDistanceX = (-(absoluteDistanceX));
            }

            priority = 0;
            priority += (100 * absoluteDistanceX);
            if (piece.GetX() < board.GetPieceSpawnX()) 
            {
                priority += 10;
            }
            priority -= (piece.GetOrientation( ) - 1);

        }











    }
}
