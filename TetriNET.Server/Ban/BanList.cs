using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace TetriNET.Server.Ban
{
    public enum BanReasons
    {
        Ban,    // server master banned player
        Spam,   // server banned automatically because of spam
    }

    public sealed class BanEntry
    {
        public string Name { get; private set; }
        public IPAddress Address { get; private set; }
        public BanReasons Reason { get; private set; }

        public BanEntry(string name, IPAddress address, BanReasons reason)
        {
            Name = name;
            Address = address;
            Reason = reason;
        }
    }

    // TODO: 
    //  transform in singleton + thread safety
    //  read/write in file
    public sealed class BanList
    {
        private readonly Dictionary<IPAddress, BanEntry> _banList = new Dictionary<IPAddress, BanEntry>();

        public void Ban(string name, IPAddress address, BanReasons reason)
        {
            if (!_banList.ContainsKey(address))
            {
                BanEntry banEntry = new BanEntry(name, address, reason);
                _banList.Add(address, banEntry);
            }
            // TODO: save in file
        }

        public bool IsBanned(IPAddress address)
        {
            return _banList.ContainsKey(address);
        }
    }
}
