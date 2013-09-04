using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
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
}
