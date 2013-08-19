using System;
using TetriNET.Common;

namespace TetriNET.Client
{
    public interface IProxyManager
    {
        IWCFTetriNET CreateProxy(ITetriNETCallback callback, IClient client);
    }
}
