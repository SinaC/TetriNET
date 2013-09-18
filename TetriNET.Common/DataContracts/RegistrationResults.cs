using System.ComponentModel;
using System.Runtime.Serialization;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public enum RegistrationResults
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
        RegistrationFailedInvalidId
    }
}
