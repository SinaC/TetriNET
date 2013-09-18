using System.Runtime.Serialization;
using TetriNET.Common.Randomizer;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class TetriminoOccurancy : IOccurancy<Tetriminos>
    {
        [DataMember]
        public Tetriminos Value { get; set; }

        [DataMember]
        public int Occurancy { get; set; }
    }
}
