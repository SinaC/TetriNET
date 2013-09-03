using System.Collections.Generic;
using System.Runtime.Serialization;
using TetriNET.Common.Contracts;
using TetriNET.Common.Randomizer;

namespace TetriNET.Common
{
    [DataContract]
    public enum Tetriminos
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

    [DataContract]
    public enum Specials
    {
        [EnumMember]
        Invalid = 0,
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
    public class TetriminoOccurancy : IOccurancy<Tetriminos>
    {
        [DataMember]
        public Tetriminos Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }

    [DataContract]
    public class SpecialOccurancy : IOccurancy<Specials>
    {
        [DataMember]
        public Specials Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }

    [DataContract]
    public class GameOptions
    {
        [DataMember]
        public List<TetriminoOccurancy> TetriminoOccurancies { get; set; } // in %, number of entries must match Tetriminos enum length

        [DataMember]
        public List<SpecialOccurancy> SpecialOccurancies { get; set; } // in %, number of entries must match Specials enum length

        [DataMember]
        public bool ClassicStyleMultiplayerRules { get; set; } // if true, lines are send to other players when collapsing multiple lines (2->1, 3->2, Tetris->4)

        [DataMember]
        public int StartingLevel { get; set; }

        [DataMember]
        public int InventorySize { get; set; }

        [DataMember]
        public int LinesToMakeForSpecials { get; set; }

        [DataMember]
        public int SpecialsAddedEachTime { get; set; }

        [DataMember]
        public int DelayBeforeSuddenDeath { get; set; } // in minutes, 0 means no sudden death

        [DataMember]
        public int SuddenDeathTick { get; set; } // in seconds
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
