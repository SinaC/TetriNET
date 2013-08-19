using TetriNET.Common.Contracts;

namespace TetriNET.Client
{
    public interface IProxyManager
    {
        IWCFTetriNET CreateProxy(ITetriNETCallback callback, IClient client);
    }
}
