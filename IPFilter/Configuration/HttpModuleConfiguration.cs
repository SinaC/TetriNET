using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace IPFiltering.Configuration
{
    public class HttpModuleConfiguration : ConfigurationElement
    {
        private const string _filterNameAttribName = "FilterName";

        /// <summary>
        /// The name of the filter the http module should use
        /// </summary>
        [ConfigurationProperty(_filterNameAttribName, IsRequired = true)]
        public string FilterName
        {
            get
            {
                return this[_filterNameAttribName] as string;
            }
        }
    }
}
