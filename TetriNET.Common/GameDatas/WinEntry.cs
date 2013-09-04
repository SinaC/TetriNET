using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
    [DataContract]
    public class WinEntry
    {
        [DataMember]
        public string PlayerName { get; set; }

        [DataMember]
        public int Score { get; set; }
    }
}
