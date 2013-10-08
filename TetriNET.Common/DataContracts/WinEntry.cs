using System.Runtime.Serialization;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class WinEntry
    {
        [DataMember]
        public string PlayerName { get; set; }

        [DataMember(IsRequired=false)]
        public string Team { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
