using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class GameStatistics
    {
        [DataMember]
        public double MatchTime { get; set; } // in seconds

        [DataMember]
        public List<GameStatisticsByPlayer> Players { get; set; }
    }
}
