using System.Runtime.Serialization;
using TetriNET.Common.Interfaces;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class SpecialOccurancy : IOccurancy<Specials>
    {
        [DataMember]
        public Specials Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }
}
