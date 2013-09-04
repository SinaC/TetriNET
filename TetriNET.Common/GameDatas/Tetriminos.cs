using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
    [DataContract]
    public enum Tetriminos  // Max 15 values
    {
        [EnumMember]
        Invalid = 0,

        //  * * *
        //      *
        [EnumMember]
        TetriminoJ = 1,

        //  * *
        //    * *
        [EnumMember]
        TetriminoZ = 2,

        //  * *
        //  * *
        [EnumMember]
        TetriminoO = 3,

        //  * * *
        //  *
        [EnumMember]
        TetriminoL = 4,

        //   * *
        // * *
        [EnumMember]
        TetriminoS = 5,

        //  * * *
        //    *
        [EnumMember]
        TetriminoT = 6,

        //  * * * *
        [EnumMember]
        TetriminoI = 7,
    }
}
