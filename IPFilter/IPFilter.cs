using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IPFiltering
{
    public class IPFilter
    {
        private readonly string _name;
        private readonly IList<IPFilterItem> _items;
        private readonly IPFilterTypes _defaultBehavior;

        public IPFilter(string name,IList<IPFilterItem> items, IPFilterTypes defaultBehavior)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("The parameter cannot be null or an empty string.", "name");
            }
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }
            _name = name;
            _items = items;
            _defaultBehavior = defaultBehavior;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the filter items
        /// </summary>
        /// <value>The items.</value>
        public IList<IPFilterItem> Items
        {
            get
            {
                return _items;
            }
        }

        /// <summary>
        /// Gets the behavior when no matches are found.
        /// </summary>
        /// <value>The default behavior.</value>
        public IPFilterTypes DefaultBehavior
        {
            get
            {
                return _defaultBehavior;
            }
        }

        /// <summary>
        /// Checks the address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public IPFilterTypes CheckAddress(IPAddress ipAddress)
        {
            if (ipAddress.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork)
            {
                IPAddress address = GetIPv4Address(ipAddress);
                if (address == null)
                {
                    throw new NotSupportedException("IPv4 supported only.  Address Family : " + ipAddress.AddressFamily.ToString());
                }
                ipAddress = address;
            }
            return CheckAddress((uint)ipAddress.Address);
        }

        /// <summary>
        /// Checks the address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public IPFilterTypes CheckAddress(uint ipAddress)
        {
            foreach (IPFilterItem item in _items)
            {
                IPFilterTypes result = item.CheckAddress(ipAddress);
                if (result != IPFilterTypes.NoMatch)
                {
                    return result;
                }
            }
            return _defaultBehavior;
        }

        /// <summary>
        /// Gets an ip filter that allows all incoming requests.
        /// </summary>
        /// <value></value>
        public static IPFilter Allow
        {
            get
            {
                return new IPFilter("Allow", new IPFilterItem[0], IPFilterTypes.Allow);
            }
        }

        /// <summary>
        /// Gets an ip filter that denies all incoming requests.
        /// </summary>
        /// <value></value>
        public static IPFilter Deny
        {
            get
            {
                return new IPFilter("Deny", new IPFilterItem[0], IPFilterTypes.Deny);
            }
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
