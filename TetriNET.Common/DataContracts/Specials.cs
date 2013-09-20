using System.Runtime.Serialization;
using TetriNET.Common.Helpers;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum Specials // Specials index start after last piece
    {
        [EnumMember]
        [Availability(Availabilities.NotAvailable)]
        Invalid = 0,

        // TetriNET (http://en.wikipedia.org/wiki/TetriNET)
        [EnumMember]
        [Availability(Availabilities.Available, 'A', "Add Line")]
        AddLines = Pieces.MaxPieces + 1,
        [EnumMember]
        [Availability(Availabilities.Available, 'C', "Clear Line")]
        ClearLines,
        [EnumMember]
        [Availability(Availabilities.Available, 'N', "Nuke Field")]
        NukeField,
        [EnumMember]
        [Availability(Availabilities.Available, 'R', "Random Blocks Clear")]
        RandomBlocksClear,
        [EnumMember]
        [Availability(Availabilities.Available, 'S', "Switch Fields")]
        SwitchFields,
        [EnumMember]
        [Availability(Availabilities.Available, 'B', "Clear Special Blocks")]
        ClearSpecialBlocks,
        [EnumMember]
        [Availability(Availabilities.Available, 'G', "Block Gravity")]
        BlockGravity,
        [EnumMember]
        [Availability(Availabilities.Available, 'Q', "Block Quake")]
        BlockQuake,
        [EnumMember]
        [Availability(Availabilities.Available, 'O', "Block Bomb")]
        BlockBomb,

        // TetriNET 2 (http://harddrop.com/wiki/Tetrinet2 or http://en.wikipedia.org/wiki/TetriNET or http://web.archive.org/web/20070623140748/www.tetrinet2.com/?page=overview_specials)
        [EnumMember]
        [Availability(Availabilities.Available, 'V', "Clear Column")]
        ClearColumn,
        [EnumMember]
        [Availability(Availabilities.Available, 'D', "Darkness")]
        Darkness,
        [EnumMember]
        [Availability(Availabilities.Available, 'F', "Confusion")]
        Confusion,
        // NOT IMPLEMENTED Immunity
        // NOT IMPLEMENTED Mutate Pieces

        // Blocktrix (http://en.wikipedia.org/wiki/TetriNET)
        [EnumMember]
        [Availability(Availabilities.NotAvailable)]
        ZebraField, // will be available when Left Gravity is implemented
        // NOT IMPLEMENTED Piece Change
        [EnumMember]
        [Availability(Availabilities.NotAvailable)]
        LeftGravity, // not yet implemented

        // BlockBattle (http://blockbattle.net/tutorial)
        // NOT IMPLEMENTED Multiplier
        // NOT IMPLEMENTED Pause
    }
}
