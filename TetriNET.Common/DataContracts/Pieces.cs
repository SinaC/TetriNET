using System.Runtime.Serialization;
using TetriNET.Common.Attributes;


namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Pieces
    {
        [EnumMember]
        [Piece(false)]
        Invalid = 0,

        //  * * * *
        [EnumMember]
        [Piece(true, "Tetrimino I")]
        TetriminoI = 1,

        //  * * *
        //      *
        [EnumMember]
        [Piece(true, "Tetrimino J")]
        TetriminoJ = 2,

        //  * * *
        //  *
        [EnumMember]
        [Piece(true, "Tetrimino L")]
        TetriminoL = 3,

        //  * *
        //  * *
        [EnumMember]
        [Piece(true, "Tetrimino O")]
        TetriminoO = 4,

        //   * *
        // * *
        [EnumMember]
        [Piece(true, "Tetrimino S")]
        TetriminoS = 5,

        //  * * *
        //    *
        [EnumMember]
        [Piece(true, "Tetrimino T")]
        TetriminoT = 6,

        //  * *
        //    * *
        [EnumMember]
        [Piece(true, "Tetrimino Z")]
        TetriminoZ = 7,

        //
        [EnumMember]
        [Piece(false)]
        MaxPieces = 64,
    }
}
