using System.Collections.Generic;
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
    public enum Specials
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
    public class Position
    {
        [DataMember]
        public int X;

        [DataMember]
        public int Y;
    }

    [DataContract]
    public class GameOptions
    {
        [DataMember]
        public List<int> TetriminoProbabilities { get; set; } // number of entries should match Tetriminos enum length

        [DataMember]
        public List<int> SpecialProbabilities { get; set; } // number of entries should match Specials enum length
    }

    [DataContract]
    public enum LeaveReasons
    {
        [EnumMember]
        Disconnected, // player called Unregister
        [EnumMember]
        ConnectionLost, // exception while calling player callback
        [EnumMember]
        Timeout, // timeout
        [EnumMember]
        Kick, // kicked by server master
        [EnumMember]
        Ban, // banned by server master
        [EnumMember]
        Spam // kicked by host because of spam
    }

    [DataContract]
    public class WinEntry
    {
        [DataMember]
        public string PlayerName { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
