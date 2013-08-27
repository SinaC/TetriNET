// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.Text;
using System.IO;



namespace CPF.StandardTetris
{
    public class STFileReader
    {
        private List<String> mListString;



        public STFileReader ( )
        {
        }



        public bool ReadFileAndGetClusterStrings ( String filePathAndName )
        {
            mListString = new List<string>();


            FileStream fileStream;
            StreamReader streamReader;

            try
            {
                fileStream =
                    new FileStream
                    (
                        filePathAndName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read | FileShare.Delete
                    );
            }
            catch
            {
                fileStream = null;
                return(false);
            }

            try
            {
                streamReader =
                    new StreamReader( fileStream, Encoding.ASCII );
            }
            catch
            {
                fileStream = null;
                streamReader = null;
                return (false);
            }




            // Put contiguous non-whitespace in to strings, and add strings to list.
            StringBuilder sb = new StringBuilder( );
            Char c = (Char)0;
            bool collectingCharacters = false;

            while (false == streamReader.EndOfStream)
            {
                c = (Char) streamReader.Read( );

                if 
                    (
                       (' ' == c) 
                    || ('\t' == c)
                    || ('\r' == c)
                    || ('\n' == c)
                    || ('\b' == c)
                    )
                {
                    // Whitespace encountered
                    if (true == collectingCharacters)
                    {
                        this.mListString.Add( sb.ToString() );
                        sb.Remove(0, sb.Length); // Clobber string
                        collectingCharacters = false;
                    }
                    else
                    {
                        // Still whitespace...
                    }
                }
                else
                {
                    // Non-whitespace
                    if (true == collectingCharacters)
                    {
                        // Continue accumulating
                        sb.Append( c );
                    }
                    else
                    {
                        // Start accumulating
                        sb.Remove( 0, sb.Length ); // Clear string
                        sb.Append( c ); // Append first character
                        collectingCharacters = true;
                    }
                }
            }




            // If we are still in the character collection state,
            // add the final string to the list.
            if (true == collectingCharacters)
            {
                this.mListString.Add( sb.ToString( ) );
                sb.Remove( 0, sb.Length );  // Clobber string
                collectingCharacters = false;
            }




            if (null != streamReader)
            {
                try
                {
                    // Close() calls Dispose(true) on file
                    streamReader.Close( );
                }
                catch
                {
                }

                streamReader = null;
            }


            return (true);
        }







        public int GetTotalStrings( )
        {
            return(this.mListString.Count);
        }

        public bool GetStringByIndex ( int index, ref String text )
        {
            text = "";

            int totalStrings = 0;
            totalStrings = this.mListString.Count;

            if ((index < 0) || (index >= totalStrings))
            {
                return (false);
            }

            text = this.mListString[index];

            return(true);
        }






        public int GetStringAsInteger32ByIndex ( int index )
        {
            String text = "";
            if (false == this.GetStringByIndex( index, ref text ))
            {
                return (0);
            }

            int value = 0;
            try
            {
                value = Convert.ToInt32( text );
            }
            catch
            {
            }
            return (value);
        }

        public long GetStringAsInteger64ByIndex ( int index )
        {
            String text = "";
            if (false == this.GetStringByIndex( index, ref text ))
            {
                return (0);
            }

            long value = 0;
            try
            {
                value = Convert.ToInt64( text );
            }
            catch
            {
            }
            return (value);
        }

        public float GetStringAsFloatByIndex ( int index )
        {
            String text = "";
            if (false == this.GetStringByIndex( index, ref text ))
            {
                return (0);
            }

            float value = 0.0f;
            try
            {
                value = Convert.ToSingle( text );
            }
            catch
            {
            }
            return (value);
        }

        public double GetStringAsDoubleByIndex( int index )
        {
            String text = "";
            if (false == this.GetStringByIndex( index, ref text ))
            {
                return (0);
            }

            double value = 0.0;
            try
            {
                value = Convert.ToDouble( text );
            }
            catch
            {
            }
            return (value);
        }





        public int FindIndexOfStringAtOrAfterIndex( int startIndex, String text )
        {
            if (null == text)
            {
                return (-1);
            }

            if (text.Length <= 0)
            {
                return (-1);
            }

            if (startIndex < 0)
            {
                return (-1);
            }

            int totalStrings = 0;
            totalStrings = this.mListString.Count;

            if (startIndex >= totalStrings)
            {
                return (-1);
            }

            int index = 0;

            for
            (
                index = startIndex;
                index < totalStrings;
                index++
            )
            {
                if (0 == String.Compare( text, this.mListString[index], true ))
                {
                    return (index);
                }
            }

            return (-1);
        }

        public int FindString ( String text ) // (-1)==Not found, otherwise index
        {
            return ( this.FindIndexOfStringAtOrAfterIndex( 0, text ) );
        }




        public void FindTextAndInteger32AtOrAfterIndex( int index, String text, ref int value )
        {
            value = 0;

            int indexText = 0;
            indexText = this.FindIndexOfStringAtOrAfterIndex( index, text );
            if (indexText < 0)
            {
                return;
            }

            value = this.GetStringAsInteger32ByIndex( indexText + 1 );
        }

        public void FindTextAndInteger64AtOrAfterIndex( int index, String text, ref long value )
        {
            value = 0;

            int indexText = 0;
            indexText = this.FindIndexOfStringAtOrAfterIndex( index, text );
            if (indexText < 0)
            {
                return;
            }

            value = this.GetStringAsInteger64ByIndex( indexText + 1 );
        }

        public void FindTextAndFloatAtOrAfterIndex( int index, String text, ref float value )
        {
            value = 0.0f;

            int indexText = 0;
            indexText = this.FindIndexOfStringAtOrAfterIndex( index, text );
            if (indexText < 0)
            {
                return;
            }

            value = this.GetStringAsFloatByIndex( indexText + 1 );
        }

        public void FindTextAndDoubleAtOrAfterIndex( int index, String text, ref double value )
        {
            value = 0.0;

            int indexText = 0;
            indexText = this.FindIndexOfStringAtOrAfterIndex( index, text );
            if (indexText < 0)
            {
                return;
            }

            value = this.GetStringAsDoubleByIndex( indexText + 1 );
        }






        public void FindTextAndInteger32( String text, ref int value )
        {
            value = 0;
            FindTextAndInteger32AtOrAfterIndex( 0, text, ref value );
        }

        public void FindTextAndInteger64( String text, ref long value )
        {
            value = 0;
            FindTextAndInteger64AtOrAfterIndex( 0, text, ref value );
        }

        public void FindTextAndFloat( String text, ref float value )
        {
            value = 0.0f;
            FindTextAndFloatAtOrAfterIndex( 0, text, ref value );
        }

        public void FindTextAndDouble( String text, ref double value )
        {
            value = 0.0;
            FindTextAndDoubleAtOrAfterIndex( 0, text, ref value );
        }



    }
}
