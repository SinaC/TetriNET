using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace IPFiltering
{
    public class IPFilterItem
    {
        
        private IList<IPRange> _ranges;
        private IPFilterType _result;


        /// <summary>
        /// Initializes a new instance of the <see cref="IPFilterItem"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="ipranges">The ipranges.</param>
        /// <param name="result">The result.</param>
        public IPFilterItem( IList<IPRange> ipranges, IPFilterType result)
        {
            if (ipranges == null)
            {
                throw new ArgumentNullException("ipranges");
            }
            _result = result;
            _ranges = ipranges;
        }


        /// <summary>
        /// Gets the type of the filter.
        /// </summary>
        /// <value>The type of the filter.</value>
        public IPFilterType FilterType
        {
            get {
                return _result;
            }
        }

        /// <summary>
        /// Gets the IPRange items.
        /// </summary>
        /// <value>The items.</value>
        public IList<IPRange> Items
        {
            get
            {
                return _ranges;
            }
        }


        /// <summary>
        /// Checks the address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public IPFilterType CheckAddress(IPAddress ipAddress)
        {
            return CheckAddress((uint)ipAddress.Address);
        }
        /// <summary>
        /// Checks the address.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns></returns>
        public IPFilterType CheckAddress(uint ipAddress)
        {
            for (int i = 0; i < _ranges.Count; i++)
            {
                if (_ranges[i].IsMatch(ipAddress))
                {
                    return _result;
                }
            }
            return IPFilterType.NoMatch;
        }
    }
}
