using TetriNET.Common;

namespace TetriNET.Client
{
    public interface IProxyManager
    {
        ITetriNET CreateProxy(ITetriNETCallback callback, IClient client);
    }
}
