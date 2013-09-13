// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.


using System.Security.Cryptography;



namespace CPF.StandardTetris
{
    public class STRandom
    {
        private MD5 mMD5;
        private long mState;
        

        public STRandom ( )
        {
            mMD5 = MD5CryptoServiceProvider.Create( );
        }


        public long GetState ( )
        {
            return (this.mState);
        }


        public void SetState ( long state )
        {
            this.mState = state;
        }


        public void Advance ( )
        {
            this.mState += 1;
        }

        public long GetRandomBits ( )
        {
            byte[] input = new byte[8];
            int i = 0;
            for ( i = 0; i < 8; i++ )
            {
                input[i] = (byte)((this.mState >> (8 * i)) & 0xff);
            }
            byte[] output = this.mMD5.ComputeHash( input );
            long outputValue = 0;
            for (i = 0; i < 8; i++)
            {
                outputValue |= ((long)output[i]) << (8 * i);
            }
            return (outputValue);
        }

        public int GetIntegerInRangeUsingCurrentState
        (
            int minimum,
            int maximum
        )
        {
            if (minimum == maximum)
            {
                // Trivial case: no range at all.
                return(minimum);
            }

            // If the specified range is not in order, then determine
            // the actual minimum and maximum.  (We will always return
            // a random value in the specified interval, even if the
            // specified interval is not in an ascending order.)
            int actualMinimum = 0;
            int actualMaximum = 0;
            if (minimum <= maximum)
            {
                actualMinimum = minimum;
                actualMaximum = maximum;
            }
            else
            {
                actualMinimum = maximum;
                actualMaximum = minimum;
            }

            int totalValues = ((actualMaximum - actualMinimum) + 1);
            // e.g., a range of 3,4,5,6,7 has ((7-3)+1) = 5 values


            long randomBits = this.GetRandomBits( );

            long mask = 0x7fffffffffffffff;
            long maskedValue = (randomBits & mask);
            if (maskedValue == mask)
            {
                maskedValue = 0; // prevent fraction from hitting 1.0
            }
            double fraction = (double)maskedValue / (double)mask; // 0.0 ... 0.9999...
            double relativeValue = (double)totalValues * fraction; // 0.0 ... (0.999... * totalValues)

            int value = actualMinimum + (int)relativeValue;

            // Clip to minimum in the unexpected event that the computed
            // value is less than the desired minimum.  This shouldn't
            // happen, but paranoia is awesome.
            if (value < actualMinimum)
            {
                value = actualMinimum;
            }

            // Clip to maximum in the unexpected event that the computed
            // value is greater than the desired maximum.  This shouldn't
            // happen, but paranoia is awesome.
            if (value > actualMaximum)
            {
                value = actualMaximum;
            }

            return (value);
        }
    }
}
