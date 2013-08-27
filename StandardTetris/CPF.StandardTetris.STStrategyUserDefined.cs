// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;



namespace CPF.StandardTetris
{
    public class STStrategyUserDefined : STStrategy
    {


        public override String GetStrategyName ( )
        {
            return ("User Defined");
        }


        // WARNING: Moves requiring rotation must wait until piece has fallen by
        // at least one row.

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

            // We are given the current board, and the current piece
            // configuration.  Our goal is to evaluate various possible
            // moves and return the best move we explored.

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
            if (false == piece.IsValid( ))
            {
                return (0.0);
            }



            int currentBestTranslationDelta = 0;
            int currentBestRotationDelta = 0;
            double currentBestMerit = (-1.0e+20); // Really bad!

            int trialTranslationDelta = 0;
            int trialRotationDelta = 0;
            double trialMerit = 0.0;

            bool moveAcceptable = false;
            int count = 0;


            STBoard tempBoard = new STBoard( );
            STPiece tempPiece = new STPiece( );



            int maxOrientations = 0;
            maxOrientations =
                STPiece.GetMaximumOrientationsOfShape( piece.GetShape( ) );



            for
                (
                    trialRotationDelta = 0;
                    trialRotationDelta < maxOrientations;
                    trialRotationDelta++
                )
            {
                // Make temporary copy of piece, and rotate the copy.
                tempPiece.CopyFrom( piece );
                for (count = 0; count < trialRotationDelta; count++)
                {
                    tempPiece.Rotate( );
                }


                // Determine the translation limits for this rotated piece.
                bool moveIsPossible = false;
                int minDeltaX = 0;
                int maxDeltaX = 0;
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
                        for (count = 0; count < trialRotationDelta; count++)
                        {
                            tempPiece.Rotate( );
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
                                ref trialMerit
                            );

                            // If this move is better than any move considered before,
                            // or if this move is equally ranked but has a higher priority,
                            // then update this to be our best move.
                            if (trialMerit > currentBestMerit)
                            {
                                currentBestMerit = trialMerit;
                                currentBestTranslationDelta = trialTranslationDelta;
                                currentBestRotationDelta = trialRotationDelta;
                            }
                        }
                    }
                }
            }


            // commit to this move
            bestTranslationDelta = currentBestTranslationDelta;
            bestRotationDelta = currentBestRotationDelta;

            return (currentBestMerit);
        }





        void PrivateStrategyEvaluate
        (
            STBoard board,
            STPiece piece,
            ref double rating
        )
        {
            rating = 0.0;

            if (false == piece.IsValid( ))
            {
                return;
            }


            // The board was given to us with the piece already committed
            // to the board cells, so we can now collapse any completed
            // (fully-occupied) rows.
            board.CollapseAnyCompletedRows( );


            // Note that this evaluation of pile height is AFTER collapsing
            // any completed rows.
            int pileHeight = 0;
            pileHeight = board.GetPileMaxHeight( );


            // This simplistic strategy only punishes the maximum
            // height of the pile.
            rating = ((-1.0) * (double)pileHeight);
        }







    }
}
