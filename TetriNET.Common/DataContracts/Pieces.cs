using System.Runtime.Serialization;
using TetriNET.Common.Attributes;


namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Pieces
    {
        [EnumMember]
        [Piece(Availabilities.NotAvailable)]
        Invalid = 0,

        //  * * * *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino I")]
        TetriminoI = 1,

        //  * * *
        //      *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino J")]
        TetriminoJ = 2,

        //  * * *
        //  *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino L")]
        TetriminoL = 3,

        //  * *
        //  * *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino O")]
        TetriminoO = 4,

        //   * *
        // * *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino S")]
        TetriminoS = 5,

        //  * * *
        //    *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino T")]
        TetriminoT = 6,

        //  * *
        //    * *
        [EnumMember]
        [Piece(Availabilities.Available, "Tetrimino Z")]
        TetriminoZ = 7,
        
        // Mutated pieces
        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedI = 8,

        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedJ = 9,

        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedL = 10,

        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedO = 11,

        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedS = 12,

        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedT = 13,

        [EnumMember]
        [Piece(Availabilities.Displayable)]
        MutatedZ = 14,

        //
        [EnumMember]
        [Piece(Availabilities.NotAvailable)]
        MaxPieces = 64,
    }
}
