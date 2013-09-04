using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
    [DataContract]
    public enum Specials // Max 15 values
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
        BlockBomb = 9,
        [EnumMember]
        ClearColumn = 10,
    }
}
