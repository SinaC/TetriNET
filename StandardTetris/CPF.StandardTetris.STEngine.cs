// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Windows.Forms;



namespace CPF.StandardTetris
{
    public static class STEngine
    {
        private static STForm mSTForm;

        private static STVideoProcessing mSTVideoProcessing = new STVideoProcessing( );

        private static STConsole mSTConsole = new STConsole( );

        private static STGame mSTGame = new STGame( );

        private static STFileList mSTFileList = new STFileList( );



        public static STVideoProcessing GetVideoProcessing ( )
        {
            return (mSTVideoProcessing);
        }

        public static STConsole GetConsole ( )
        {
            return (mSTConsole);
        }

        public static STForm GetMainForm ( )
        {
            return (mSTForm);
        }

        public static STFileList GetFileList ( )
        {
            return (mSTFileList);
        }

        public static STGame GetGame ( )
        {
            return (mSTGame);
        }

        public static String GetApplicationPath()
        {
            String path = "";

            // Find out the executing assembly information
            System.Reflection.Assembly executingAssembly =
              System.Reflection.Assembly.GetExecutingAssembly( );
            // UNUSED: string exeName = System.IO.Path.GetFileNameWithoutExtension( executingAssembly.Location );
            string exeFolder = System.IO.Path.GetDirectoryName( executingAssembly.Location );

            path = exeFolder;
            path = path.TrimEnd( new char[] { '\\' } );

            return (path);
        }

        public static void Start ( )
        {
            mSTGame.SeedPieceSequenceGeneratorWithCurrentTime( );

            mSTForm = new STForm( );
            Application.Run( mSTForm );
        }
    }
}
