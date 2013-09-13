// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Text;
using System.IO;



namespace CPF.StandardTetris
{
    public class STFileWriter : IDisposable
    {
        private FileStream mFileStream;
        private StreamWriter mStreamWriter;



        // See pp.202-203 of Professional C# 2005 (Wrox) 
        //   on IDisposable and destructor

        // A pattern ensure the calling of Dispose() is:
        //
        //  using ( MyClass x = new MyClass() )
        //  {
        //      // use the instance
        //  }



        private bool unmanagedResourcesDisposed = false;



        public void Dispose ( )
        {
            Dispose( true ); // true == dispose of managed resources (in addition to unmanaged resources)
            GC.SuppressFinalize( this ); // suppress future calling of destructor method
        }



        protected virtual void Dispose ( bool disposeOfManagedResourcesInAdditionToUnmanagedResources )
        {
            if (false == unmanagedResourcesDisposed)
            {
                if (true == disposeOfManagedResourcesInAdditionToUnmanagedResources)
                {
                    // dispose of managed objects by calling
                    // their Dispose() methods.
                    if (null != this.mStreamWriter)
                    {
                        try
                        {
                            // Close() calls Dispose(true) on file
                            this.mStreamWriter.Close( );
                        }
                        catch
                        {
                        }

                        this.mStreamWriter = null;
                    }
                }

                // dispose of un-managed objects
                // (...nothing to do here...)
            }

            unmanagedResourcesDisposed = true;
        }



        ~STFileWriter ( )
        {
            // only dispose of un-managed resources, and mark
            // the object as disposed
            Dispose( false );  // false == do not dispose of managed resources
        }



        public STFileWriter ( )
        {
        }



        public bool Open ( String filePathAndName )
        {
            if (null == filePathAndName)
            {
                return (false);
            }

            if (filePathAndName.Length <= 0)
            {
                return (false);
            }

            try
            {
                this.mFileStream =
                    new FileStream
                    (
                        filePathAndName,
                        FileMode.OpenOrCreate,
                        FileAccess.Write,
                        FileShare.Read | FileShare.Delete
                    );
            }
            catch
            {
                this.mFileStream = null;
                return(false);
            }

            try
            {
                this.mStreamWriter =
                    new StreamWriter( this.mFileStream, Encoding.ASCII );
            }
            catch
            {
                this.mFileStream = null;
                this.mStreamWriter = null;
                return(false);
            }

            return (true);
        }



        public void Close ( )
        {
            if (null != this.mStreamWriter)
            {
                try
                {
                    // Close() calls Dispose(true) on file
                    this.mStreamWriter.Close( );
                }
                catch
                {
                }

                this.mStreamWriter = null;
            }
        }



        public void WriteText ( String text )
        {
            if (null != this.mStreamWriter)
            {
                this.mStreamWriter.Write( text );                
            }
        }


    }
}
