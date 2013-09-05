using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
    [DataContract]
    public enum Tetriminos
    {
        [EnumMember]
        Invalid = 0,

        //  * * * *
        [EnumMember]
        TetriminoI = 1,

        //  * * *
        //      *
        [EnumMember]
        TetriminoJ = 2,

        //  * * *
        //  *
        [EnumMember]
        TetriminoL = 3,

        //  * *
        //  * *
        [EnumMember]
        TetriminoO = 4,

        //   * *
        // * *
        [EnumMember]
        TetriminoS = 5,

        //  * * *
        //    *
        [EnumMember]
        TetriminoT = 6,

        //  * *
        //    * *
        [EnumMember]
        TetriminoZ = 7,
        
        [EnumMember]
        TetriminoReserved1 = 8,

        [EnumMember]
        TetriminoReserved2 = 9,

        [EnumMember]
        TetriminoReserved3 = 10,

        [EnumMember]
        TetriminoReserved4 = 11,

        [EnumMember]
        TetriminoReserved5 = 12,

        [EnumMember]
        TetriminoReserved6 = 13,

        [EnumMember]
        TetriminoReserved7 = 14,

        [EnumMember]
        TetriminoReserved8 = 15,

        [EnumMember]
        TetriminoLast = 15,
    }
}
