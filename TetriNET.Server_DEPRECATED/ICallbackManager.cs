using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public interface ICallbackManager
    {
        ITetriNETCallback Callback { get; }
        string Endpoint { get; }
    }
}
