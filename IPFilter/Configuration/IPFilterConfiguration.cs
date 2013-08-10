using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace IPFiltering.Configuration
{
    public class IPFilterConfiguration : ConfigurationSection
    {
        private const string _defaultSectionName = "IPFilter";
        private const string _httpModuleSectionName = "HttpModule";
        private const string _filtersSectionName = "Filters";
        private static readonly ThreadSafeSingleton<IPFilterConfiguration> _instance = new ThreadSafeSingleton<IPFilterConfiguration>(() => ConfigurationManager.GetSection(_defaultSectionName) as IPFilterConfiguration);
        
        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static IPFilterConfiguration Default
        {
            get
            {
                return _instance.Value;
            }
        }

        /// <summary>
        /// Gets the HTTP module config.
        /// </summary>
        /// <value>The HTTP module config.</value>
        [ConfigurationProperty(_httpModuleSectionName, IsRequired=false)]
        public HttpModuleConfiguration HttpModuleConfig
        {
            get
            {
                return (this[_httpModuleSectionName] as HttpModuleConfiguration) ?? new HttpModuleConfiguration();                    
            }
        }

        /// <summary>
        /// Gets the filter collection.
        /// </summary>
        /// <value>The filters.</value>
        [ConfigurationProperty(_filtersSectionName, IsRequired = false)]
        public FilterConfigurationCollection Filters
        {
            get
            {
                return (this[_filtersSectionName] as FilterConfigurationCollection) ?? new FilterConfigurationCollection();
            }
        }
    }
}
