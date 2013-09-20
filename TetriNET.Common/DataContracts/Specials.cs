using System.Runtime.Serialization;
using TetriNET.Common.Helpers;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Specials // Specials index start after last piece
    {
        [EnumMember]
        [Availability(false)]
        Invalid = 0,

        // TetriNET (http://en.wikipedia.org/wiki/TetriNET)
        [EnumMember]
        [Availability(true, 'A', "Add Line")]
        AddLines = Pieces.PieceLast+1,
        [EnumMember]
        [Availability(true, 'C', "Clear Line")]
        ClearLines,
        [EnumMember]
        [Availability(true, 'N', "Nuke Field")]
        NukeField,
        [EnumMember]
        [Availability(true, 'R', "Random Blocks Clear")]
        RandomBlocksClear,
        [EnumMember]
        [Availability(true, 'S', "Switch Fields")]
        SwitchFields,
        [EnumMember]
        [Availability(true, 'B', "Clear Special Blocks")]
        ClearSpecialBlocks,
        [EnumMember]
        [Availability(true, 'G', "Block Gravity")]
        BlockGravity,
        [EnumMember]
        [Availability(true, 'Q', "Block Quake")]
        BlockQuake,
        [EnumMember]
        [Availability(true, 'O', "Block Bomb")]
        BlockBomb,

        // TetriNET 2 (http://harddrop.com/wiki/Tetrinet2 or http://en.wikipedia.org/wiki/TetriNET or http://web.archive.org/web/20070623140748/www.tetrinet2.com/?page=overview_specials)
        [EnumMember]
        [Availability(true, 'V', "Clear Column")]
        ClearColumn,
        [EnumMember]
        [Availability(true, 'D', "Darkness")]
        Darkness,
        [EnumMember]
        [Availability(true, 'F', "Confusion")]
        Confusion,
        // NOT IMPLEMENTED Immunity
        // NOT IMPLEMENTED Mutate Pieces

        // Blocktrix (http://en.wikipedia.org/wiki/TetriNET)
        //[EnumMember]
        //ZebraField, // will be available when Left Gravity is implemented
        // NOT IMPLEMENTED Piece Change
        // NOT IMPLEMENTED Left Gravity

        // BlockBattle (http://blockbattle.net/tutorial)
        // NOT IMPLEMENTED Multiplier
        // NOT IMPLEMENTED Pause
        // NOT IMPLEMENTED Night (same as darkness)
    }
}
