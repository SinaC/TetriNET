using System.Runtime.Serialization;
using TetriNET.Common.Interfaces;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class PieceOccurancy : IOccurancy<Pieces>
    {
        [DataMember]
        public Pieces Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }
}
