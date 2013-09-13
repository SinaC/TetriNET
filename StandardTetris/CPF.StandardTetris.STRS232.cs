// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Windows.Forms;



namespace CPF.StandardTetris
{
    public class STRS232
    {
        private const int KEY_WAIT_MILLISECONDS  = 30;
        private const int KEY_PRESS_MILLISECONDS = 30;

        private static IntPtr mHandleCom = IntPtr.Zero;



        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct DCB 
        {
            public int DCBlength;           // sizeof(DCB) 
            public int BaudRate;            // current baud rate 
            public int flags;
            // int fBinary: 1;          // (1<<31) binary mode, no EOF check 
            // int fParity: 1;          // (1<<30) enable parity checking 
            // int fOutxCtsFlow:1;      // (1<<29) CTS output flow control 
            // int fOutxDsrFlow:1;      // (1<<28) DSR output flow control 
            // int fDtrControl:2;       // (1<<27) (1<<26) DTR flow control type 
            // int fDsrSensitivity:1;   // (1<<25) DSR sensitivity 
            // int fTXContinueOnXoff:1; // (1<<24) XOFF continues Tx 
            // int fOutX: 1;            // (1<<23) XON/XOFF out flow control 
            // int fInX: 1;             // (1<<22) XON/XOFF in flow control 
            // int fErrorChar: 1;       // (1<<21) enable error replacement 
            // int fNull: 1;            // (1<<20) enable null stripping 
            // int fRtsControl:2;       // (1<<19) (1<<18) RTS flow control 
            // int fAbortOnError:1;     // (1<<17) abort reads/writes on error 
            // int fDummy2:17;          // (1<<16)...(1<<0) reserved 
            public short wReserved;         // not currently used 
            public short XonLim;            // transmit XON threshold 
            public short XoffLim;           // transmit XOFF threshold 
            public byte ByteSize;           // number of bits/byte, 4-8 
            public byte Parity;             // 0-4=no,odd,even,mark,space 
            public byte StopBits;           // 0,1,2 = 1, 1.5, 2 
            public byte XonChar;            // Tx and Rx XON character 
            public byte XoffChar;           // Tx and Rx XOFF character 
            public byte ErrorChar;          // error replacement character 
            public byte EofChar;            // end of input character 
            public byte EvtChar;            // received event character 
            public short wReserved1;        // reserved; do not use 
        }


        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct SECURITY_ATTRIBUTES 
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public byte bInheritHandle; 
        }



        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct OVERLAPPED
        {
            public int Internal;
            public int InternalHigh;
            public int Offset;
            public int OffsetHigh;
            public IntPtr hEvent; 
        }


        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct COMMTIMEOUTS 
        {
            public int ReadIntervalTimeout;
            public int ReadTotalTimeoutMultiplier;
            public int ReadTotalTimeoutConstant;
            public int WriteTotalTimeoutMultiplier;
            public int WriteTotalTimeoutConstant; 
        } 
 

        private const int GENERIC_READ = unchecked((int)(0x80000000));
        private const int GENERIC_WRITE = (0x40000000);
        private const int GENERIC_EXECUTE = (0x20000000);
        private const int GENERIC_ALL = (0x10000000);


        private const int CREATE_NEW = 1;
        private const int CREATE_ALWAYS = 2;
        private const int OPEN_EXISTING = 3;
        private const int OPEN_ALWAYS = 4;
        private const int TRUNCATE_EXISTING = 5;

        private static IntPtr INVALID_HANDLE_VALUE = new IntPtr( -1 );

        private const int CBR_9600 = 9600;

        private const int NOPARITY = 0;
        private const int ODDPARITY = 1;
        private const int EVENPARITY = 2;
        private const int MARKPARITY = 3;
        private const int SPACEPARITY = 4;

        private const int ONESTOPBIT = 0;
        private const int ONE5STOPBITS = 1;
        private const int TWOSTOPBITS = 2;

        private const int IGNORE = 0; // Ignore signal
        private const int INFINITE = unchecked( (int)0xFFFFFFFF );  // Infinite timeout



        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "CreateFileA", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        private
        static
        extern
        IntPtr
        CreateFile 
        (
            String fileName,
            int dwDesiredAccess,
            int dwShareMode,
            ref SECURITY_ATTRIBUTES securityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );



        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "SetupComm", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        SetupComm
        (
            IntPtr hFile,
            int dwInQueue,
            int dwOutQueue
        );



        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "GetCommState", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        GetCommState 
        (
            IntPtr hFile,
            ref DCB dcb
        );



        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "SetCommState", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        SetCommState 
        (
            IntPtr hFile,
            ref DCB dcb
        );



        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "WriteFile", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        WriteFile 
        (
            IntPtr hFile,
            byte[] buffer,
            int numberOfBytesToWrite,
            ref int numberOfBytesWritten,
            ref OVERLAPPED overlapped
        );


        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "FlushFileBuffers", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        FlushFileBuffers 
        (
          IntPtr hFile   // open handle to file whose buffers are to be flushed
        );


        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "CloseHandle", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        CloseHandle 
        (
            IntPtr hObject   // handle to object to close
        );




        [System.Runtime.InteropServices.DllImport( "kernel32", EntryPoint = "SetCommTimeouts", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        SetCommTimeouts 
        (
            IntPtr hFile,                  // handle to comm device
            ref COMMTIMEOUTS commTimeouts  // pointer to comm time-out structure
        );





        public static int   InitializePort( )
        {

            String comPort = "COM1";
            SECURITY_ATTRIBUTES security_attributes = new SECURITY_ATTRIBUTES();

            mHandleCom = 
                CreateFile
                (
                    comPort,
                    (GENERIC_READ | GENERIC_WRITE),
                    0,    // comm devices must be opened w/exclusive-access
                    ref security_attributes, // no security attributes
                    OPEN_EXISTING, // comm devices must use OPEN_EXISTING
                    0,    // not overlapped I/O
                    IntPtr.Zero  // hTemplate must be NULL for comm devices
                );


            if (INVALID_HANDLE_VALUE == mHandleCom)
            {
                mHandleCom = IntPtr.Zero;

                MessageBox.Show
                ( 
                    "CreateFile() failed to create COM1",
                    "ERROR: STRS232.InitializePort()",
                    MessageBoxButtons.OK
                );

                return (1);
            }


            //  BOOL SetupComm(
            //  HANDLE hFile,     // handle to communications device
            //  DWORD dwInQueue,  // size of input buffer
            //  DWORD dwOutQueue  // size of output buffer
            //);


            SetupComm
            ( 
                mHandleCom, 
                1024, 
                1024 
            );


            // We will build on the current configuration, and skip setting the size
            // of the input and output buffers with SetupComm.

            DCB dcb = new DCB( );

            bool getCommStateResult = false;

            getCommStateResult = 
                GetCommState
                ( 
                    mHandleCom, 
                    ref dcb 
                );

            if (false == getCommStateResult)
            {
                MessageBox.Show
                (
                    "GetCommState() failed",
                    "ERROR: STRS232.InitializePort()",
                    MessageBoxButtons.OK
                );

                return (2);
            }

            // Fill in the DCB: baud=9,600 bps, 8 data bits, no parity, and 1 stop bit.

            dcb.BaudRate = CBR_9600;     // set the baud rate
            dcb.ByteSize = 8;            // data size, xmit, and rcv
            dcb.Parity = NOPARITY;     // no parity bit
            dcb.StopBits = ONESTOPBIT;   // one stop bit

            bool setCommStateResult = false;

            setCommStateResult = 
                SetCommState
                ( 
                    mHandleCom, 
                    ref dcb 
                );


            if (false == setCommStateResult)
            {
                MessageBox.Show
                (
                    "SetCommState() failed",
                    "ERROR: STRS232.InitializePort()",
                    MessageBoxButtons.OK
                );

                return (3);
            }


            //typedef struct _COMMTIMEOUTS {  
            //  DWORD ReadIntervalTimeout; 
            //  DWORD ReadTotalTimeoutMultiplier; 
            //  DWORD ReadTotalTimeoutConstant; 
            //  DWORD WriteTotalTimeoutMultiplier; 
            //  DWORD WriteTotalTimeoutConstant; 
            //} COMMTIMEOUTS,*LPCOMMTIMEOUTS; 

            COMMTIMEOUTS timeouts = new COMMTIMEOUTS();
            timeouts.ReadIntervalTimeout = 100;
            timeouts.ReadTotalTimeoutMultiplier = 1;
            timeouts.ReadTotalTimeoutConstant = 100;
            timeouts.WriteTotalTimeoutMultiplier = 1;
            timeouts.WriteTotalTimeoutConstant = 100;

            //BOOL SetCommTimeouts(
            //  HANDLE hFile,                  // handle to comm device
            //  LPCOMMTIMEOUTS lpCommTimeouts  // time-out values
            //);

            bool setCommTimeoutsResult = false;

            setCommTimeoutsResult = 
                SetCommTimeouts
                ( 
                    mHandleCom, 
                    ref timeouts 
                );


            if (false == setCommTimeoutsResult)
            {
                MessageBox.Show
                (
                    "SetCommTimeouts() failed",
                    "ERROR: STRS232.InitializePort()",
                    MessageBoxButtons.OK
                );

                return (4);
            }

            // Serial port successfully reconfigured.

            return (0);
        }









        public static int TerminatePort ( )
        {
            if 
                (
                (INVALID_HANDLE_VALUE == mHandleCom) ||
                (IntPtr.Zero == mHandleCom)
                )
            {
                mHandleCom = IntPtr.Zero;
                return(0);
            }

            bool closeHandleResult = false;

            closeHandleResult =
                CloseHandle( mHandleCom );

            mHandleCom = IntPtr.Zero;

            if (false == closeHandleResult)
            {
                MessageBox.Show
                (
                    "CloseHandle() failed",
                    "ERROR: STRS232.TerminatePort()",
                    MessageBoxButtons.OK
                );

                return (1);
            }

            return(0);
        }




        public static void SetRelay ( int relayIndex, bool relayState )
        {
	        if 
                ( 
                (INVALID_HANDLE_VALUE == mHandleCom) ||
                (IntPtr.Zero == mHandleCom) 
                )
	          {
		        return;
	          }

	        byte[] message = new byte[32];

	        if (true == relayState)
	          {
		        message[0] = (byte)'s';
		        message[1] = (byte)'k';
		        message[2] = (byte)((int)'0' + relayIndex);
		        message[3] = (byte)'\r';
		        message[4] = (byte)'\0';
	          }
	        else
	          {
		        message[0] = (byte)'r';
		        message[1] = (byte)'k';
		        message[2] = (byte)((int)'0' + relayIndex);
		        message[3] = (byte)'\r';
		        message[4] = (byte)'\0';
	          }

	        int bytesWritten = 0;
	        bool writeFileResult = false;
            OVERLAPPED overlapped = new OVERLAPPED( );

            writeFileResult = 
		        WriteFile
		          (
		              mHandleCom,
		              message,
		              4,
                      ref bytesWritten,
                      ref overlapped
		          );

            if (false == writeFileResult)
	          {
                MessageBox.Show
                (
                    "WriteFile() failed", 
                    "ERROR: STRS232.SetRelay()",
                    MessageBoxButtons.OK
                );

		        return;
	          }

	        FlushFileBuffers( mHandleCom );
        }





        public static void SetRelayBits ( int bits )
        {
	        if 
                (
                (INVALID_HANDLE_VALUE == mHandleCom) ||
                (IntPtr.Zero == mHandleCom) 
                )
            {
                return;
            }

	        byte[] message = new byte[32];

	        //sprintf( message, "MK%03d\r", bits );
	        message[0] = (byte)'M';
	        message[1] = (byte)'K';
	        message[2] = (byte)((int)'0' + ((bits / 100) % 10));
	        message[3] = (byte)((int)'0' + ((bits / 10) % 10));
	        message[4] = (byte)((int)'0' + ((bits / 1) % 10));
	        message[5] = (byte)'\r'; // CR
	        message[6] = (byte)0;

            int bytesWritten = 0;
            bool writeFileResult = false;
            OVERLAPPED overlapped = new OVERLAPPED();

            writeFileResult =
                WriteFile
                  (
                      mHandleCom,
                      message,
                      6,
                      ref bytesWritten,
                      ref overlapped
                  );

            if (false == writeFileResult)
            {
                MessageBox.Show
                (
                    "WriteFile() failed", 
                    "ERROR: STRS232.SetRelayBits()",
                    MessageBoxButtons.OK
                );

		        return;
	          }

	        FlushFileBuffers( mHandleCom );
        }


















        public static void Test ( )
        {
            int i = 0;
            int n = 0;
            n = 256;
            for (i = 0; i < n; i++)
            {
                STRS232.SetRelayBits( (int)(i & 0xff) );
                //SetRelay( (i%8), (int)((i/13)&0x1) );
                System.Threading.Thread.Sleep( 1 );
            }
            STRS232.SetRelayBits( 0 );
        }








        // Tetris controls
        public static void MomentaryRelay_LEFT ( ) // Relay 0
        {
            System.Threading.Thread.Sleep( STRS232.KEY_WAIT_MILLISECONDS );
            STRS232.SetRelayBits( 0x01 );
            System.Threading.Thread.Sleep( STRS232.KEY_PRESS_MILLISECONDS );
            STRS232.SetRelayBits( 0 );
        }

        public static void MomentaryRelay_RIGHT ( ) // Relay 1
        {
            System.Threading.Thread.Sleep( STRS232.KEY_WAIT_MILLISECONDS );
            STRS232.SetRelayBits( 0x02 );
            System.Threading.Thread.Sleep( STRS232.KEY_PRESS_MILLISECONDS );
            STRS232.SetRelayBits( 0 );
        }

        public static void MomentaryRelay_ROTATE ( ) // Relay 2
        {
            System.Threading.Thread.Sleep( STRS232.KEY_WAIT_MILLISECONDS );
            STRS232.SetRelayBits( 0x04 );
            System.Threading.Thread.Sleep( STRS232.KEY_PRESS_MILLISECONDS );
            STRS232.SetRelayBits( 0 );
        }

        public static void MomentaryRelay_DROP ( ) // Relay 3
        {
            System.Threading.Thread.Sleep( STRS232.KEY_WAIT_MILLISECONDS );
            STRS232.SetRelayBits( 0x08 );
            System.Threading.Thread.Sleep( STRS232.KEY_PRESS_MILLISECONDS );
            STRS232.SetRelayBits( 0 );
        }

        public static void MomentaryRelay_RESET ( ) // Relay 4
        {
            System.Threading.Thread.Sleep( STRS232.KEY_WAIT_MILLISECONDS );
            STRS232.SetRelayBits( 0x10 );
            System.Threading.Thread.Sleep( STRS232.KEY_PRESS_MILLISECONDS );
            STRS232.SetRelayBits( 0 );
        }

    }
}
