using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
    [DataContract]
    public enum Specials // Specials index start after last tetrimino
    {
        [EnumMember]
        Invalid = 0,

        // TetriNET (http://en.wikipedia.org/wiki/TetriNET)
        [EnumMember]
        AddLines = Tetriminos.TetriminoLast+1,
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
        BlockBomb,

        // TetriNET 2 (http://harddrop.com/wiki/Tetrinet2 or http://en.wikipedia.org/wiki/TetriNET or http://web.archive.org/web/20070623140748/www.tetrinet2.com/?page=overview_specials)
        [EnumMember]
        ClearColumn,
        [EnumMember]
        Darkness,
        [EnumMember]
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
