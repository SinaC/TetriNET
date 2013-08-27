// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;



namespace CPF.StandardTetris
{
    public class STConsole
    {

        private List<String> mListString = new List<string>( );
        private int mMaximumLines = 24;

        public void ClearAllLines ( )
        {
            mListString.Clear( );
        }

        public int GetTotalLines ( )
        {
            return (mListString.Count);
        }

        public String GetLineByIndex ( int index )
        {
            if ((index < 0) || (index >= mListString.Count))
            {
                return ("");
            }
            return (mListString[index]);
        }

        public void AddLine ( String text )
        {
            // clobber lines until we have a free slot at the end
            while (mListString.Count >= mMaximumLines)
            {
                mListString.RemoveAt( 0 );
            }

            mListString.Add( text );
        }

        public void AddToLastLine ( String text )
        {
            // clobber lines until we have a free slot at the end
            while (mListString.Count >= mMaximumLines)
            {
                mListString.RemoveAt( 0 );
            }

            if (mListString.Count <= 0)
            {
                mListString.Add( text );
            }
            else
            {
                mListString[mListString.Count - 1] = 
                    mListString[mListString.Count - 1] + text;
            }
        }

    }
}
