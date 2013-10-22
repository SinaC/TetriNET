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
        const string XMLROOT = "configuration";

        // Configuration declaration node
        const string CONFIGNODE = "configSections";

        // Configuration section group declaration node
        const string GROUPNODE = "sectionGroup";

        // User section node
        const string USERNODE = "userSettings";

        // Application Specific Node
        readonly string APPNODE = Assembly.GetExecutingAssembly().GetName().Name + ".Properties.Settings";

        private XmlDocument _xmlDoc = null;


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
                return (Assembly.GetExecutingAssembly().GetName().Name);
            }
            set
            {
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

            //// Create a temporary SettingsPropertyValue to reuse
            //SettingsPropertyValue setVal;

            // Loop through the list of settings that the application has requested and add them
            // to our collection of return values.
            foreach (SettingsProperty sProp in settingsColl)
            {
                //setVal = new SettingsPropertyValue(sProp);
                //setVal.IsDirty = false;
                //setVal.SerializedValue = GetSetting(sProp);
                //setVal.PropertyValue = setVal.SerializedValue;
                //retValues.Add(setVal);
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
            {
                SetSetting(spVal);
            }

            // Write the XML file to disk
            try
            {
                XMLConfig.Save(Path.Combine(GetSettingsPath(), GetSettingsFilename()));
            }
            catch (Exception ex)
            {
                // Create an informational message for the user if we cannot save the settings.
                // Enable whichever applies to your application type.

                // Uncomment the following line to enable a MessageBox for forms-based apps
                Log.WriteLine(Log.LogLevels.Error, "Error writting configuration file to disk. Exception: {0}", ex.ToString());

                // Uncomment the following line to enable a console message for console-based apps
                //Console.WriteLine("Error writing configuration file to disk: " + ex.Message);
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
                        XmlElement rootNode = _xmlDoc.CreateElement(XMLROOT);
                        _xmlDoc.AppendChild(rootNode);

                        // Create Configuration Sections node and add as the first node under the root
                        // <configSections>
                        XmlElement configNode = _xmlDoc.CreateElement(CONFIGNODE);
                        _xmlDoc.DocumentElement.PrependChild(configNode);

                        // Create the user settings section group declaration and append to the config node above
                        // <sectionGroup name="userSettings"...>
                        XmlElement groupNode = _xmlDoc.CreateElement(GROUPNODE);
                        groupNode.SetAttribute("name", USERNODE);
                        groupNode.SetAttribute("type", "System.Configuration.UserSettingsGroup");
                        configNode.AppendChild(groupNode);

                        // Create the Application section declaration and append to the groupNode above
                        // <section name="AppName.Properties.Settings"...>
                        XmlElement newSection = _xmlDoc.CreateElement("section");
                        newSection.SetAttribute("name", APPNODE);
                        newSection.SetAttribute("type", "System.Configuration.ClientSettingsSection");
                        groupNode.AppendChild(newSection);

                        // Create the userSettings node and append to the root node
                        // <userSettings>
                        XmlElement userNode = _xmlDoc.CreateElement(USERNODE);
                        _xmlDoc.DocumentElement.AppendChild(userNode);

                        // Create the Application settings node and append to the userNode above
                        // <AppName.Properties.Settings>
                        XmlElement appNode = _xmlDoc.CreateElement(APPNODE);
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
                string settingType = setProp.PropertyType.ToString();
                var t = Type.GetType(setProp.PropertyType.FullName);
                if (setProp.SerializeAs.ToString() == "String")
                {
                    string textData = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild.InnerText;
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
                    string xmlData = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild.InnerXml;
                    switch (settingType)
                    {
                        case "TetriNET.WPF_WCF_Client.CustomSettings.AchievementsSettings":
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
                        case "TetriNET.Common.DataContracts.GameOptions":
                            {
                                XmlSerializer xs = new XmlSerializer(typeof(GameOptions));
                                GameOptions data = (GameOptions)xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
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
            }
            catch(Exception ex)
            {
                if (setProp.DefaultValue != null)
                {
                    var t = Type.GetType(setProp.PropertyType.FullName);

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
                        SerializedValue = ""
                    };
            }
            return null;
        }

        private void SetSetting(SettingsPropertyValue setProp)
        {
            // Define the XML path under which we want to write our settings if they do not already exist
            XmlNode settingNode = null;

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
                if (setProp.Property.SerializeAs.ToString() == "String")
                    settingNode.InnerText = setProp.SerializedValue.ToString();
                else
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration.
                    settingNode.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");
            }
            else
            {
                // If the value did not already exist in this settings file, create a new entry for this setting

                // Search for the application settings node (<Appname.Properties.Settings>) and store it.
                XmlNode tmpNode = XMLConfig.SelectSingleNode("//" + APPNODE);

                // Create a new settings node and assign its name as well as how it will be serialized
                XmlElement newSetting = _xmlDoc.CreateElement("setting");
                newSetting.SetAttribute("name", setProp.Name);
                newSetting.SetAttribute("serializeAs", setProp.Property.SerializeAs.ToString() == "String" ? "String" : "Xml");
                // Append this node to the application settings node (<Appname.Properties.Settings>)
                tmpNode.AppendChild(newSetting);

                // Create an element under our named settings node, and assign it the value we are trying to save
                XmlElement valueElement = _xmlDoc.CreateElement("value");
                if (setProp.Property.SerializeAs.ToString() == "String")
                    valueElement.InnerText = setProp.SerializedValue.ToString();
                else
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration
                    valueElement.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");

                //Append this new element under the setting node we created above
                newSetting.AppendChild(valueElement);
            }
        }
    }
}
