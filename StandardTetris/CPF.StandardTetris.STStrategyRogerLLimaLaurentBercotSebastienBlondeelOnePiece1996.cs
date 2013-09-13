// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.
// This file contains an adapted form of an algorithm developed by the following
// people: Roger LLima, Laurent Bercot, Sebastien Blondeel ; 1996.


using System;


namespace CPF.StandardTetris
{
    class STStrategyRogerLLimaLaurentBercotSebastienBlondeelOnePiece1996 : STStrategy
    {




        public override String GetStrategyName ( )
        {
            return ("Roger LLima, Laurent Bercot, Sebastien Blondeel (one-piece, 1996)");
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






        // The following one-ply board evaluation function is adapted from the 
        // "xtris" application (multi-player Tetris for the X Window system),
        // created by Roger Espel Llima <roger.espel.llima@pobox.com>
        //
        // From the "xtris" documentation:
        //
        //   "The values for the coefficients were obtained with a genetic algorithm
        //   using a population of 50 sets of coefficients, calculating 18 generations
        //   in about 500 machine-hours distributed among 20-odd Sparc workstations.
        //   This resulted in an average of about 50,000 completed lines."
        //
        // The following people contributed "ideas for the bot's decision algorithm":
        //
        // Laurent Bercot      <Laurent.Bercot@ens.fr>
        // Sebastien Blondeel  <Sebastien.Blondeel@ens.fr>
        //		
        //
        // The algorithm computes 6 values on the whole pile:
        //
        // [1] height   = max height of the pieces in the pile
        // [2] holes    = number of holes (empty positions with a full position somewhere
        //                above them)
        // [3] frontier = length of the frontier between all full and empty zones
        //		            (for each empty position, add 1 for each side of the position
        //		            that touches a border or a full position).
        // [4] drop     = how far down we're dropping the current brick
        // [5] pit      = sum of the depths of all places where a long piece ( ====== )
        //                would be needed.
        // [6] ehole    = a kind of weighted sum of holes that attempts to calculate
        //                how hard they are to fill.
        //
        // droppedPieceRow is the row where we're dropping the piece,
        // which is already in the board.  Note that full lines have not been
        // dropped, so we need to do special tests to skip them.


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

            int width = 0;
            int height = 0;
            width = board.GetWidth( );
            height = board.GetHeight( );


            int pieceDropDistance = 0;
            pieceDropDistance = (height - piece.GetY());



            int coeff_f = 260;
            int coeff_height = 110;
            int coeff_hole = 450;
            int coeff_y = 290;
            int coeff_pit = 190;
            int coeff_ehole = 80;


			int[] lineCellTotal = new int [ (1 + 20 + 200) ];
			int[] lin = new int [ (1 + 20 + 200) ];
			int[] hol = new int  [ (1 + 10 + 200) * (1 + 20 + 400) ];
            int[] blockedS = new int[(1 + 10 + 200)];

			// If width is greater than 200, or height is greater than 400,
			// just give up. Really, this algorithm needs to be repaired to 
            // avoid the use of memory!
			if (width  > 200) return;
			if (height > 400) return;



			int x = 0;
			int y = 0;


			// NOTE: ALL ARRAYS ARE ACCESSED WITH 0 BEING FIRST ELEMENT

			// Fill lineCellTotal[] with total cells in each row.
			
			for ( y = 1; y <= height; y++ )
			{
				lineCellTotal[ (y-1) ] = 0;
				for ( x = 1; x <= width; x++ )
				{
                    if (board.GetCell( x, y ) > 0)
						lineCellTotal[ (y-1) ]++;
				}
			}
			


			
			// Clobber blocked column array
			
			for ( x = 1; x <= width; x++ )
			{
				blockedS[ (x-1) ] = (-1);
			}
			


			
			// Clear Lin array.
			
			for ( y = 1; y <= height; y++ )
			{
				lin[ (y-1) ] = 0;
			}
			


			
			// Embedded Holes
			
			int eHoles     =  0;
			for ( y = height; y >= 1; y-- ) // Top-to-Bottom
			{
				for ( x = 1; x <= width; x++ )
				{

                    if (board.GetCell( x, y ) > 0)
					{
						hol     [ (width * (y-1)) + (x-1) ] = 0;
						blockedS[ (x-1)                           ] = y;
					}
					else
					{
						hol     [ (width * (y-1)) + (x-1) ] = 1;
						if (blockedS[ (x-1) ] >= 0)
						{
							int y2 = 0;

							y2 = blockedS[ (x-1) ];

							// If this more than two rows ABOVE current row, set
							// to exactly two rows above.
							if (y2 > (y + 2)) 
							{
								y2 = (y + 2);
							}

							// Descend to current row

							for ( ; y2 > y; y2-- )
							{
                                if (board.GetCell( x, y2 ) > 0)
								{
									hol[ (width * (y-1)) + (x-1) ] += lin[ (y2-1) ];
								}
							}           
						}
						lin[ (y-1) ] += hol[ (width * (y-1)) + (x-1) ];
						eHoles       += hol[ (width * (y-1)) + (x-1) ];
					}
				}
			}
			



			
			// Determine Max Height
			
			int maxHeight  =  0;
			for ( x = 1; x <= width; x++ )
			{
				for ( y = height; y >= 1; y-- ) // Top-to-Bottom
				{
					// If line is complete, ignore it for Max Height purposes...
					if (width == lineCellTotal[ (y-1) ])
						continue;

					if ( (y > maxHeight) &&
                        (board.GetCell( x, y ) > 0))
					{
						maxHeight = y;
					}
				}
			}
			



			
			// Count buried holes
			
			int holes      =  0;
			int blocked    =  0;
			for ( x = 1; x <= width; x++ )
			{
				blocked = 0;

				for ( y = height; y >= 1; y-- ) // Top-to-Bottom
				{
					// If line is complete, skip it!
					if (width == lineCellTotal[ (y-1) ])
						continue;

                    if (board.GetCell( x, y ) > 0)
					{
						blocked = 1;  // We encountered an occupied cell; all below is blocked
					}
					else
					{
						// All of the following is in the context of the cell ( x, y )
						// being UN-occupied.

						// If any upper row had an occupied cell in this column, this
						// unoccupied cell is considered blocked.
						if (0 != blocked)
						{
							holes++; // This unoccupied cell is buried; it's a hole.
						}
					}
				}
			}
			




			
			// Count Frontier
			
			int frontier      =  0;
			for ( x = 1; x <= width; x++ )
			{
				for ( y = height; y >= 1; y-- ) // Top-to-Bottom
				{
					// If line is complete, skip it!
					if (width == lineCellTotal[ (y-1) ])
						continue;

                    if (0 == board.GetCell( x, y ))
					{
						// All of the following is in the context of the cell ( x, y )
						// being UN-occupied.

						// If row is not the top, and row above this one is occupied,
						// then this unoccupied cell counts as a frontier.
						if ( (y < height) &&
                            (board.GetCell( x, (y + 1) ) > 0))
						{
							frontier++;
						}

						// If this row is not the bottom, and the row below is occupied,
						// this unoccupied cell counts as a frontier.
						if ( (y > 1) &&
                            (board.GetCell( x, (y - 1) ) > 0))
						{
							frontier++;
						}

						// If the column is not the first, and the column to the left is
						// occupied, then this unoccupied cell counts as a frontier.
						// Or, if this *is* the left-most cell, it is an automatic frontier.
						//  (since the beyond the board is in a sense "occupied")
						if (  ((x > 1) &&
                            (board.GetCell( x - 1, y ) > 0)) ||
							(1 == x) )
						{
							frontier++;
						}

						// If the column is not the right-most, and the column to the right is
						// occupied, then this unoccupied cell counts as a frontier.
						// Or, if this *is* the right-most cell, it is an automatic frontier.
						//  (since the beyond the board is in a sense "occupied")
						if ( ((x < width) &&
                            (board.GetCell( x + 1, y ) > 0)) ||
							(width == x) )
						{
							frontier++;
						}
					}
				}
			}
			



			
			int v   =  0;
			for ( x = 1; x <= width; x++ )
			{

				// NOTE: The following seems to descend as far as a 2-column-wide
				//       profile can fall for each column.
				// Scan Top-to-Bottom
				y = height;
				while 
					( 
					// Line is not below bottom row...
					(y >= 1) &&  
					// Cell is unoccupied or line is full...
                    ((0 == board.GetCell( x, y )) || 
					(width == lineCellTotal[ (y-1) ])   )   &&

					(  
					// (Not left column AND (left is empty OR line full))
					((x > 1) &&
                    ((0 == board.GetCell( x - 1, y )) || 
					(width == lineCellTotal[ (y-1) ])))          
					|| // ...OR...
					// (Not right column AND (right is empty OR line full))
					((x < width) &&
                    ((0 == board.GetCell( x + 1, y )) ||
					(width == lineCellTotal[ (y-1) ]))))
					)
				{
					y--; // Descend
				}

				// Count how much further we can fall just considering obstacles
				// in our column.
				int p = 0;
				p = 0;
				for (  ; 
					((y >= 1) && (0 == board.GetCell( x, y ))); 
					y--, p++ ) ;

				// If this is a deep well, it's worth punishing.
				if (p >= 2)
				{
					v -= (coeff_pit * ( p - 1 ));
				}
			}
			




            // compute rating by summing weighted factors
			rating  = (float)(v);
			rating -= (float)(coeff_f      * frontier          );
			rating -= (float)(coeff_height * maxHeight         );
			rating -= (float)(coeff_hole   * holes             );
			rating -= (float)(coeff_ehole  * eHoles            );
			rating += (float)(coeff_y      * pieceDropDistance );  // Reward drop depth!
        }






    }
}
