using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET2
{
    [DataContract]
    public class GameDescription
    {
        [DataMember]
        Guid Id { get; set; }

        [DataMember]
        string Name { get; set; }

        [DataMember]
        List<string> Clients { get; set; }
    }
}
