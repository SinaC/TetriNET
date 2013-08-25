using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET.Common
{
    [DataContract]
    public enum Tetriminos
    {
        //  * * *
        //      *
        [EnumMember]
        TetriminoJ,

        //  * *
        //    * *
        [EnumMember]
        TetriminoZ,

        //  * *
        //  * *
        [EnumMember]
        TetriminoO,

        //  * * *
        //  *
        [EnumMember]
        TetriminoL,

        //   * *
        // * *
        [EnumMember]
        TetriminoS,

        //  * * *
        //    *
        [EnumMember]
        TetriminoT,

        //  * * * *
        [EnumMember]
        TetriminoI,
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
        AddLines,
        [EnumMember]
        ClearLines,
        [EnumMember]
        NukeField,
        [EnumMember]
        RandomBlocksClear,
        [EnumMember]
        SwitchFields,
        [EnumMember]
        ClearSpecialBlocks,
        [EnumMember]
        BlockGravity,
        [EnumMember]
        BlockQuake,
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
        public List<int> TetriminoProbabilities { get; set; } // number of entries must match Tetriminos enum length

        [DataMember]
        public List<int> SpecialProbabilities { get; set; } // number of entries must match Specials enum length

        [DataMember]
        public bool ClassicStyleMultiplayerRules { get; set; }

        [DataMember]
        public int StartingLevel { get; set; }

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
