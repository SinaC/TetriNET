﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.BanManager
{
    // TODO: read/write in file
    public sealed class BanManager : IBanManager
    {
        private sealed class BanEntry
        {
            public string Name { get; }
            public IPAddress Address { get; }
            public BanReasons Reason { get; }

            public BanEntry(string name, IPAddress address, BanReasons reason)
            {
                Name = name;
                Address = address;
                Reason = reason;
            }
        }

        private readonly Dictionary<IPAddress, BanEntry> _banList = new Dictionary<IPAddress, BanEntry>();

        public BanManager()
        {
            // TODO: read from file
            //Ban("joel", IPAddress.Parse("127.0.0.1"), BanReasons.Ban);
        }

        public void Ban(string name, IPAddress address, BanReasons reason)
        {
            address = FixAddress(address);

            if (_banList.ContainsKey(address))
                return;
            BanEntry banEntry = new BanEntry(name, address, reason);
            _banList.Add(address, banEntry);
            // TODO: save in file
        }

        public bool IsBanned(IPAddress address)
        {
            address = FixAddress(address);

            return _banList.ContainsKey(address);
        }

        public void Dump()
        {
            foreach (BanEntry entry in _banList.Values)
                Log.Default.WriteLine(LogLevels.Info, "{0} {1} {2}", entry.Address, entry.Name, entry.Reason);
        }

        private static IPAddress FixAddress(IPAddress address)
        {
            if (address.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                IPAddress addressIPV4 = GetIPv4Address(address);
                if (addressIPV4 != null)
                    address = addressIPV4;
                else
                    Log.Default.WriteLine(LogLevels.Error, "IPv4 supported only. {0} Address family: {1}", address, address.AddressFamily);
            }
            return address;
        }

        private static IPAddress GetIPv4Address(IPAddress address)
        {
            if (IPAddress.IPv6Loopback.Equals(address))
            {
                return new IPAddress(0x0100007F);
            }
            IPAddress[] addresses = Dns.GetHostAddresses(address.ToString());
            return addresses.FirstOrDefault(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
