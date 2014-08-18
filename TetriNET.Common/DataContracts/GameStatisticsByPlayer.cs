using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class GameStatisticsByPlayer
    {
        [DataMember]
        // Player Id
        public int PlayerId { get; set; }

        // Lines count (received from client when game is finished)
        [DataMember]
        public int SingleCount { get; set; }
        [DataMember]
        public int DoubleCount { get; set; }
        [DataMember]
        public int TripleCount { get; set; }
        [DataMember]
        public int TetrisCount { get; set; }

        // Playing time in seconds
        [DataMember]
        public double PlayingTime { get; set; }

        // Number of specials used on each players <Specials, <PlayerId, Count>>
        [DataMember]
        public Dictionary<Specials, Dictionary<int, int>> SpecialsUsed { get; set; }
    }
}
