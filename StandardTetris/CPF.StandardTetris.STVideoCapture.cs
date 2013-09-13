// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.
// This file contains information adapted from several Microsoft files
// (e.g., VFW.H and WINDOWS.H).



using System;
using System.Collections.Generic;


namespace CPF.StandardTetris
{
    public class STVideoCapture
    {

        // See windows.h for HWND, LRESULT, CALLBACK, ...
        // See vfw.h for LPVIDEOHDR, capCreateCaptureWindow(), ...

        
        // From VFW.H

        // video data block header
        //typedef struct videohdr_tag {
        //    LPBYTE      lpData;                 // pointer to locked data buffer
        //    DWORD       dwBufferLength;         // Length of data buffer
        //    DWORD       dwBytesUsed;            // Bytes actually used
        //    DWORD       dwTimeCaptured;         // Milliseconds from start of stream
        //    DWORD       dwUser;                 // for client's use
        //    DWORD       dwFlags;                // assorted flags (see defines)
        //    DWORD       dwReserved[4];          // reserved for driver
        //} VIDEOHDR, NEAR *PVIDEOHDR, FAR * LPVIDEOHDR;
        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct VIDEOHDR
        {
            public IntPtr lpData;           // pointer to locked data buffer
            public int dwBufferLength;   // Length of data buffer
            public int dwBytesUsed;      // Bytes actually used
            public int dwTimeCaptured;   // Milliseconds from start of stream
            public int dwUser;           // for client's use
            public int dwFlags;          // assorted flags (see defines)
            public IntPtr dwReserved;       // reserved for driver
        }

        // dwFlags field of VIDEOHDR
        public const int VHDR_DONE = 0x00000001; // Done bit
        public const int VHDR_PREPARED = 0x00000002; // Set if this header has been prepared
        public const int VHDR_INQUEUE = 0x00000004; // Reserved for driver
        public const int VHDR_KEYFRAME = 0x00000008; // Key Frame


        public const int WM_USER = 0x0400;
        public const int WM_CAP_START = WM_USER;
        public const int WM_CAP_UNICODE_START = (WM_USER + 100);
        public const int WM_CAP_GET_CAPSTREAMPTR = (WM_CAP_START + 1);
        public const int WM_CAP_SET_CALLBACK_ERRORW = (WM_CAP_UNICODE_START + 2);
        public const int WM_CAP_SET_CALLBACK_STATUSW = (WM_CAP_UNICODE_START + 3);
        public const int WM_CAP_SET_CALLBACK_ERRORA = (WM_CAP_START + 2);
        public const int WM_CAP_SET_CALLBACK_STATUSA = (WM_CAP_START + 3);
        // public const int WM_CAP_SET_CALLBACK_ERROR = WM_CAP_SET_CALLBACK_ERRORW;
        // public const int WM_CAP_SET_CALLBACK_STATUS = WM_CAP_SET_CALLBACK_STATUSW;
        public const int WM_CAP_SET_CALLBACK_ERROR = WM_CAP_SET_CALLBACK_ERRORA;
        public const int WM_CAP_SET_CALLBACK_STATUS = WM_CAP_SET_CALLBACK_STATUSA;
        public const int WM_CAP_SET_CALLBACK_YIELD = (WM_CAP_START + 4);
        public const int WM_CAP_SET_CALLBACK_FRAME = (WM_CAP_START + 5);
        public const int WM_CAP_SET_CALLBACK_VIDEOSTREAM = (WM_CAP_START + 6);
        public const int WM_CAP_SET_CALLBACK_WAVESTREAM = (WM_CAP_START + 7);
        public const int WM_CAP_GET_USER_DATA = (WM_CAP_START + 8);
        public const int WM_CAP_SET_USER_DATA = (WM_CAP_START + 9);
        public const int WM_CAP_DRIVER_CONNECT = (WM_CAP_START + 10);
        public const int WM_CAP_DRIVER_DISCONNECT = (WM_CAP_START + 11);
        public const int WM_CAP_DRIVER_GET_NAMEA = (WM_CAP_START + 12);
        public const int WM_CAP_DRIVER_GET_VERSIONA = (WM_CAP_START + 13);
        public const int WM_CAP_DRIVER_GET_NAMEW = (WM_CAP_UNICODE_START + 12);
        public const int WM_CAP_DRIVER_GET_VERSIONW = (WM_CAP_UNICODE_START + 13);
        // public const int WM_CAP_DRIVER_GET_NAME = WM_CAP_DRIVER_GET_NAMEW;
        // public const int WM_CAP_DRIVER_GET_VERSION = WM_CAP_DRIVER_GET_VERSIONW;
        public const int WM_CAP_DRIVER_GET_NAME = WM_CAP_DRIVER_GET_NAMEA;
        public const int WM_CAP_DRIVER_GET_VERSION = WM_CAP_DRIVER_GET_VERSIONA;
        public const int WM_CAP_DRIVER_GET_CAPS = (WM_CAP_START + 14);
        public const int WM_CAP_FILE_SET_CAPTURE_FILEA = (WM_CAP_START + 20);
        public const int WM_CAP_FILE_GET_CAPTURE_FILEA = (WM_CAP_START + 21);
        public const int WM_CAP_FILE_SAVEASA = (WM_CAP_START + 23);
        public const int WM_CAP_FILE_SAVEDIBA = (WM_CAP_START + 25);
        public const int WM_CAP_FILE_SET_CAPTURE_FILEW = (WM_CAP_UNICODE_START + 20);
        public const int WM_CAP_FILE_GET_CAPTURE_FILEW = (WM_CAP_UNICODE_START + 21);
        public const int WM_CAP_FILE_SAVEASW = (WM_CAP_UNICODE_START + 23);
        public const int WM_CAP_FILE_SAVEDIBW = (WM_CAP_UNICODE_START + 25);
        // public const int WM_CAP_FILE_SET_CAPTURE_FILE = WM_CAP_FILE_SET_CAPTURE_FILEW;
        // public const int WM_CAP_FILE_GET_CAPTURE_FILE = WM_CAP_FILE_GET_CAPTURE_FILEW;
        // public const int WM_CAP_FILE_SAVEAS = WM_CAP_FILE_SAVEASW;
        // public const int WM_CAP_FILE_SAVEDIB = WM_CAP_FILE_SAVEDIBW;
        public const int WM_CAP_FILE_SET_CAPTURE_FILE = WM_CAP_FILE_SET_CAPTURE_FILEA;
        public const int WM_CAP_FILE_GET_CAPTURE_FILE = WM_CAP_FILE_GET_CAPTURE_FILEA;
        public const int WM_CAP_FILE_SAVEAS = WM_CAP_FILE_SAVEASA;
        public const int WM_CAP_FILE_SAVEDIB = WM_CAP_FILE_SAVEDIBA;
        public const int WM_CAP_FILE_ALLOCATE = (WM_CAP_START + 22);
        public const int WM_CAP_FILE_SET_INFOCHUNK = (WM_CAP_START + 24);
        public const int WM_CAP_EDIT_COPY = (WM_CAP_START + 30);
        public const int WM_CAP_SET_AUDIOFORMAT = (WM_CAP_START + 35);
        public const int WM_CAP_GET_AUDIOFORMAT = (WM_CAP_START + 36);
        public const int WM_CAP_DLG_VIDEOFORMAT = (WM_CAP_START + 41);
        public const int WM_CAP_DLG_VIDEOSOURCE = (WM_CAP_START + 42);
        public const int WM_CAP_DLG_VIDEODISPLAY = (WM_CAP_START + 43);
        public const int WM_CAP_GET_VIDEOFORMAT = (WM_CAP_START + 44);
        public const int WM_CAP_SET_VIDEOFORMAT = (WM_CAP_START + 45);
        public const int WM_CAP_DLG_VIDEOCOMPRESSION = (WM_CAP_START + 46);
        public const int WM_CAP_SET_PREVIEW = (WM_CAP_START + 50);
        public const int WM_CAP_SET_OVERLAY = (WM_CAP_START + 51);
        public const int WM_CAP_SET_PREVIEWRATE = (WM_CAP_START + 52);
        public const int WM_CAP_SET_SCALE = (WM_CAP_START + 53);
        public const int WM_CAP_GET_STATUS = (WM_CAP_START + 54);
        public const int WM_CAP_SET_SCROLL = (WM_CAP_START + 55);
        public const int WM_CAP_GRAB_FRAME = (WM_CAP_START + 60);
        public const int WM_CAP_GRAB_FRAME_NOSTOP = (WM_CAP_START + 61);
        public const int WM_CAP_SEQUENCE = (WM_CAP_START + 62);
        public const int WM_CAP_SEQUENCE_NOFILE = (WM_CAP_START + 63);
        public const int WM_CAP_SET_SEQUENCE_SETUP = (WM_CAP_START + 64);
        public const int WM_CAP_GET_SEQUENCE_SETUP = (WM_CAP_START + 65);
        public const int WM_CAP_SET_MCI_DEVICEA = (WM_CAP_START + 66);
        public const int WM_CAP_GET_MCI_DEVICEA = (WM_CAP_START + 67);
        public const int WM_CAP_SET_MCI_DEVICEW = (WM_CAP_UNICODE_START + 66);
        public const int WM_CAP_GET_MCI_DEVICEW = (WM_CAP_UNICODE_START + 67);
        // public const int WM_CAP_SET_MCI_DEVICE = WM_CAP_SET_MCI_DEVICEW;
        // public const int WM_CAP_GET_MCI_DEVICE = WM_CAP_GET_MCI_DEVICEW;
        public const int WM_CAP_SET_MCI_DEVICE = WM_CAP_SET_MCI_DEVICEA;
        public const int WM_CAP_GET_MCI_DEVICE = WM_CAP_GET_MCI_DEVICEA;
        public const int WM_CAP_STOP = (WM_CAP_START + 68);
        public const int WM_CAP_ABORT = (WM_CAP_START + 69);
        public const int WM_CAP_SINGLE_FRAME_OPEN = (WM_CAP_START + 70);
        public const int WM_CAP_SINGLE_FRAME_CLOSE = (WM_CAP_START + 71);
        public const int WM_CAP_SINGLE_FRAME = (WM_CAP_START + 72);
        public const int WM_CAP_PAL_OPENA = (WM_CAP_START + 80);
        public const int WM_CAP_PAL_SAVEA = (WM_CAP_START + 81);
        public const int WM_CAP_PAL_OPENW = (WM_CAP_UNICODE_START + 80);
        public const int WM_CAP_PAL_SAVEW = (WM_CAP_UNICODE_START + 81);
        // public const int WM_CAP_PAL_OPEN = WM_CAP_PAL_OPENW;
        // public const int WM_CAP_PAL_SAVE = WM_CAP_PAL_SAVEW;
        public const int WM_CAP_PAL_OPEN = WM_CAP_PAL_OPENA;
        public const int WM_CAP_PAL_SAVE = WM_CAP_PAL_SAVEA;
        public const int WM_CAP_PAL_PASTE = (WM_CAP_START + 82);
        public const int WM_CAP_PAL_AUTOCREATE = (WM_CAP_START + 83);
        public const int WM_CAP_PAL_MANUALCREATE = (WM_CAP_START + 84);
        public const int WM_CAP_SET_CALLBACK_CAPCONTROL = (WM_CAP_START + 85);
        public const int WM_CAP_UNICODE_END = WM_CAP_PAL_SAVEW;
        public const int WM_CAP_END = WM_CAP_UNICODE_END;



        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct CAPDRIVERCAPS // 44 bytes
        {    
            public int wDeviceIndex;
            public int fHasOverlay;
            public int fHasDlgVideoSource;
            public int fHasDlgVideoFormat;
            public int fHasDlgVideoDisplay;
            public int fCaptureInitialized;
            public int fDriverSuppliesPalettes;
            public IntPtr hVideoIn;
            public IntPtr hVideoOut;
            public IntPtr hVideoExtIn;
            public IntPtr hVideoExtOut; 
        } 
 


        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct RGBQUAD
        {
            public byte rgbBlue;
            public byte rgbGreen;
            public byte rgbRed;
            public byte rgbReserved; 
        } 


        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct BITMAPINFOHEADER
        {
            public int biSize; // 40 bytes
            public int biWidth;
            public int biHeight;
            public short biPlanes;
            public short biBitCount;
            public int biCompression;
            public int biSizeImage;
            public int biXPelsPerMeter;
            public int biYPelsPerMeter;
            public int biClrUsed;
            public int biClrImportant; 
        }

        public const int BI_RGB = 0;
        public const int BI_RLE8 = 1;
        public const int BI_RLE4 = 2;



        [System.Runtime.InteropServices.StructLayout( System.Runtime.InteropServices.LayoutKind.Sequential )]
        public struct BITMAPINFO
        {
            public BITMAPINFOHEADER bmiHeader; // 40 bytes
            public RGBQUAD[] bmiColors; // 4 bytes
        }








        [System.Runtime.InteropServices.DllImport( "user32", EntryPoint = "IsWindow", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        IsWindow
        (
            IntPtr hWnd
        );


        [System.Runtime.InteropServices.DllImport( "user32", EntryPoint = "DestroyWindow", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        [return: System.Runtime.InteropServices.MarshalAs( System.Runtime.InteropServices.UnmanagedType.Bool )]
        private
        static
        extern
        bool
        DestroyWindow
        (
            IntPtr hWnd
        );


        [System.Runtime.InteropServices.DllImport( "user32", EntryPoint = "SendMessage", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        private
        static
        extern
        int
        SendMessage
        (
            IntPtr hWnd, // handle of destination window
            int Msg,     // message to send
            int wParam,  // first message parameter
            int lParam   // second message parameter
        );











        private static List<STVideoCapture> mListOfActiveInstances = new List<STVideoCapture>( );


        // Delegate for a video capture callback
        public delegate int DelegateCaptureCallback ( IntPtr hwnd, ref VIDEOHDR videoHdr );

        // Implementation of a video capture callback
        public static int CommonCaptureCallback
            (
                IntPtr hwnd,
                ref VIDEOHDR videoHdr
            )
        {
            // Attempt to find an instance of STVideoCapture with the specified hwnd.
            // If found, call it's delegate.
            foreach (STVideoCapture videoCapture in STVideoCapture.mListOfActiveInstances)
            {
                if (videoCapture.mHWNDCapture == hwnd)
                {
                    return (videoCapture.InstanceCaptureCallback( hwnd, ref videoHdr ));
                }
            }

            return (0);
        }

        public static DelegateCaptureCallback mCommonCaptureCallback =
            new DelegateCaptureCallback( CommonCaptureCallback );





        public bool capSetCallbackOnFrame ( IntPtr hwnd, DelegateCaptureCallback delegateCaptureCallback )
        {
            if (true == IsWindow( hwnd ))
            {
                int result = 0;
                if (null != delegateCaptureCallback)
                {
                    IntPtr delegatePointer =
                        System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate
                        (
                        delegateCaptureCallback
                        );

                    result = SendMessage( hwnd, WM_CAP_SET_CALLBACK_FRAME, 0, delegatePointer.ToInt32( ) );
                }
                else
                {
                    result = SendMessage( hwnd, WM_CAP_SET_CALLBACK_FRAME, 0, 0 );
                }

                if (0 == result)
                {
                    // 0 == failed (for capSetCallbackOnFrame())
                    return (false);
                }
                return (true);
            }
            return (false);
        }









        [System.Runtime.InteropServices.DllImport( "avicap32", EntryPoint = "capCreateCaptureWindowA", SetLastError = true ),
        System.Security.SuppressUnmanagedCodeSecurity]
        private
        static
        extern
        IntPtr
        capCreateCaptureWindow
        (
            String windowName,
            int dwStyle,
            int x, 
            int y, 
            int nWidth, 
            int nHeight,
            IntPtr hwndParent, 
            int nID
        );



        public bool capDriverConnect ( IntPtr hwnd, int indexOfCaptureDriver0Through9 )
        {
            if (true == IsWindow( hwnd ))
            {
                int result = 0;
                result = SendMessage( hwnd, WM_CAP_DRIVER_CONNECT, indexOfCaptureDriver0Through9, 0 );
                if (0 == result)
                {
                    return (false);
                }
                return (true);
            }
            return (false);
        }





        // UNTESTED
        // UNTESTED
        //
        // CAUTION:  The following implementation has not been tested.
        //           However, I think it is correct and will work fine.
        //
        //           I'm not using capDriverGetCaps() for anything right now, 
        //           so I'll simply leave this alone for now.
        //
        public static IntPtr mPersistingUnmanagedMemoryForCapDriverCapsResult = IntPtr.Zero;

        public bool capDriverGetCaps ( IntPtr hwnd, ref CAPDRIVERCAPS capDriverCaps, int sizeOfCapDriverCapsIs44Bytes )
        {
            if (true == IsWindow( hwnd ))
            {
                int sizeOfCAPDRIVERCAPSInBytes = 44;

                if (IntPtr.Zero == mPersistingUnmanagedMemoryForCapDriverCapsResult)
                {
                    mPersistingUnmanagedMemoryForCapDriverCapsResult =
                        System.Runtime.InteropServices.Marshal.AllocHGlobal( sizeOfCAPDRIVERCAPSInBytes );
                }

                int result = 0;
                result = SendMessage( hwnd, WM_CAP_DRIVER_GET_CAPS, sizeOfCAPDRIVERCAPSInBytes, mPersistingUnmanagedMemoryForCapDriverCapsResult.ToInt32( ) );
                if (0 == result)
                {
                    return (false);
                }

                // Copy the raw byte data to a CAPDRIVERCAPS structure
                if (IntPtr.Zero != mPersistingUnmanagedMemoryForCapDriverCapsResult)
                {

                    System.Runtime.InteropServices.Marshal.PtrToStructure
                        (
                        mPersistingUnmanagedMemoryForCapDriverCapsResult,
                        (object)capDriverCaps
                        );
                }
                
                return (true);
            }
            return (false);
        }
        // UNTESTED
        // UNTESTED







        public bool capDlgVideoFormat ( IntPtr hwnd )
        {
            if (true == IsWindow( hwnd ))
            {
                int result = 0;
                result = SendMessage( hwnd, WM_CAP_DLG_VIDEOFORMAT, 0, 0 );
                if (0 == result)
                {
                    return (false);
                }
                return (true);
            }
            return (false);
        }


        public bool capDlgVideoSource ( IntPtr hwnd )
        {
            if (true == IsWindow( hwnd ))
            {
                int result = 0;
                result = SendMessage( hwnd, WM_CAP_DLG_VIDEOSOURCE, 0, 0 );
                if (0 == result)
                {
                    return (false);
                }
                return (true);
            }
            return (false);
        }



        public bool capPreview ( IntPtr hwnd, bool enablePreview )
        {
            if (true == IsWindow( hwnd ))
            {
                int wparam = 0;
                if (true == enablePreview)
                {
                    wparam = 1;
                }

                int result = 0;
                result = SendMessage( hwnd, WM_CAP_SET_PREVIEW, wparam, 0 );
                if (0 == result)
                {
                    return (false);
                }
                return (true);
            }
            return (false);
        }



        public bool capPreviewRate ( IntPtr hwnd, int rateInMilliseconds )
        {
            if (true == IsWindow( hwnd ))
            {
                int result = 0;
                result = SendMessage( hwnd, WM_CAP_SET_PREVIEWRATE, rateInMilliseconds, 0 );
                if (0 == result)
                {
                    return (false);
                }
                return (true);
            }
            return (false);
        }





        public static IntPtr mPersistingUnmanagedMemoryForCapSetVideoFormatBITMAPINFO = IntPtr.Zero;

        public bool capSetVideoFormat ( IntPtr hwnd, BITMAPINFO bitmapinfo, int sizeOfBITMAPINFOIs44Bytes )
        {
            if (true == IsWindow( hwnd ))
            {
                int sizeOfBITMAPINFOInBytes = 44;

                if (IntPtr.Zero == mPersistingUnmanagedMemoryForCapSetVideoFormatBITMAPINFO)
                {
                    mPersistingUnmanagedMemoryForCapSetVideoFormatBITMAPINFO =
                        System.Runtime.InteropServices.Marshal.AllocHGlobal( sizeOfBITMAPINFOInBytes );
                }

                if (IntPtr.Zero != mPersistingUnmanagedMemoryForCapSetVideoFormatBITMAPINFO)
                {

                    System.Runtime.InteropServices.Marshal.StructureToPtr
                        (
                        (object)bitmapinfo,
                        mPersistingUnmanagedMemoryForCapSetVideoFormatBITMAPINFO,
                        false
                        );
                }

                int result = 0;
                result = SendMessage( hwnd, WM_CAP_SET_VIDEOFORMAT, sizeOfBITMAPINFOInBytes, mPersistingUnmanagedMemoryForCapSetVideoFormatBITMAPINFO.ToInt32() );
                if (0 == result)
                {
                    return (false);
                }
                return (true);
            }
            return (false);
        }




        public const int WS_OVERLAPPED       = 0x00000000;
        public const int WS_POPUP            = unchecked((int)0x80000000);
        public const int WS_CHILD            = 0x40000000;
        public const int WS_MINIMIZE         = 0x20000000;
        public const int WS_VISIBLE          = 0x10000000;
        public const int WS_DISABLED         = 0x08000000;
        public const int WS_CLIPSIBLINGS     = 0x04000000;
        public const int WS_CLIPCHILDREN     = 0x02000000;
        public const int WS_MAXIMIZE         = 0x01000000;
        public const int WS_CAPTION          = 0x00C00000; // WS_BORDER | WS_DLGFRAME
        public const int WS_BORDER           = 0x00800000;
        public const int WS_DLGFRAME         = 0x00400000;
        public const int WS_VSCROLL          = 0x00200000;
        public const int WS_HSCROLL          = 0x00100000;
        public const int WS_SYSMENU          = 0x00080000;
        public const int WS_THICKFRAME       = 0x00040000;
        public const int WS_GROUP            = 0x00020000;
        public const int WS_TABSTOP          = 0x00010000;

        public const int WS_MINIMIZEBOX      = 0x00020000;
        public const int WS_MAXIMIZEBOX      = 0x00010000;


        public const int WS_TILED            = WS_OVERLAPPED;
        public const int WS_ICONIC           = WS_MINIMIZE;
        public const int WS_SIZEBOX          = WS_THICKFRAME;
        public const int WS_TILEDWINDOW      = WS_OVERLAPPEDWINDOW;

        public const int WS_OVERLAPPEDWINDOW = 
                                                (
                                                WS_OVERLAPPED     |
                                                WS_CAPTION        |
                                                WS_SYSMENU        |
                                                WS_THICKFRAME     |
                                                WS_MINIMIZEBOX    |
                                                WS_MAXIMIZEBOX
                                                );

        public const int WS_POPUPWINDOW      = (WS_POPUP | WS_BORDER | WS_SYSMENU);

        public const int WS_CHILDWINDOW      = (WS_CHILD);

        public const int WS_EX_DLGMODALFRAME     = 0x00000001;
        public const int WS_EX_NOPARENTNOTIFY    = 0x00000004;
        public const int WS_EX_TOPMOST           = 0x00000008;
        public const int WS_EX_ACCEPTFILES       = 0x00000010;
        public const int WS_EX_TRANSPARENT       = 0x00000020;
        public const int WS_EX_MDICHILD          = 0x00000040;
        public const int WS_EX_TOOLWINDOW        = 0x00000080;
        public const int WS_EX_WINDOWEDGE        = 0x00000100;
        public const int WS_EX_CLIENTEDGE        = 0x00000200;
        public const int WS_EX_CONTEXTHELP       = 0x00000400;

        public const int WS_EX_RIGHT             = 0x00001000;
        public const int WS_EX_LEFT              = 0x00000000;
        public const int WS_EX_RTLREADING        = 0x00002000;
        public const int WS_EX_LTRREADING        = 0x00000000;
        public const int WS_EX_LEFTSCROLLBAR     = 0x00004000;
        public const int WS_EX_RIGHTSCROLLBAR    = 0x00000000;

        public const int WS_EX_CONTROLPARENT     = 0x00010000;
        public const int WS_EX_STATICEDGE        = 0x00020000;
        public const int WS_EX_APPWINDOW         = 0x00040000;


        public const int WS_EX_OVERLAPPEDWINDOW  = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const int WS_EX_PALETTEWINDOW     = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);


        public const int CS_VREDRAW          = 0x0001;
        public const int CS_HREDRAW          = 0x0002;
        public const int CS_DBLCLKS          = 0x0008;
        public const int CS_OWNDC            = 0x0020;
        public const int CS_CLASSDC          = 0x0040;
        public const int CS_PARENTDC         = 0x0080;
        public const int CS_NOCLOSE          = 0x0200;
        public const int CS_SAVEBITS         = 0x0800;
        public const int CS_BYTEALIGNCLIENT  = 0x1000;
        public const int CS_BYTEALIGNWINDOW  = 0x2000;
        public const int CS_GLOBALCLASS      = 0x4000;

        public const int CS_IME              = 0x00010000;






        public enum STVideoCaptureFormat
        {
            BGR320x240 = 0
        }




        // Delegate for client of this video capture class
        public delegate void DelegateClientCaptureCallback 
            ( 
            byte[] bgrData,
            int imageWidth,
            int imageHeight
            );




        private bool mCapDriverConnectStatus; // capDriverConnect()
        private IntPtr mHWNDCapture;
        private STVideoCaptureFormat mSTVideoCaptureFormat;
        private int mCapturePeriodMilliseconds;
        private DelegateClientCaptureCallback mClientCaptureCallback;
        private byte[] mBGRData;



        public int GetCaptureWidth ( )
        {
            int width = 0;
            switch (this.mSTVideoCaptureFormat)
            {
                case STVideoCaptureFormat.BGR320x240: width = 320; break;
            }
            return (width);
        }


        public int GetCaptureHeight ( )
        {
            int height = 0;
            switch (this.mSTVideoCaptureFormat)
            {
                case STVideoCaptureFormat.BGR320x240: height = 240; break;
            }
            return (height);
        }


        public int GetCapturePeriodMilliseconds ( )
        {
            return (this.mCapturePeriodMilliseconds);
        }


        public void Clear ( )
        {
            mCapDriverConnectStatus = false; // capDriverConnect()
            mHWNDCapture = IntPtr.Zero;
            mSTVideoCaptureFormat = STVideoCaptureFormat.BGR320x240;
            mCapturePeriodMilliseconds = 67; // 67 milliseconds --> 15 frames per second maximum
            mClientCaptureCallback = null;
            mBGRData = null;
        }






        public STVideoCapture ( )
        {
            this.Clear( );


            if (false == STVideoCapture.mListOfActiveInstances.Contains( this ))
            {
                STVideoCapture.mListOfActiveInstances.Add( this );
            }
        }






        public int InstanceCaptureCallback
            (
                IntPtr hwnd,
                ref VIDEOHDR videoHdr
            )
        {
            if (null == mClientCaptureCallback)
            {
                return (0);
            }


            // Copy image data to local buffer before calling client
            int captureWidth = 0;
            int captureHeight = 0;
            captureWidth = this.GetCaptureWidth( );
            captureHeight = this.GetCaptureHeight( );


            int sourceStrideBytes = 0;
            int sourceCopyStartByteOffset = 0;
            int destinationStrideBytes = 0;
            int destinationCopyStartByteOffset = 0;
            int bytesToCopyPerLine = 0;
            
            sourceCopyStartByteOffset = 0;
            sourceStrideBytes = (captureWidth * 3);
            destinationCopyStartByteOffset = 0;
            destinationStrideBytes = (captureWidth * 3);
            bytesToCopyPerLine = (captureWidth * 3);



            int lineIndex = 0;
            for (lineIndex = 0; lineIndex < captureHeight; lineIndex++)
            {
                IntPtr sourceLineStartPtr =
                    new IntPtr( videoHdr.lpData.ToInt32( ) + sourceCopyStartByteOffset );

                System.Runtime.InteropServices.Marshal.Copy
                    ( 
                    sourceLineStartPtr,
                    this.mBGRData,
                    destinationCopyStartByteOffset, 
                    bytesToCopyPerLine 
                    );

                sourceCopyStartByteOffset += sourceStrideBytes;
                destinationCopyStartByteOffset += destinationStrideBytes;
            }


            // Call the client
            mClientCaptureCallback
                ( 
                this.mBGRData, 
                this.GetCaptureWidth(), 
                this.GetCaptureHeight() 
                );

            return (0);
        }






        public
        bool
        Initialize
        (
            IntPtr hwndParentWindow, // This must be non-null
            STVideoCaptureFormat videoCaptureFormat,
            int capturePeriodMilliseconds,
            DelegateClientCaptureCallback delegateClientCaptureCallback
        )
        {
            this.Clear( );



            // Validate capture format
            int captureWidth = 0;
            int captureHeight = 0;
            switch (videoCaptureFormat)
            {
                case STVideoCaptureFormat.BGR320x240: { captureWidth = 320; captureHeight = 240; } break;
            }

            if ((0 == captureWidth) || (0 == captureHeight))
            {
                System.Console.WriteLine( "Invalid capture format." );
                this.Clear( );
                return (false);
            }
            this.mSTVideoCaptureFormat = videoCaptureFormat;



            // We create a capture window only because we are required to create
            // such a window to use the Video for Windows (VFW32) API.  We make
            // this window tiny to keep it out of the way of our application.
            // For our purposes, this window is simply a necessary interface to
            // VFW functionality.
            this.mHWNDCapture =
                capCreateCaptureWindow
                (
                    "STVideoCapture Window",
                    (WS_CHILD | WS_VISIBLE | WS_CLIPCHILDREN | WS_CLIPSIBLINGS), // style
                    0, // x
                    0, // y
                    4, // width
                    4, // height
                    hwndParentWindow, // Parent window : This must be non-null
                    0 // nID
                );



            if (IntPtr.Zero == this.mHWNDCapture)
            {
                System.Console.WriteLine( "Failed to create video capture window." );
                DestroyWindow( this.mHWNDCapture );
                this.Clear( );
                return (false);
            }



            this.mCapDriverConnectStatus = capDriverConnect( this.mHWNDCapture, 0 );

            if (false == this.mCapDriverConnectStatus)
            {
                System.Console.WriteLine( "Failed to connect to video driver." );
                DestroyWindow( this.mHWNDCapture );
                this.Clear( );
                return (false);
            }



            // CAPDRIVERCAPS capdrivercaps = new CAPDRIVERCAPS( );
            // NOT FINISHED SUPPORT FOR THIS: capDriverGetCaps( mHWNDCapture, ref capdrivercaps, 44 );



            //capDlgVideoFormat( this.mHWNDCapture );
            //capDlgVideoSource( this.mHWNDCapture ); // Source; Brightness, Contrast, Saturation, Exposure


            
            // Allocate a local buffer to store capture data
            int bytesPerPixel = 3;
            switch (videoCaptureFormat)
            {
                case STVideoCaptureFormat.BGR320x240: { bytesPerPixel = 3; } break;
            }
            int totalImageBytes = 0;
            totalImageBytes = (captureHeight * (captureWidth * bytesPerPixel));
            this.mBGRData = new byte[totalImageBytes];



            // Set the video stream callback function
            this.mClientCaptureCallback = delegateClientCaptureCallback;
            capSetCallbackOnFrame( this.mHWNDCapture, STVideoCapture.mCommonCaptureCallback );


            // Set the preview rate in milliseconds
            this.mCapturePeriodMilliseconds = capturePeriodMilliseconds;
            capPreviewRate( this.mHWNDCapture, capturePeriodMilliseconds );


            // Disable preview mode
            capPreview( this.mHWNDCapture, false );


            // Set the format of the data we wish returned to us
            BITMAPINFO capbitmapinfo = new BITMAPINFO( );
            capbitmapinfo.bmiHeader.biSize = 40; // sizeof( BITMAPINFOHEADER )
            capbitmapinfo.bmiHeader.biWidth = captureWidth;
            capbitmapinfo.bmiHeader.biHeight = captureHeight;
            capbitmapinfo.bmiHeader.biPlanes = 1;
            capbitmapinfo.bmiHeader.biBitCount = 24;
            capbitmapinfo.bmiHeader.biCompression = BI_RGB;
            capbitmapinfo.bmiHeader.biSizeImage = totalImageBytes;
            capbitmapinfo.bmiHeader.biXPelsPerMeter = 100;
            capbitmapinfo.bmiHeader.biYPelsPerMeter = 100;

            bool setVideoFormatResult = false;
            setVideoFormatResult = capSetVideoFormat( this.mHWNDCapture, capbitmapinfo, 44 ); 

            if (false == setVideoFormatResult)
            {
                System.Console.WriteLine( "Failed to set the desired video capture format." );
                capSetCallbackOnFrame( this.mHWNDCapture, null ); // disable the callback function
                DestroyWindow( this.mHWNDCapture );
                this.Clear( );
                return (false);
            }


            return (true);
        }





        public IntPtr GetHWND ( )
        {
            return (this.mHWNDCapture);
        }





        public void Terminate ( )
        {
            if (true == STVideoCapture.mListOfActiveInstances.Contains( this ))
            {
                STVideoCapture.mListOfActiveInstances.Remove( this );
            }

            if (true == this.mCapDriverConnectStatus)
            {
                capSetCallbackOnFrame( this.mHWNDCapture, null );                
            }

            if (IntPtr.Zero != this.mHWNDCapture)
            {
                DestroyWindow( this.mHWNDCapture );
            }

            this.Clear( );
        }



    }
}
