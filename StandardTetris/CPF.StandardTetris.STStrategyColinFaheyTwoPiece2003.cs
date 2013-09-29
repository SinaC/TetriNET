// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;


namespace CPF.StandardTetris
{
    public class STStrategyColinFaheyTwoPiece2003 : STStrategy
    {
        public override String GetStrategyName ( )
        {
            return ("Colin Fahey (two-piece, 2003)");
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


			if (true == nextPieceFlag)
			{
				// two-piece analysis (current piece and next piece)
                PrivateStrategyNextPiece
                (
                    board,
                    piece,
                    nextPieceShape,
                    ref bestRotationDelta, // 0 or {0,1,2,3}
                    ref bestTranslationDelta // 0 or {...,-2,-1,0,1,2,...}
                );
			}
			else
			{
				// one-piece analysis
                PrivateStrategy
                (
                    false,
                    board,
                    piece,
                    ref bestRotationDelta, // 0 or {0,1,2,3}
                    ref bestTranslationDelta // 0 or {...,-2,-1,0,1,2,...}
                );
			}
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



			int    currentBestTranslationDelta = 0;
			int    currentBestRotationDelta = 0;
			double currentBestMerit = (-1.0e20);
			int    currentBestPriority = 0;

			int    trialTranslationDelta = 0;
			int    trialRotationDelta = 0;
			double trialMerit = 0.0;
			int    trialPriority = 0;

			int    maxOrientations = 0;
			bool   moveAcceptable = false;
			int    count = 0;



			STBoard  tempBoard = new STBoard();
			STPiece  tempPiece = new STPiece();



			maxOrientations =
				STPiece.GetMaximumOrientationsOfShape( piece.GetShape() );



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
				int minDeltaX = 0;
				int maxDeltaX = 0;
				board.DetermineAccessibleTranslationsForPieceOrientation
					(
					tempPiece,
					ref moveIsPossible, // false==NONE POSSIBLE
					ref minDeltaX,    // Left limit
					ref maxDeltaX     // Right limit
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
							// Since the piece can be (not necessarily GET) at the goal
							// horizontal translation and orientation, it's worth trying
							// out a drop and evaluating the move.
							tempBoard.CopyFrom( board );


							tempBoard.FullDropAndCommitPieceToBoard( tempPiece );


							trialPriority = 0;


							if (true == flagCalledFromParentPly)
							{
                                // UNUSED: int rowsEliminated = 0;
                                // UNUSED: rowsEliminated =
 
                                    tempBoard.CollapseAnyCompletedRows();


								double weightTotalShadowedHoles      = (-0.65);
                                double weightPileHeightWeightedCells = (-0.10);
                                double weightSumOfWellHeights        = (-0.20);

                                trialMerit = (weightTotalShadowedHoles) * (double)(tempBoard.GetTotalShadowedHoles( ));
                                trialMerit += (weightPileHeightWeightedCells) * (double)(tempBoard.GetPileHeightWeightedCells( ));
                                trialMerit += (weightSumOfWellHeights) * (double)(tempBoard.GetSumOfWellHeights( ));          
							}
							else
							{
								double weightRowElimination          = ( 0.30);
                                double weightTotalOccupiedCells      = (-0.00);
                                double weightTotalShadowedHoles      = (-0.65);
                                double weightPileHeightWeightedCells = (-0.10);
                                double weightSumOfWellHeights        = (-0.20);

								int rowsEliminated = 0;
								rowsEliminated = tempBoard.CollapseAnyCompletedRows();
								// Single Ply (No next piece)
								// Averages around 1310 rows in 10 games, with a min of 445 and a max of 3710.
                                trialMerit = (weightRowElimination) * (double)(rowsEliminated);
                                trialMerit += (weightTotalOccupiedCells) * (double)(tempBoard.GetTotalOccupiedCells( ));
                                trialMerit += (weightTotalShadowedHoles) * (double)(tempBoard.GetTotalShadowedHoles( ));
                                trialMerit += (weightPileHeightWeightedCells) * (double)(tempBoard.GetPileHeightWeightedCells( ));
                                trialMerit += (weightSumOfWellHeights) * (double)(tempBoard.GetSumOfWellHeights( ));
							}



							// If this move is better than any move considered before,
							// or if this move is equally ranked but has a higher priority,
							// then update this to be our best move.
							if 
                                ( 
                                (trialMerit > currentBestMerit) ||
								((trialMerit == currentBestMerit) && (trialPriority > currentBestPriority)) 
                                )
							{
								currentBestPriority = trialPriority;
								currentBestMerit = trialMerit;
								currentBestTranslationDelta = trialTranslationDelta;
								currentBestRotationDelta = trialRotationDelta;
							}
						}
					}
				}
				
			}


			// Commit to this move
			bestTranslationDelta = currentBestTranslationDelta;
			bestRotationDelta    = currentBestRotationDelta;


			return( currentBestMerit );
        }

        private double PrivateStrategyNextPiece
        (
            STBoard board,
            STPiece piece,
            STPiece.STPieceShape nextPieceShape, // None == no piece available or known
            ref int bestRotationDelta,    // 0 or {0,1,2,3} 
            ref int bestTranslationDelta  // 0 or {...,-2,-1,0,1,2,...}
        )
        {
            if (false == piece.IsValid( ))
            {
                return (0.0);
            }



			int    currentBestTranslationDelta = 0;
			int    currentBestRotationDelta = 0;
			double currentBestMerit = (-1.0e20);
			int    currentBestPriority = 0;

			int    trialTranslationDelta = 0;
			int    trialRotationDelta = 0;
			double trialMerit = 0.0;
			int    trialPriority = 0;

			int    maxOrientations = 0;
			bool   moveAcceptable = false;
			int    count = 0;



			STBoard  tempBoard = new STBoard();
			STPiece  tempPiece = new STPiece();



			maxOrientations = STPiece.GetMaximumOrientationsOfShape( piece.GetShape() );



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
				int minDeltaX = 0;
				int maxDeltaX = 0;
				board.DetermineAccessibleTranslationsForPieceOrientation
					(
					tempPiece,
					ref moveIsPossible, // false==NONE POSSIBLE
					ref minDeltaX,    // Left limit
					ref maxDeltaX     // Right limit
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
							// Since the piece can be (not necessarily GET) at the goal
							// horizontal translation and orientation, it's worth trying
							// out a drop and evaluating the move.
							tempBoard.CopyFrom( board );


							tempBoard.FullDropAndCommitPieceToBoard( tempPiece );


							trialPriority = 0;



							// Okay, now do second move with "Next Piece"
							int   nextPieceBestRotation    = 0;  // Dummy variable
							int   nextPieceBestTranslation = 0;  // Dummy variable

							STPiece  nextPiece = new STPiece();
							nextPiece.SetShape( nextPieceShape );
							nextPiece.SetX( tempBoard.GetPieceSpawnX()     );
							nextPiece.SetY( tempBoard.GetPieceSpawnY() - 1 );
							nextPiece.SetOrientation( 1 );

							trialMerit = 
								PrivateStrategy
								( 
								true, // Not just a single ply; We are calling from a parent ply.
								tempBoard,
								nextPiece,
								ref nextPieceBestRotation, 
								ref nextPieceBestTranslation
								);



							// If this move is better than any move considered before,
							// or if this move is equally ranked but has a higher priority,
							// then update this to be our best move.
							if ( 
                                (trialMerit > currentBestMerit) ||
								((trialMerit == currentBestMerit) && (trialPriority > currentBestPriority)) 
                                )
							{
								currentBestPriority = trialPriority;
								currentBestMerit = trialMerit;
								currentBestTranslationDelta = trialTranslationDelta;
								currentBestRotationDelta = trialRotationDelta;
							}
						}
					}
				}
				
			}


			// Commit to this move
			bestTranslationDelta = currentBestTranslationDelta;
			bestRotationDelta = currentBestRotationDelta;


			return( currentBestMerit );
        }
    }
}
