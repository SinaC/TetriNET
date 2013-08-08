using System.Runtime.Serialization;

namespace TetriNET.Common
{
    [DataContract]
    public enum Tetriminos
    {
        // (*)* * *
        [EnumMember]
        TetriminoI,

        // (*)* *
        //      *
        [EnumMember]
        TetriminoJ,

        // (*)* *
        //  *
        [EnumMember]
        TetriminoL,

        // (*)*
        //  * *
        [EnumMember]
        TetriminoO,

        // ()* *
        // * *
        [EnumMember]
        TetriminoS,

        // (*)* *
        //    *
        [EnumMember]
        TetriminoT,

        // (*)*
        //    * *
        [EnumMember]
        TetriminoZ,
    }

    [DataContract]
    public enum Orientations
    {
        [EnumMember]
        Top, // start position
        [EnumMember]
        Left, // 90° clockwise
        [EnumMember]
        Bottom, // 180°
        [EnumMember]
        Right // 90° counter-clockwise
    }

    [DataContract]
    public enum Attacks
    {
        [EnumMember]
        AddLine,
        [EnumMember]
        ClearLine,
        [EnumMember]
        Nuke,
        [EnumMember]
        Switch,
        [EnumMember]
        Gravity,
        [EnumMember]
        ClearSpecialBlocks,
        [EnumMember]
        RandomBlocksClear,
        [EnumMember]
        BlockBomb
    }

    [DataContract]
    public struct Position
    {
        [DataMember]
        public int X;
        [DataMember]
        public int Y;
    }
}
