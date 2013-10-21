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
            get { return "user.config"; }
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
            base.Initialize(this.ApplicationName, config);
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
                return;
            }
        }

        // Simply returns the name of the settings file, which is the solution name plus ".config"
        public virtual string GetSettingsFilename()
        {
            //return ApplicationName + ".config";
            //return "user.config";
            return SettingsFilename;
        }

        // Gets current executable path in order to determine where to read and write the config file
        public virtual string GetSettingsPath()
        {
            ////return new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            //Assembly assembly = AssemblyHelper.GetEntryAssembly();
            //string companyName = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false)).Company;
            //string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute), false)).Product;
            //string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //return Path.Combine(appDataPath, companyName, productName);
            return SettingsPath;
        }

        //public virtual string GetAppFilename()
        //{
        //    Assembly assembly = AssemblyHelper.GetEntryAssembly();
        //    string companyName = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute), false)).Company;
        //    string productName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute), false)).Product;
        //    string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //    return Path.Combine(appDataPath, companyName, productName, "user.config");
        //}

        // Retrieve settings from the configuration file
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext sContext, SettingsPropertyCollection settingsColl)
        {
            // Create a collection of values to return
            SettingsPropertyValueCollection retValues = new SettingsPropertyValueCollection();

            // Create a temporary SettingsPropertyValue to reuse
            SettingsPropertyValue setVal;

            // Loop through the list of settings that the application has requested and add them
            // to our collection of return values.
            foreach (SettingsProperty sProp in settingsColl)
            {
                //setVal = new SettingsPropertyValue(sProp);
                //setVal.IsDirty = false;
                //setVal.SerializedValue = GetSetting(sProp);
                //setVal.PropertyValue = setVal.SerializedValue;
                //retValues.Add(setVal);
                SettingsPropertyValue value = GetSetting2(sProp);
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
                Log.WriteLine(Log.LogLevels.Error, "Error writting configuration file to disk");

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
                        _xmlDoc.Load(System.IO.Path.Combine(GetSettingsPath(), GetSettingsFilename()));
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

        private SettingsPropertyValue GetSetting2(SettingsProperty setProp)
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
                                XmlSerializer xs = new XmlSerializer(typeof(Common.DataContracts.GameOptions));
                                Common.DataContracts.GameOptions data = (Common.DataContracts.GameOptions)xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
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
                            PropertyValue = Convert.ChangeType(setProp.DefaultValue, t),//setProp.DefaultValue,
                            SerializedValue = setProp.DefaultValue,
                        };
                }
            }
            return null;
        }

        // Retrieve values from the configuration file, or if the setting does not exist in the file, 
        // retrieve the value from the application's default configuration
        private object GetSetting(SettingsProperty setProp)
        {
            object retVal;
            try
            {
                // Search for the specific settings node we are looking for in the configuration file.
                // If it exists, return the InnerText or InnerXML of its first child node, depending on the setting type.

                // If the setting is serialized as a string, return the text stored in the config
                if (setProp.SerializeAs.ToString() == "String")
                {
                    return XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild.InnerText;
                }

                // If the setting is stored as XML, deserialize it and return the proper object.  This only supports
                // StringCollections at the moment - I will likely add other types as I use them in applications.
                else
                {
                    string settingType = setProp.PropertyType.ToString();
                    string xmlData = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild.InnerXml;
                    switch (settingType)
                    {
                        case "System.Collections.Specialized.StringCollection":
                            {
                                XmlSerializer xs = new XmlSerializer(typeof(string[]));
                                string[] data = (string[])xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
                                StringCollection sc = new StringCollection();
                                sc.AddRange(data);
                                return sc;
                            }
                        case "TetriNET.WPF_WCF_Client.CustomSettings.AchievementsSettings":
                            {
                                XmlSerializer xs = new XmlSerializer(typeof(TetriNET.WPF_WCF_Client.CustomSettings.AchievementsSettings));
                                TetriNET.WPF_WCF_Client.CustomSettings.AchievementsSettings data = (TetriNET.WPF_WCF_Client.CustomSettings.AchievementsSettings)xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));
                                return data;
                            }
                            break;
                        default:
                            return "";
                    }
                }
            }
            catch (Exception)
            {
                // Check to see if a default value is defined by the application.
                // If so, return that value, using the same rules for settings stored as Strings and XML as above
                if ((setProp.DefaultValue != null))
                {
                    if (setProp.SerializeAs.ToString() == "String")
                    {
                        retVal = setProp.DefaultValue.ToString();
                    }
                    else
                    {
                        string settingType = setProp.PropertyType.ToString();
                        string xmlData = setProp.DefaultValue.ToString();
                        XmlSerializer xs = new XmlSerializer(typeof(string[]));
                        string[] data = (string[])xs.Deserialize(new XmlTextReader(xmlData, XmlNodeType.Element, null));

                        switch (settingType)
                        {
                            case "System.Collections.Specialized.StringCollection":
                                StringCollection sc = new StringCollection();
                                sc.AddRange(data);
                                return sc;

                            default: return "";
                        }
                    }
                }
                else
                {
                    retVal = "";
                }
            }
            return retVal;
        }

        private void SetSetting(SettingsPropertyValue setProp)
        {
            // Define the XML path under which we want to write our settings if they do not already exist
            XmlNode SettingNode = null;

            try
            {
                // Search for the specific settings node we want to update.
                // If it exists, return its first child node, (the <value>data here</value> node)
                SettingNode = XMLConfig.SelectSingleNode("//setting[@name='" + setProp.Name + "']").FirstChild;
            }
            catch (Exception)
            {
                SettingNode = null;
            }

            // If we have a pointer to an actual XML node, update the value stored there
            if ((SettingNode != null))
            {
                if (setProp.Property.SerializeAs.ToString() == "String")
                {
                    SettingNode.InnerText = setProp.SerializedValue.ToString();
                }
                else
                {
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration.
                    SettingNode.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");
                }
            }
            else
            {
                // If the value did not already exist in this settings file, create a new entry for this setting

                // Search for the application settings node (<Appname.Properties.Settings>) and store it.
                XmlNode tmpNode = XMLConfig.SelectSingleNode("//" + APPNODE);

                // Create a new settings node and assign its name as well as how it will be serialized
                XmlElement newSetting = _xmlDoc.CreateElement("setting");
                newSetting.SetAttribute("name", setProp.Name);

                if (setProp.Property.SerializeAs.ToString() == "String")
                {
                    newSetting.SetAttribute("serializeAs", "String");
                }
                else
                {
                    newSetting.SetAttribute("serializeAs", "Xml");
                }

                // Append this node to the application settings node (<Appname.Properties.Settings>)
                tmpNode.AppendChild(newSetting);

                // Create an element under our named settings node, and assign it the value we are trying to save
                XmlElement valueElement = _xmlDoc.CreateElement("value");
                if (setProp.Property.SerializeAs.ToString() == "String")
                {
                    valueElement.InnerText = setProp.SerializedValue.ToString();
                }
                else
                {
                    // Write the object to the config serialized as Xml - we must remove the Xml declaration when writing
                    // the value, otherwise .Net's configuration system complains about the additional declaration
                    valueElement.InnerXml = setProp.SerializedValue.ToString().Replace(@"<?xml version=""1.0"" encoding=""utf-16""?>", "");
                }

                //Append this new element under the setting node we created above
                newSetting.AppendChild(valueElement);
            }
        }
    }
}
