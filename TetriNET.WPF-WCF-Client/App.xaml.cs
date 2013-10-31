using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Windows;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.CustomSettings;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            //
            ExecuteOnUIThread.Initialize();

            // Initialize Log
            string logFilename = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5) + ".log";
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);
            
            //// Log user settings path
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            //Log.WriteLine(Log.LogLevels.Info, "Local user config path: {0}", config.FilePath);

            //// TODO: if PortableSettingsProvider file doesn't exist, copy config.FilePath to PortableSettingsProvider path

            // If new config file doesn't exist, rename user.config or copy old one
            try
            {
                string newPath = Path.Combine(PortableSettingsProvider.SettingsPath, PortableSettingsProvider.SettingsFilename);
                if (!File.Exists(newPath))
                {
                    //
                    string oldPath = Path.Combine(PortableSettingsProvider.SettingsPath, "user.config");
                    if (File.Exists(oldPath))
                    {
                        Log.WriteLine(Log.LogLevels.Info, @"User settings file not found. Rename {0} to {1}", oldPath, newPath);
                        File.Move(oldPath, newPath);
                    }
                    else
                    {
                        // Original config file path
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                        if (File.Exists(config.FilePath))
                        {
                            Log.WriteLine(Log.LogLevels.Info, "User settings file not found. Copy {0} to {1}", config.FilePath, newPath);
                            if (!Directory.Exists(PortableSettingsProvider.SettingsPath))
                                Directory.CreateDirectory(PortableSettingsProvider.SettingsPath);
                            File.Copy(config.FilePath, newPath);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while creating new config file from old one. Exception: {0}", ex.ToString());
            }


            //ExtractTetrinetTextures();

            // Get textures
            string textureFilepath = ConfigurationManager.AppSettings["texture"];
            bool isDirectory = false;
            try
            {
                FileAttributes attr = File.GetAttributes(textureFilepath);
                isDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch
            {
            }
            if (isDirectory)
                TextureManager.TextureManager.TexturesSingleton.Instance.ReadFromPath(textureFilepath);
            else
                TextureManager.TextureManager.TexturesSingleton.Instance.ReadFromFile(textureFilepath);

            //
            base.OnStartup(e);
        }

        private static void ExtractTetrinetTextures()
        {
            TextureManager.TextureManager.TexturesSingleton.Instance.ReadFromFile(@"d:\github\TetriNET\Images\tetrinet2.bmp");
            TextureManager.TextureManager.TexturesSingleton.Instance.SaveToPath(@"d:\temp\tetrinet2tex\");
        }
    }
}
