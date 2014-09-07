using System.Runtime.Serialization;

namespace TetriNET2
{
    [DataContract]
    public enum GameFinishedReasons
    {
        [EnumMember]
        Stopped,

        [EnumMember]
        NotEnoughPlayers,

        [EnumMember]
        Won,
    }
}
