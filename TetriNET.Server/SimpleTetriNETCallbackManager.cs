using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class SimpleTetriNETCallbackManager : ITetriNETCallbackManager
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