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

    [DataContract]
    public enum Specials
    {
        [EnumMember]
        AddLines = 1,
        [EnumMember]
        ClearLines = 2,
        [EnumMember]
        NukeField = 3,
        [EnumMember]
        RandomBlocksClear = 4,
        [EnumMember]
        SwitchFields = 5,
        [EnumMember]
        ClearSpecialBlocks = 6,
        [EnumMember]
        BlockGravity = 7,
        [EnumMember]
        BlockQuake = 8,
        [EnumMember]
        BlockBomb = 9
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
