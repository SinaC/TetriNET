using System.Runtime.Serialization;

namespace TetriNET2
{
    [DataContract]
    public enum GameJoinFailReasons
    {
        [EnumMember]
        WrongPassword,
        
        [EnumMember]
        TooManyPlayers,

        [EnumMember]
        TooManySpectators,
    }
}
