// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.


namespace CPF.StandardTetris
{
    public sealed class STPrecisionTimer
    {
        private bool mStarted;
        private long mCountsPerSecond;
        private long mStartCount;


        private void ClearAllFields ( )
        {
            this.mStarted = false;
            this.mCountsPerSecond = 0;
            this.mStartCount = 0;
        }


        public STPrecisionTimer ( )
        {
            this.ClearAllFields( );
        }


        public void SetReferenceTimeToNow ( )
        {
            if (false == this.mStarted)
            {
                if (false == STPrecisionTimer.Kernel32_QueryPerformanceFrequency( out this.mCountsPerSecond ))
                {
                    // Failed
                }
                else
                {
                    this.mStarted = true;
                }
            }

            if (true == mStarted)
            {
                STPrecisionTimer.Kernel32_QueryPerformanceCounter( out this.mStartCount );
            }
        }



        public double GetElapsedTimeSeconds ( )
        {
            if (true == this.mStarted)
            {
                long currentCount = 0;
                STPrecisionTimer.Kernel32_QueryPerformanceCounter( out currentCount );

                if (this.mCountsPerSecond > 0)
                {
                    return ((double)(currentCount - this.mStartCount) / ((double)this.mCountsPerSecond));
                }
                else
                {
                    return (0.0);
                }
            }

            return (0.0);
        }


        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "QueryPerformanceFrequency" ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        public
        static
        extern
        bool
        Kernel32_QueryPerformanceFrequency
        (
            out long countFrequency
        );


        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "QueryPerformanceCounter" ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        public
        static
        extern
        bool
        Kernel32_QueryPerformanceCounter
        (
            out long countValue
        );


    }
}
