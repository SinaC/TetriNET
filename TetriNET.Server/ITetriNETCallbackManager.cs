using TetriNET.Common;

namespace TetriNET.Server
{
    public interface ITetriNETCallbackManager
    {
        ITetriNETCallback Callback { get; }
    }
}
