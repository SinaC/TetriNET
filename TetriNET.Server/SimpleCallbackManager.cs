using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class SimpleCallbackManager : ICallbackManager
    {
        public ITetriNETCallback Callback
        {
            get
            {
                return OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();
            }
        }
    }
}