using System;
using System.Collections.Generic;

namespace CPF.StandardTetris
{
    public class STStrategyManager
    {
        private static List<STStrategy> mListSTStrategy = new List<STStrategy>();

        private static String mCurrentStrategyName = "";




        public static void AddStrategy( STStrategy strategy )
        {
            if (false == mListSTStrategy.Contains( strategy ))
            {
                mListSTStrategy.Add( strategy );
            }
        }

        

        static STStrategyManager ( )
        {
            STStrategyManager.AddStrategy( new STStrategyPierreDellacherieOnePiece2003( ) );
            STStrategyManager.AddStrategy( new STStrategyRogerLLimaLaurentBercotSebastienBlondeelOnePiece1996( ) );
            STStrategyManager.AddStrategy( new STStrategyUserDefined( ) );
            STStrategyManager.AddStrategy( new STStrategyColinFaheyTwoPiece2003( ) );
        }



        // The following methods to set and get the current strategy
        // by name intentionally avoid validating the name with the
        // list of strategies.  Only when the strategy is used do we
        // attempt to fix a specified choice.  This is to make sure
        // that loading and saving is not affected by whether or not
        // required strategies are inserted in to the list.

        public static void SetCurrentStrategyByName ( String strategyName )
        {
            mCurrentStrategyName = strategyName;
        }

        public static String GetCurrentStrategyName ( )
        {
            // Force mCurrentStrategyName to have a current value
            GetCurrentStrategy( );

            return(mCurrentStrategyName);
        }





        public static STStrategy GetCurrentStrategy( )
        { 
            if (mListSTStrategy.Count <= 0)
            {
                return(null);
            }

            foreach( STStrategy strategy in mListSTStrategy )
            {
                if (0 == String.Compare(strategy.GetStrategyName(), mCurrentStrategyName, true ))
                {
                    return( strategy );
                }
            }

            // The name did not match any strategy in the list, so we
            // arbitrarily choose the first strategy in the list.

            mCurrentStrategyName = mListSTStrategy[0].GetStrategyName();

            return( mListSTStrategy[0] );
        }




        public static void SelectNextStrategy ( )
        {
            if (mListSTStrategy.Count <= 1)
            {
                return;
            }

            // There are at least two strategies; thus, advancing
            // to a next strategy (with wrap-around) is possible.
            int k = -1;
            int i = 0;
            int n = 0;
            n = mListSTStrategy.Count;
            for (i = 0; i < n; i++)
            {
                if (0 == String.Compare( mListSTStrategy[i].GetStrategyName( ), mCurrentStrategyName, true ))
                {
                    k = i;
                }
            }
            if ((-1) == k)
            {
                // We failed to find the "current" strategy, so let's
                // choose an arbitrary successor.
                mCurrentStrategyName = mListSTStrategy[0].GetStrategyName( );
                return;
            }
            // We found the current strategy at index k; so, we increment
            // this variable, with wrapping.
            k++;
            k %= n; // thus, k == n becomes k == 0
            mCurrentStrategyName = mListSTStrategy[k].GetStrategyName( );
        }




        // WARNING: When you get the "best" rotation and translation
        // from the following function, you must wait until the piece has
        // its origin at least as low as row 0 (zero) instead of its initial
        // row -1 (negative one) if any rotations (1,2,3) are required.
        // Perform all rotations, and then perform translations.  This
        // avoids the problem of getting the piece jammed on the sides
        // of the board where rotation is impossible. ***
        // Also, the following strategy does not take advantage of the
        // possibility of using free-fall and future movements to
        // slide under overhangs and fill them in.

        public static void GetBestMoveOncePerPiece
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

            STStrategy strategy = null;

            strategy = GetCurrentStrategy();

            if (null == strategy)
            {
                return;
            }

            strategy.GetBestMoveOncePerPiece
            (
                board,
                piece,
                nextPieceFlag, // false == no next piece available or known
                nextPieceShape, // None == no piece available or known
                ref bestRotationDelta, // 0 or {0,1,2,3}
                ref bestTranslationDelta // 0 or {...,-2,-1,0,1,2,...}
            );
        }


        //The default algorithm for the bot depends on 6 coefficients to evaluate
        //each possible position of the piece.  You can set the environment
        //variables XTBOT_FRONTIER, XTBOT_HEIGHT, XTBOT_HOLE, XTBOT_DROP,
        //XTBOT_PIT, XTBOT_EHOLE.  See the file decide.c to see what they do.

        //The values for the coefficients that xtbot uses now were obtained with a
        //genetic algorithm using a population of 50 sets of coefficients,
        //calculating 18 generations in about 500 machine-hours distributed among
        //20-odd Sparc workstations.  This improved the average number of lines
        //from 10,000 to about 50,000.  The code used for this isn't nearly clean
        //enough to distribute in a release.  If you're interesed, please e-mail
        //the author privately.




    }
}
