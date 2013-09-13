// All contents of this file written by Colin Fahey ( http://colinfahey.com )
// 2007 June 4 ; Visit web site to check for any updates to this file.


using System.Collections.Generic;


namespace CPF.StandardTetris
{
    public class STPieceSequence
    {


        public enum STPieceSelectionSource
        {
            Random = 0,
            AlternatingSAndZ = 1,
            Queue = 2
            // Queue can accept piece shapes from video capture, network, etc.
            // Queue can accept piece shapes from a file.
            // Queue can accept piece shapes selected by an adversarial
            //   "worst-piece-possible" AI (like "birdtris") to
            //   maximize the challenge to the player/AI.
        };

        private STPieceSelectionSource mPieceSelectionSource = STPieceSelectionSource.Random;

        private STPiece.STPieceShape mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.None;
        private STPiece.STPieceShape mCachedSelectedPieceShapeNext = STPiece.STPieceShape.None;


        // state information for the different piece-sequence sources

        private long mCachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator = 0;
        private STRandom mSTRandom = new STRandom();
        private bool mAlternatingSZState;
        private List<int> mQueue = new List<int>( );




        public STPieceSequence ( )
        {
        }




        private void PrivateAdvanceRandom ( )
        {
            int pieceShapeIndexCurrent = 0;
            pieceShapeIndexCurrent = 
                this.mSTRandom.GetIntegerInRangeUsingCurrentState( 1, 7 );

            this.mSTRandom.Advance( );

            int pieceShapeIndexNext = 0;
            pieceShapeIndexNext =
                this.mSTRandom.GetIntegerInRangeUsingCurrentState( 1, 7 );

            // Set current and next piece shapes
            this.mCachedSelectedPieceShapeCurrent =
              STPiece.GetShapeCorrespondingToByteCode( (byte)pieceShapeIndexCurrent );

            this.mCachedSelectedPieceShapeNext =
              STPiece.GetShapeCorrespondingToByteCode( (byte)pieceShapeIndexNext );
        }



        private void PrivateAdvanceAlternatingSZ ( )
        {
            if (false == this.mAlternatingSZState)
            {
                this.mAlternatingSZState = true;
                this.mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.S;
                this.mCachedSelectedPieceShapeNext = STPiece.STPieceShape.Z;
            }
            else
            {
                this.mAlternatingSZState = false;
                this.mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.Z;
                this.mCachedSelectedPieceShapeNext = STPiece.STPieceShape.S;
            }
        }





        // UNUSED:
        //private bool PrivateQueueIsEmpty ( )
        //{
        //    if (this.mQueue.Count <= 0)
        //    {
        //        return (true);
        //    }
        //    return (false);
        //}

        private int PrivateQueuePeekItem ( )
        {
            if (this.mQueue.Count <= 0)
            {
                return (0);
            }
            return (this.mQueue[0]);
        }

        // UNUSED:
        //private int PrivateQueueGetItem ( )
        //{
        //    if (this.mQueue.Count <= 0)
        //    {
        //        return (0);
        //    }
        //    int value = (this.mQueue[0]);
        //    this.mQueue.RemoveAt( 0 );
        //    return (value);
        //}

        private void PrivateAdvanceQueue ( )
        {
            this.mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.None;
            this.mCachedSelectedPieceShapeNext = STPiece.STPieceShape.None;

            if (this.mQueue.Count <= 0)
            {
                return;
            }

            // We definitely have one shape in the queue
            int pieceShapeIndexCurrent = 0;
            pieceShapeIndexCurrent = this.PrivateQueuePeekItem( );

            // Remove the shape from the queue
            this.mQueue.RemoveAt( 0 );

            // There might be another shape in the queue.  We'll peek.
            int pieceShapeIndexNext = 0;
            pieceShapeIndexNext = this.PrivateQueuePeekItem( );


            // Set current and next piece shapes
            this.mCachedSelectedPieceShapeCurrent =
              STPiece.GetShapeCorrespondingToByteCode( (byte)pieceShapeIndexCurrent );

            this.mCachedSelectedPieceShapeNext =
              STPiece.GetShapeCorrespondingToByteCode( (byte)pieceShapeIndexNext );
        }






        // CLIENT OPERATIONS


        // STPiece.STPieceShape.None means piece information unavailable
        public STPiece.STPieceShape ClientPeekSelectedPieceCurrent( )
        {
          return( this.mCachedSelectedPieceShapeCurrent );
        }


        // STPiece.STPieceShape.None means piece information unavailable
        public STPiece.STPieceShape ClientPeekSelectedPieceNext( )
        {
          return( this.mCachedSelectedPieceShapeNext );
        }



        // The following method is called by the client (i.e., game engine) to
        // request an update of the current and next pieces.  It is possible 
        // that the next piece and possibly the current piece become UNAVAILABLE.
        // It is the client's responsibility to check for pieces to become 
        // available in the future.
        // Sources of piece sequences that are external to this application, such as
        // video capture and network packets, are asynchronous and essentially 
        // non-deterministic (i.e., algorithm and parameters are essentially unknown).
        // The client must wait for new pieces to become available.
        // In any case, the following method should be called whenever the client
        // has consumed the current piece.

        public void ClientRequestSelectionUpdate ( )
        {
            this.mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.None;
            this.mCachedSelectedPieceShapeNext = STPiece.STPieceShape.None;

            switch(this.mPieceSelectionSource)
            {
                case STPieceSelectionSource.Random:
                    {
                        this.PrivateAdvanceRandom( );
                    }
                    break;

                case STPieceSelectionSource.AlternatingSAndZ:
                    {
                        this.PrivateAdvanceAlternatingSZ( );
                    }
                    break;

                case STPieceSelectionSource.Queue:
                    {
                        this.PrivateAdvanceQueue( );
                    }
                    break;

                default:
                    {
                    }
                    break;
            }
        }








        // The following method selects the source to use for piece spawning.
        public void ClientSelectPieceSelectionSource
          (
          STPieceSelectionSource pieceSelectionSource
          )
        {
            this.mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.None;
            this.mCachedSelectedPieceShapeNext = STPiece.STPieceShape.None;

            switch (pieceSelectionSource)
            {
                case STPieceSelectionSource.Random:
                    {
                        this.mPieceSelectionSource = pieceSelectionSource;
                    }
                    break;

                case STPieceSelectionSource.AlternatingSAndZ:
                    {
                        this.mPieceSelectionSource = pieceSelectionSource;
                    }
                    break;

                case STPieceSelectionSource.Queue:
                    {
                        this.mPieceSelectionSource = pieceSelectionSource;
                    }
                    break;

                default:
                    {
                        this.mPieceSelectionSource = STPieceSelectionSource.Random;
                    }
                    break;
            }
        }



        public STPieceSelectionSource ClientCheckPieceSelectionSource ( )
        {
            return (this.mPieceSelectionSource);
        }




        // Call the following before each new game, using seed value that is
        // as random as possible.  Calling this method with the same seed value
        // should start the deterministic generators in the same initial state.

        public void ClientRequestSelectionGeneratorReset
          (
            long seedValue
          )
        {
            // Cache seed value
            this.mCachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator = seedValue;
            this.mSTRandom.SetState(seedValue);

            // Clear cached selections
            this.mCachedSelectedPieceShapeCurrent = STPiece.STPieceShape.None;
            this.mCachedSelectedPieceShapeNext = STPiece.STPieceShape.None;

            // FIFO : Simply clear the FIFO.
            this.mQueue.Clear( );

            // S/Z alternating sequence : set phase
            this.mAlternatingSZState = false;
            if (0 != (seedValue % 2))
            {
                this.mAlternatingSZState = true;
            }
        }






        // SERVER OPERATIONS

        // These methods are only used when the source of the piece sequence is
        // external to this application, such as video capture or network packets.
        // Such external sources will be regarded as remote piece servers, and this
        // class will be a proxy.
        // The following methods are used to accept piece sequence data from the
        // external sources.  Any received data will be cached, so that the client
        // can consume the data asynchronously at its own rate.

        public void ServerQueueReset ( )
        {
            this.mQueue.Clear( );
        }

        public void ServerQueueSubmitPiece ( STPiece.STPieceShape shape )
        {
            int pieceIndex = 0;
            pieceIndex = (int) STPiece.GetByteCodeValueOfShape( shape );
            this.mQueue.Add( pieceIndex );
        }




        // DIRECT METHODS

        // Such methods query/modify properties of this object directly and
        // without processing of any kind; suitable for writing state to 
        // persisting storage, or reading state from storage.




        public STPieceSelectionSource DirectGetPieceSelectionSource ( ) 
        { 
            return (this.mPieceSelectionSource); 
        }

        public void DirectSetPieceSelectionSource ( STPieceSelectionSource pieceSelectionSource )
        {
            this.mPieceSelectionSource = pieceSelectionSource;
        }



        public STPiece.STPieceShape DirectGetCurrentPieceShape ( ) 
        { 
            return (this.mCachedSelectedPieceShapeCurrent); 
        }

        public void DirectSetCurrentPieceShape ( STPiece.STPieceShape currentPieceShape )
        {
            this.mCachedSelectedPieceShapeCurrent = currentPieceShape;
        }



        public STPiece.STPieceShape DirectGetNextPieceShape ( ) 
        { 
            return (this.mCachedSelectedPieceShapeNext); 
        }

        public void DirectSetNextPieceShape ( STPiece.STPieceShape nextPieceShape )
        {
            this.mCachedSelectedPieceShapeNext = nextPieceShape;
        }



        public bool DirectGetAlternateSZState ( ) 
        { 
            return (this.mAlternatingSZState); 
        }

        public void DirectSetAlternateSZState ( bool alternateSZState )
        {
            this.mAlternatingSZState = alternateSZState;
        }




        public int DirectGetTotalQueueElements ( ) 
        { 
            return (this.mQueue.Count); 
        }

        public int DirectGetQueueElementByIndex ( int index )
        {
            if ((index >= 0) && (index < this.mQueue.Count))
            {
                return (this.mQueue[index]);
            }
            return (0);
        }


        public void DirectQueueClear ( )
        {
            this.mQueue.Clear( );
        }

        public void DirectAddQueueElement ( int value )
        {
            this.mQueue.Add( value );
        }





        public long DirectGetSeedUsedMostRecentlyToInitializeRandomNumberGenerator ( )
        {
            return (this.mCachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator);
        }

        public void DirectSetSeedUsedMostRecentlyToInitializeRandomNumberGenerator ( long cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator )
        {
            this.mCachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator = cachedRandomSeedUsedMostRecentlyToInitializeRandomGenerator;
        }



        public long DirectGetCurrentRandomNumberGeneratorInternalStateValue ( )
        {
            return (this.mSTRandom.GetState( ));
        }

        public void DirectSetCurrentRandomNumberGeneratorInternalStateValue ( long randomNumberGeneratorInternalState )
        {
            this.mSTRandom.SetState( randomNumberGeneratorInternalState );
        }


    }
}
