using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace IPFiltering
{
    public class IPFilter
    {
        private string _name;
        private IList<IPFilterItem> _items;
        private IPFilterType _defaultBehavior;

        public IPFilter(string name,IList<IPFilterItem> items, IPFilterType defaultBehavior)
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
        public IPFilterType DefaultBehavior
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
        public IPFilterType CheckAddress(IPAddress ipAddress)
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
        public IPFilterType CheckAddress(uint ipAddress)
        {
            IPFilterType result;
            for (int i = 0; i < _items.Count; i++)
            {
                result = _items[i].CheckAddress(ipAddress);
                if (result != IPFilterType.NoMatch)
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
                return new IPFilter("Allow", new IPFilterItem[0], IPFilterType.Allow);
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
                return new IPFilter("Deny", new IPFilterItem[0], IPFilterType.Deny);
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
