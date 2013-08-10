using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace IPFiltering.Configuration
{
    public class FilterConfigurationCollection : ConfigurationElementCollection
    {

        /// <summary>
        /// Gets the <see cref="IPFilter.Configuration.FilterConfiguration"/> at the specified index.
        /// </summary>
        /// <value></value>
        public FilterConfiguration this[int index]
        {
            get
            {
                return this.BaseGet(index) as FilterConfiguration;
            }
        }

        /// <summary>
        /// Gets the <see cref="IPFilter.Configuration.FilterConfiguration"/> with the specified name.
        /// </summary>
        /// <value></value>
        public FilterConfiguration this[string name]
        {
            get
            {
                return this.BaseGet(name) as FilterConfiguration;
            }
        }

        /// <summary>
        /// Gets the type of the <see cref="T:System.Configuration.ConfigurationElementCollection"/>.
        /// </summary>
        /// <value></value>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationElementCollectionType"/> of this collection.</returns>
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.AddRemoveClearMap;
            }
        }


        

        /// <summary>
        /// When overridden in a derived class, creates a new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new FilterConfiguration();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element when overridden in a derived class.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement"/> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"/> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"/>.
        /// </returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as FilterConfiguration).Name;
        }
    }
}
