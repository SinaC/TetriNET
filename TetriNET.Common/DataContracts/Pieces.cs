using System.Runtime.Serialization;
using TetriNET.Common.Helpers;


namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Pieces
    {
        [EnumMember]
        [Availability(Availabilities.NotAvailable)]
        Invalid = 0,

        //  * * * *
        [EnumMember]
        [Availability(Availabilities.Available, 'I', "Tetrimino I")]
        TetriminoI = 1,

        //  * * *
        //      *
        [EnumMember]
        [Availability(Availabilities.Available, 'J', "Tetrimino J")]
        TetriminoJ = 2,

        //  * * *
        //  *
        [EnumMember]
        [Availability(Availabilities.Available, 'L', "Tetrimino L")]
        TetriminoL = 3,

        //  * *
        //  * *
        [EnumMember]
        [Availability(Availabilities.Available, 'O', "Tetrimino O")]
        TetriminoO = 4,

        //   * *
        // * *
        [EnumMember]
        [Availability(Availabilities.Available, 'S', "Tetrimino S")]
        TetriminoS = 5,

        //  * * *
        //    *
        [EnumMember]
        [Availability(Availabilities.Available, 'T', "Tetrimino T")]
        TetriminoT = 6,

        //  * *
        //    * *
        [EnumMember]
        [Availability(Availabilities.Available, 'Z', "Tetrimino Z")]
        TetriminoZ = 7,
        
        // Mutated pieces
        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoI)]
        MutatedI = 8,

        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoJ)]
        MutatedJ = 9,

        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoL)]
        MutatedL = 10,

        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoO)]
        MutatedO = 11,

        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoS)]
        MutatedS = 12,

        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoT)]
        MutatedT = 13,

        [EnumMember]
        [Availability(Availabilities.Mutation, TetriminoZ)]
        MutatedZ = 14,

        //
        [EnumMember]
        [Availability(Availabilities.NotAvailable)]
        MaxPieces = 64,
    }
}
