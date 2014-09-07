using System.Runtime.Serialization;

namespace TetriNET2
{
    [DataContract]
    public enum LeaveReasons
    {
        [EnumMember]
        Disconnected, // client called Unregister
        [EnumMember]
        ConnectionLost, // exception while calling client callback
        [EnumMember]
        Timeout, // timeout
        [EnumMember]
        Kick, // vote kicked
        [EnumMember]
        Ban, // banned by server
        [EnumMember]
        Spam // kicked by host because of spam
    }
}
