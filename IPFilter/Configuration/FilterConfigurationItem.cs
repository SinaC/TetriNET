using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace IPFiltering.Configuration
{
    public class FilterConfigurationItem
    {
        /// <summary>
        /// Gets or sets the type of the filter.
        /// </summary>
        /// <value>The type of the filter.</value>
        public IPFilterTypes FilterTypes { get; set; }
        /// <summary>
        /// Gets a comma demlimited list of hosts
        /// </summary>
        /// <value>The hosts.</value>
        public string Hosts { get; set; }
    }
}
