/*************************************************************
 * PortableSettingsProvider.cs
 * Portable Settings Provider for C# applications
 * 
 * 2010- Michael Nathan
 * http://www.Geek-Republic.com
 * 
 * Licensed under Creative Commons CC BY-SA
 * http://creativecommons.org/licenses/by-sa/3.0/legalcode
 * 
 * 
 * 
 * 
 *************************************************************/

//http://www.geek-republic.com/2010/11/c-portable-settings-provider/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.CustomSettings
{
    public class PortableSettingsProvider : SettingsProvider
    {
        // Define some static strings later used in our XML creation
        // XML Root node
        const string XMLRoot = "configuration";

        // Configuration declaration node
        const string ConfigNode = "configSections";

        // Configuration section group declaration node
        const string GroupNode = "sectionGroup";

        // User section node
        const string UserNode = "userSettings";

        // Application Specific Node
        private readonly string _appNode = Assembly.GetExecutingAssembly().GetName().Name + ".Properties.Settings";

        private XmlDocument _xmlDoc;


        public static string SettingsFilename
        {
            get { return "tetrinet.config"; }
        }

        public static string SettingsPath
        {
            get
            {
                Assembly assembly = AssemblyHelper.GetEntryAssembly();
                string companyName = ((AssemblyCompanyAttribute) Attribute.GetCustomAttribute(assembly, typeof (AssemblyCompanyAttribute), false)).Company;
                string productName = ((AssemblyProductAttribute) Attribute.GetCustomAttribute(assembly, typeof (AssemblyProductAttribute), false)).Product;
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appDataPath, companyName, productName);
            }
        }

        // Override the Initialize method
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        // Override the ApplicationName property, returning the solution name.  No need to set anything, we just need to
        // retrieve information, though the set method still needs to be defined.
        public override string ApplicationName
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Name;
            }
            set
            {
                // NOP
            }
        }

        // Simply returns the name of the settings file, which is the solution name plus ".config"
        public virtual string GetSettingsFilename()
        {
            return SettingsFilename;
        }

        // Gets current executable path in order to determine where to read and write the config file
        public virtual string GetSettingsPath()
        {
            return SettingsPath;
        }

        // Retrieve settings from the configuration file
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sContext, SettingsPropertyCollection settingsColl)
        {
            // Create a collection of values to return
            SettingsPropertyValueCollection retValues = new SettingsPropertyValueCollection();

            // Loop through the list of settings that the application has requested and add them
            // to our collection of return values.
            foreach (SettingsProperty sProp in settingsColl)
            {
                SettingsPropertyValue value = GetSetting(sProp);
                if (value != null)
                {
                    value.SerializedValue = value.SerializedValue ?? String.Empty;
                    retValues.Add(value);
                }
            }
            return retValues;
        }

        // Save any of the applications settings that have changed (flagged as "dirty")
        public override void SetPropertyValues(SettingsContext sContext, SettingsPropertyValueCollection settingsColl)
        {
            // Set the values in XML
            foreach (SettingsPropertyValue spVal in settingsColl)
                SetSetting(spVal);

            // Write the XML file to disk
            try
            {
                XMLConfig.Save(Path.Combine(GetSettingsPath(), GetSettingsFilename()));
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Error writing configuration file to disk. Exception: {0}", ex.ToString());
            }
        }

        private XmlDocument XMLConfig
        {
            get
            {
                // Check if we already have accessed the XML config file. If the xmlDoc object is empty, we have not.
                if (_xmlDoc == null)
                {
                    _xmlDoc = new XmlDocument();

                    // If we have not loaded the config, try reading the file from disk.
                    try
                    {
                        _xmlDoc.Load(Path.Combine(GetSettingsPath(), GetSettingsFilename()));
                    }

                    // If the file does not exist on disk, catch the exception then create the XML template for the file.
                    catch (Exception)
                    {
                        // XML Declaration
                        // <?xml version="1.0" encoding="utf-8"?>
                        XmlDeclaration dec = _xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);
                        _xmlDoc.AppendChild(dec);

                        // Create root node and append to the document
                        // <configuration>
                        XmlElement rootNode = _xmlDoc.CreateElement(XMLRoot);
                        _xmlDoc.AppendChild(rootNode);

                        // Create Configuration Sections node and add as the first node under the root
                        // <configSections>
                        XmlElement configNode = _xmlDoc.CreateElement(ConfigNode);
                        _xmlDoc.DocumentElement.PrependChild(configNode);

                        // Create the user settings section group declaration and append to the config node above
                        // <sectionGroup name="userSettings"...>
                        XmlElement groupNode = _xmlDoc.CreateElement(GroupNode);
                        groupNode.SetAttribute("name", UserNode);
                        groupNode.SetAttribute("type", "System.Configuration.UserSettingsGroup");
                        configNode.AppendChild(groupNode);

                        // Create the Application section declaration and append to the groupNode above
                        // <section name="AppName.Properties.Settings"...>
                        XmlElement newSection = _xmlDoc.CreateElement("section");
                        newSection.SetAttribute("name", _appNode);
                        newSection.SetAttribute("type", "System.Configuration.ClientSettingsSection");
                        groupNode.AppendChild(newSection);

                        // Create the userSettings node and append to the root node
                        // <userSettings>
                        XmlElement userNode = _xmlDoc.CreateElement(UserNode);
                        _xmlDoc.DocumentElement.AppendChild(userNode);

                        // Create the Application settings node and append to the userNode above
                        // <AppName.Properties.Settings>
                        XmlElement appNode = _xmlDoc.CreateElement(_appNode);
                        userNode.AppendChild(appNode);
                    }
                }
                return _xmlDoc;
            }
        }

        // Retrieve values from the configuration file, or if the setting does not exist in the file, 
        // retrieve the value from the application's default configuration
        private SettingsPropertyValue GetSetting(SettingsProperty setProp)
        {
            try
            {
                Type t;
                TryFindType(setProp.PropertyType.FullName, out t);

                XmlNode node = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild;
                if (setProp.SerializeAs == SettingsSerializeAs.String)
                {
                    string textData = node.InnerText;
                    return new SettingsPropertyValue(setProp)
                        {
                            Deserialized = true,
                            IsDirty = false,
                            PropertyValue = Convert.ChangeType(textData, t),
                            SerializedValue = textData,
                        };
                }
                else
                {
                    string xmlData = node.InnerXml;
                    if (t == typeof (AchievementsSettings))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof (AchievementsSettings));
                        AchievementsSettings data = (AchievementsSettings) xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
                        return new SettingsPropertyValue(setProp)
                            {
                                Deserialized = true,
                                IsDirty = false,
                                PropertyValue = data,
                                SerializedValue = xmlData
                            };
                    }
                    else if (t == typeof (GameOptions))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof (GameOptions));
                        GameOptions data = (GameOptions) xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
                        return new SettingsPropertyValue(setProp)
                            {
                                Deserialized = true,
                                IsDirty = false,
                                PropertyValue = data,
                                SerializedValue = xmlData
                            };
                    }
                    else if (t == typeof(StringCollection))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(StringCollection));
                        StringCollection data = (StringCollection) xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
                        return new SettingsPropertyValue(setProp)
                            {
                                Deserialized = true,
                                IsDirty = false,
                                PropertyValue = data,
                                SerializedValue = xmlData
                            };
                    }
                }
            }
            catch
            {
                if (setProp.DefaultValue != null)
                {
                    Type t;
                    TryFindType(setProp.PropertyType.FullName, out t);

                    return new SettingsPropertyValue(setProp)
                    {
                        Deserialized = true,
                        IsDirty = false,
                        PropertyValue = Convert.ChangeType(setProp.DefaultValue, t), //setProp.DefaultValue,
                        SerializedValue = setProp.DefaultValue,
                    };
                }
                else
                    return new SettingsPropertyValue(setProp)
                    {
                        Deserialized = true,
                        IsDirty = false,
                        PropertyValue = null,
                        SerializedValue = String.Empty
                    };
            }
            return null;
        }

        private void SetSetting(SettingsPropertyValue setProp)
        {
            // Define the XML path under which we want to write our settings if they do not already exist
            XmlNode settingNode;

            try
            {
                // Search for the specific settings node we want to update.
                // If it exists, return its first child node, (the <value>data here</value> node)
                settingNode = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild;
            }
            catch (Exception)
            {
                settingNode = null;
            }

            // If we have a pointer to an actual XML node, update the value stored there
            if (settingNode != null)
            {
                if (setProp.Property.SerializeAs == SettingsSerializeAs.String)
                    settingNode.InnerText = setProp.SerializedValue.ToString();
                else
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration.
                    settingNode.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", String.Empty);
            }
            else
            {
                // If the value did not already exist in this settings file, create a new entry for this setting

                // Create a new settings node and assign its name as well as how it will be serialized
                XmlElement newSetting = XMLConfig.CreateElement("setting");
                newSetting.SetAttribute("name", setProp.Name);
                newSetting.SetAttribute("serializeAs", setProp.Property.SerializeAs == SettingsSerializeAs.String ? "String" : "Xml");

                // Search for the application settings node (<Appname.Properties.Settings>) and store it.
                XmlNode tmpNode = XMLConfig.SelectSingleNode("//" + _appNode);
                // Append this node to the application settings node (<Appname.Properties.Settings>)
                tmpNode.AppendChild(newSetting);

                // Create an element under our named settings node, and assign it the value we are trying to save
                XmlElement valueElement = XMLConfig.CreateElement("value");
                if (setProp.Property.SerializeAs == SettingsSerializeAs.String)
                    valueElement.InnerText = setProp.SerializedValue.ToString();
                else
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration
                    valueElement.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");

                //Append this new element under the setting node we created above
                newSetting.AppendChild(valueElement);
            }
        }

        private readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();
        private bool TryFindType(string typeName, out Type t)
        {
            lock (_typeCache)
            {
                if (!_typeCache.TryGetValue(typeName, out t))
                {
                    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = a.GetType(typeName);
                        if (t != null)
                            break;
                    }
                    _typeCache[typeName] = t; // perhaps null
                }
            }
            return t != null;
        }
    }
    
    /*
    public class PortableSettingsProvider : SettingsProvider
    {
        private const string NAME = "name";
        private const string SERIALIZE_AS = "serializeAs";
        private const string CONFIG = "configuration";
        private const string USER_SETTINGS = "userSettings";
        private const string SETTING = "setting";

        public static string SettingsFilename
        {
            get { return "tetrinet.config"; }
        }

        public static string SettingsPath
        {
            get
            {
                Assembly assembly = AssemblyHelper.GetEntryAssembly();
                string companyName = ((AssemblyCompanyAttribute) Attribute.GetCustomAttribute(assembly, typeof (AssemblyCompanyAttribute), false)).Company;
                string productName = ((AssemblyProductAttribute) Attribute.GetCustomAttribute(assembly, typeof (AssemblyProductAttribute), false)).Product;
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appDataPath, companyName, productName);
            }
        }

        /// <summary>
        /// Loads the file into memory.
        /// </summary>
        public PortableSettingsProvider()
        {
            SettingsDictionary = new Dictionary<string, SettingStruct>();

        }

        /// <summary>
        /// Override.
        /// </summary>
        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(ApplicationName, config);
        }

        /// <summary>
        /// Override.
        /// </summary>
        public override string ApplicationName
        {
            get { return Assembly.GetExecutingAssembly().ManifestModule.Name; }
            set
            {
                //do nothing
            }
        }

        public object GetPropertyValue(SettingsProperty setting, string serializedValue, Type t)
        {
            if (t == typeof (AchievementsSettings))
            {
                XmlSerializer xs = new XmlSerializer(typeof (AchievementsSettings));
                return xs.Deserialize(new XmlTextReader(serializedValue, XmlNodeType.Element, null));
            }
            else if (t == typeof (GameOptions))
            {
                XmlSerializer xs = new XmlSerializer(typeof (GameOptions));
                return xs.Deserialize(new XmlTextReader(serializedValue, XmlNodeType.Element, null));
            }
            else
                return Convert.ChangeType(serializedValue, t);
        }

        /// <summary>
        /// Must override this, this is the bit that matches up the designer properties to the dictionary values
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        /// <returns></returns>
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection collection)
        {
            //load the file
            if (!_loaded)
            {
                _loaded = true;
                LoadValuesFromFile();
            }

            //collection that will be returned.
            SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();

            //iterate through the properties we get from the designer, checking to see if the setting is in the dictionary
            foreach (SettingsProperty setting in collection)
            {
                SettingsPropertyValue value = new SettingsPropertyValue(setting)
                    {
                        IsDirty = false
                    };
                //need the type of the value for the strong typing
                //Type t = Type.GetType(setting.PropertyType.FullName);
                Type t;
                bool found = TryFindType(setting.PropertyType.FullName, out t);

                if (SettingsDictionary.ContainsKey(setting.Name))
                {
                    value.SerializedValue = SettingsDictionary[setting.Name].value;
                    //value.PropertyValue = Convert.ChangeType(SettingsDictionary[setting.Name].value, t);
                    value.PropertyValue = GetPropertyValue(setting, SettingsDictionary[setting.Name].value, t);
                }
                else //use defaults in the case where there are no settings yet
                {
                    value.SerializedValue = setting.DefaultValue;
                    value.PropertyValue = Convert.ChangeType(setting.DefaultValue, t);
                }

                values.Add(value);
            }
            return values;
        }

        /// <summary>
        /// Must override this, this is the bit that does the saving to file.  Called when Settings.Save() is called
        /// </summary>
        /// <param name="context"></param>
        /// <param name="collection"></param>
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection collection)
        {
            //grab the values from the collection parameter and update the values in our dictionary.
            foreach (SettingsPropertyValue value in collection)
            {
                var setting = new SettingStruct
                    {
                        //value = (value.PropertyValue == null ? String.Empty : value.PropertyValue.ToString()),
                        value = value.SerializedValue == null ? String.Empty : value.SerializedValue.ToString(),
                        name = value.Name,
                        serializeAs = value.Property.SerializeAs.ToString()
                    };

                if (!SettingsDictionary.ContainsKey(value.Name))
                {
                    SettingsDictionary.Add(value.Name, setting);
                }
                else
                {
                    SettingsDictionary[value.Name] = setting;
                }
            }

            //now that our local dictionary is up-to-date, save it to disk.
            SaveValuesToFile();
        }

        /// <summary>
        /// Loads the values of the file into memory.
        /// </summary>
        private void LoadValuesFromFile()
        {
            if (!File.Exists(UserConfigPath))
            {
                //if the config file is not where it's supposed to be create a new one.
                CreateEmptyConfig();
            }

            //load the xml
            var configXml = XDocument.Load(UserConfigPath);

            //get all of the <setting name="..." serializeAs="..."> elements.
            var settingElements = configXml.Element(CONFIG).Element(USER_SETTINGS).Element(typeof (Properties.Settings).FullName).Elements(SETTING);

            //iterate through, adding them to the dictionary, (checking for nulls, xml no likey nulls)
            //using "String" as default serializeAs...just in case, no real good reason.
            foreach (var element in settingElements)
            {
                var nameAttribute = element.Attribute(NAME);
                var serializeAsAttribute = element.Attribute(SERIALIZE_AS);
                var valueElement = element.Element("value");
                var newSetting = new SettingStruct
                    {
                        name = nameAttribute == null ? String.Empty : nameAttribute.Value,
                        serializeAs = serializeAsAttribute == null ? "String" : serializeAsAttribute.Value,
                        value = valueElement == null ? (element.Value ?? String.Empty) : valueElement.Nodes().Aggregate("", (b, node) => b + node.ToString())
                    };
                SettingsDictionary.Add(nameAttribute.Value, newSetting);
            }
        }

        /// <summary>
        /// Creates an empty user.config file...looks like the one MS creates.  
        /// This could be overkill a simple key/value pairing would probably do.
        /// </summary>
        private void CreateEmptyConfig()
        {
            var doc = new XDocument();
            var declaration = new XDeclaration("1.0", "utf-8", "true");
            var config = new XElement(CONFIG);
            var userSettings = new XElement(USER_SETTINGS);
            var group = new XElement(typeof (Properties.Settings).FullName);
            userSettings.Add(group);
            config.Add(userSettings);
            doc.Add(config);
            doc.Declaration = declaration;
            doc.Save(UserConfigPath);
        }

        /// <summary>
        /// Saves the in memory dictionary to the user config file
        /// </summary>
        private void SaveValuesToFile()
        {
            //load the current xml from the file.
            var import = XDocument.Load(UserConfigPath);

            //get the settings group (e.g. <Company.Project.Desktop.Settings>)
            var settingsSection = import.Element(CONFIG).Element(USER_SETTINGS).Element(typeof (Properties.Settings).FullName);

            //iterate though the dictionary, either updating the value or adding the new setting.
            foreach (var entry in SettingsDictionary)
            {
                var setting = settingsSection.Elements().FirstOrDefault(e => e.Attribute(NAME).Value == entry.Key);
                if (setting == null) //this can happen if a new setting is added via the .settings designer.
                {
                    var newSetting = new XElement(SETTING);
                    newSetting.Add(new XAttribute(NAME, entry.Value.name));
                    newSetting.Add(new XAttribute(SERIALIZE_AS, entry.Value.serializeAs));
                    newSetting.Value = (entry.Value.value ?? String.Empty);
                }
                else //update the value if it exists.
                {
                    //setting.Value = (entry.Value.value ?? String.Empty);
                    setting.SetElementValue("value", entry.Value.value);
                }
            }
            import.Save(UserConfigPath);
        }

        /// <summary>
        /// The setting key this is returning must set before the settings are used.
        /// e.g. <c>Properties.Settings.Default.SettingsKey = @"C:\temp\user.config";</c>
        /// </summary>
        private string UserConfigPath
        {
            get
            {
                //return Properties.Settings.Default.SettingsKey;
                return Path.Combine(SettingsPath, SettingsFilename);
            }

        }

        /// <summary>
        /// In memory storage of the settings values
        /// </summary>
        private Dictionary<string, SettingStruct> SettingsDictionary { get; set; }

        /// <summary>
        /// Helper struct.
        /// </summary>
        internal struct SettingStruct
        {
            internal string name;
            internal string serializeAs;
            internal string value;
        }

        private bool _loaded;
        private readonly Dictionary<string, Type> typeCache = new Dictionary<string, Type>();
        private bool TryFindType(string typeName, out Type t)
        {
            lock (typeCache)
            {
                if (!typeCache.TryGetValue(typeName, out t))
                {
                    foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = a.GetType(typeName);
                        if (t != null)
                            break;
                    }
                    typeCache[typeName] = t; // perhaps null
                }
            }
            return t != null;
        }
    }
    */
}