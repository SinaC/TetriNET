using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET2
{
    [DataContract]
    public enum ConnectResults
    {
        [EnumMember]
        [Description("Successful")]
        RegistrationSuccessful,

        [EnumMember]
        [Description("Too many players")]
        RegistrationFailedTooManyPlayers,

        [EnumMember]
        [Description("Already exists")]
        RegistrationFailedPlayerAlreadyExists,

        [EnumMember]
        [Description("Invalid name")]
        RegistrationFailedInvalidName,

        [EnumMember]
        [Description("Invalid id")]
        RegistrationFailedInvalidId,

        [EnumMember]
        [Description("Incompatible version")]
        IncompatibleVersion,
    }
}
