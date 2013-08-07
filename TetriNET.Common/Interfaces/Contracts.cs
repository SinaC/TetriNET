using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace TetriNET.Common.Interfaces
{
    [ServiceContract]
    public interface ITetriNET
    {
        [OperationContract]
        void RegisterPlayer(string playerName);
    }

    [ServiceContract]
    public interface ITetriNETCallback
    {
        [OperationContract]
        void OnPlayerRegistered(bool succeeded, int playerId);
    }
}
