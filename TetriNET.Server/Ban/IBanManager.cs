using System.Net;

namespace TetriNET.Server.Ban
{
    public enum BanReasons
    {
        Ban,    // server master banned player
        Spam,   // server banned automatically because of spam
    }

    // TODO: should use more than IPAddress
    public interface IBanManager
    {
        bool IsBanned(IPAddress address);
        void Ban(string name, IPAddress address, BanReasons reason);
    }
}
