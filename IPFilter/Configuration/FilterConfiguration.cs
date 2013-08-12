using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml;

namespace IPFiltering.Configuration
{

    public class FilterConfiguration : ConfigurationElement
    {
        private const string _allowElementName = "allow";
        private const string _denyElementName = "deny";
        private const string _hostAttributeName = "hosts";
        private const string _filterName = "Name";
        private const string _defaultBehaviorAttribName = "DefaultBehavior";
        private IList<FilterConfigurationItem> _filterItems = null;

        /// <summary>
        /// Gets the name of the filter.
        /// </summary>
        /// <value>The name.</value>
        [ConfigurationProperty(_filterName, IsRequired = true)]
        public string Name
        {
            get
            {
                return this[_filterName] as string;
            }
        }

        /// <summary>
        /// Gets the default behavior when no match is found.
        /// </summary>
        /// <value>The default behavior.</value>
        [ConfigurationProperty(_defaultBehaviorAttribName, IsRequired = false, DefaultValue = IPFilterTypes.Allow)]
        public IPFilterTypes DefaultBehavior
        {
            get
            {
                return (IPFilterTypes)this[_defaultBehaviorAttribName];
            }
        }

        /// <summary>
        /// Gets the filters.
        /// </summary>
        /// <value>The filters.</value>
        public IList<FilterConfigurationItem> Filters
        {
            get
            {
                return _filterItems;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether an unknown element is encountered during deserialization.
        /// </summary>
        /// <param name="elementName">The name of the unknown subelement.</param>
        /// <param name="reader">The <see cref="T:System.Xml.XmlReader"/> being used for deserialization.</param>
        /// <returns>
        /// true when an unknown element is encountered while deserializing; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.Configuration.ConfigurationErrorsException">The element identified by <paramref name="elementName"/> is locked.- or -One or more of the element's attributes is locked.- or -<paramref name="elementName"/> is unrecognized, or the element has an unrecognized attribute.- or -The element has a Boolean attribute with an invalid value.- or -An attempt was made to deserialize a property more than once.- or -An attempt was made to deserialize a property that is not a valid member of the element.- or -The element cannot contain a CDATA or text element.</exception>
        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {

            IPFilterTypes filterTypes = GetFilterResult(elementName);
            if (filterTypes == IPFilterTypes.NoMatch)
            {
                return base.OnDeserializeUnrecognizedElement(elementName, reader);
            }
            string host = null;
            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == _hostAttributeName)
                {
                    host = reader.Value;
                }
                else
                {
                    throw new ConfigurationErrorsException("Unknown attribute " + reader.Name);
                }
            }
            if (host == null)
            {
                throw new ConfigurationErrorsException("Host attribute not found.");
            }
            _filterItems.Add(new FilterConfigurationItem() { FilterTypes = filterTypes, Hosts = host });

            return true;
            
        }
        /// <summary>
        /// Sets the <see cref="T:System.Configuration.ConfigurationElement"/> object to its initial state.
        /// </summary>
        protected override void Init()
        {
            base.Init();
            _filterItems = new List<FilterConfigurationItem>();
        }

        private static IPFilterTypes GetFilterResult(string  value)
        {
            if (value == _allowElementName)
            {
                return IPFilterTypes.Allow;
            }
            else if (value == _denyElementName)
            {
                return IPFilterTypes.Deny;
            }
            return IPFilterTypes.NoMatch;
        }


    }
}
