using System.Net;

namespace TetriNET.Server.Interfaces
{
    public enum BanReasons
    {
        Ban,    // server master banned player
        Spam,   // server banned automatically because of spam
    }

    public interface IBanManager
    {
        bool IsBanned(IPAddress address);
        void Ban(string name, IPAddress address, BanReasons reason);
    }
}
