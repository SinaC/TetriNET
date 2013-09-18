using System.Runtime.Serialization;
using TetriNET.Common.Helpers;


namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Tetriminos
    {
        [EnumMember]
        [Availability(false)]
        Invalid = 0,

        //  * * * *
        [EnumMember]
        [Availability(true, 'I', "Tetrimino I")]
        TetriminoI = 1,

        //  * * *
        //      *
        [EnumMember]
        [Availability(true, 'J', "Tetrimino J")]
        TetriminoJ = 2,

        //  * * *
        //  *
        [EnumMember]
        [Availability(true, 'L', "Tetrimino L")]
        TetriminoL = 3,

        //  * *
        //  * *
        [EnumMember]
        [Availability(true, 'O', "Tetrimino O")]
        TetriminoO = 4,

        //   * *
        // * *
        [EnumMember]
        [Availability(true, 'S', "Tetrimino S")]
        TetriminoS = 5,

        //  * * *
        //    *
        [EnumMember]
        [Availability(true, 'T', "Tetrimino T")]
        TetriminoT = 6,

        //  * *
        //    * *
        [EnumMember]
        [Availability(true, 'Z', "Tetrimino Z")]
        TetriminoZ = 7,
        
        [EnumMember]
        [Availability(false)]
        TetriminoReserved1 = 8,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved2 = 9,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved3 = 10,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved4 = 11,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved5 = 12,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved6 = 13,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved7 = 14,

        [EnumMember]
        [Availability(false)]
        TetriminoReserved8 = 15,

        [EnumMember]
        [Availability(false)]
        TetriminoLast = 15,
    }
}
