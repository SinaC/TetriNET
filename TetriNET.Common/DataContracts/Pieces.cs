using System.Runtime.Serialization;
using TetriNET.Common.Helpers;


namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Pieces
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
        Reserved1 = 8,

        [EnumMember]
        [Availability(false)]
        Reserved2 = 9,

        [EnumMember]
        [Availability(false)]
        Reserved3 = 10,

        [EnumMember]
        [Availability(false)]
        Reserved4 = 11,

        [EnumMember]
        [Availability(false)]
        Reserved5 = 12,

        [EnumMember]
        [Availability(false)]
        Reserved6 = 13,

        [EnumMember]
        [Availability(false)]
        Reserved7 = 14,

        [EnumMember]
        [Availability(false)]
        Reserved8 = 15,

        [EnumMember]
        [Availability(false)]
        PieceLast = 15,
    }
}
