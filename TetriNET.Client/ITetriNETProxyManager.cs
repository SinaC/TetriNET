using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetriNET.Common;

namespace TetriNET.Client
{
    public interface ITetriNETProxyManager
    {
        ITetriNET CreateProxy(ITetriNETCallback callback, IClient client);
    }
}
