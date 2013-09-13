// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.



using System;
using System.Collections.Generic;
using System.IO;



namespace CPF.StandardTetris
{



    public class STFileListItem
    {
        public String mFileName;
        public String mFilePathAndName;
        public long mFileSizeInBytes;

        public static int Compare ( Object a, Object b )
        {
            STFileListItem fa = (a as STFileListItem);
            STFileListItem fb = (b as STFileListItem);

            String sa = fa.mFileName;
            String sb = fb.mFileName;

            if ((null != sa) && (null != sb))
            {
                return(String.Compare( sa, sb ));
            }
            return (0);
        }


        public STFileListItem ( )
        {
            mFileName = "";
            mFilePathAndName = "";
            mFileSizeInBytes = 0;
        }
    }



    public class STFileList
    {
        private String mPath;
        private List<STFileListItem> mListSTFileListItem;

        private void PrivateClearEntries ( )
        {
            this.mListSTFileListItem.Clear( );
        }

        private void PrivateSortEntries ( )
        {
            // NOTE: I need to provide a comparison function for this!
            // The default comparer is definitely not what I want.
            this.mListSTFileListItem.Sort( STFileListItem.Compare );
        }


        public String GetDirectoryPath ( )
        { 
            return (mPath); 
        }

        public void ScanDirectory ( String path )  // "C:", not "C:\"
        {
            this.mPath = path;

            this.PrivateClearEntries( );

            // Find all files named "tetris_state_*.txt" in the directory
            // of the given path.

            if (true == Directory.Exists( path ))
            {
                String[] filePathAndNameList = Directory.GetFiles( path, "tetris_state_*.txt", SearchOption.TopDirectoryOnly );
                foreach (String filePathAndName in filePathAndNameList)
                {
                    if (true == File.Exists( filePathAndName ))
                    {
                        long totalBytesInFile = 0;
                        String fileName = "";

                        FileInfo fi = new FileInfo( filePathAndName );
                        totalBytesInFile = fi.Length;
                        fileName = fi.Name;

                        STFileListItem item = new STFileListItem( );
                        item.mFileName = fileName;
                        item.mFilePathAndName = filePathAndName;
                        item.mFileSizeInBytes = totalBytesInFile;

                        mListSTFileListItem.Add( item );
                    }
                }
            }

            this.PrivateSortEntries( );
        }

        public int GetTotalItems ( )
        {
            return (this.mListSTFileListItem.Count);
        }

        public String GetItemNameByIndex ( int index )
        {
            if ((index < 0) || (index >= this.mListSTFileListItem.Count))
            {
                return ("");
            }
            return (this.mListSTFileListItem[index].mFileName);
        }

        public String GetItemFullPathAndNameByIndex ( int index )
        {
            if ((index < 0) || (index >= this.mListSTFileListItem.Count))
            {
                return ("");
            }
            return (this.mListSTFileListItem[index].mFilePathAndName);
        }

        public STFileList ( )
        {
            mPath = "";
            mListSTFileListItem = new List<STFileListItem>( );
        }
    }
}
