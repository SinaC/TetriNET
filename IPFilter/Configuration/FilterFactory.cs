using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPFiltering.Configuration
{
    public static class FilterFactory
    {
        /// <summary>
        /// Creates the IPFilter
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IPFilter Create(FilterConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
            IList<IPFilterItem> items = configuration.Filters.Select(f=> Create(f)).ToArray();
            return new IPFilter(configuration.Name, items, configuration.DefaultBehavior);
        }

        /// <summary>
        /// Creates a filter item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public static IPFilterItem Create(FilterConfigurationItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (item.FilterTypes == IPFilterTypes.NoMatch)
            {
                throw new ArgumentException("The item has an invalid FilterTypes : '" +
                    item.FilterTypes.ToString() + "'", "item");
            }
            if (string.IsNullOrEmpty(item.Hosts))
            {
                throw new ArgumentException("The item does not have any hosts set.", "item");
            }
            string[] hosts = item.Hosts.Split(',');
            IList<IPRange> ipRanges = hosts.Select((s) => IPRange.Parse(s)).ToArray();
            return new IPFilterItem(ipRanges, item.FilterTypes);
        }
    }
}
