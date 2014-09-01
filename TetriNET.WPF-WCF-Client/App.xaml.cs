using System;
using System.Configuration;
using System.IO;
using System.Windows;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.CustomSettings;
using TetriNET.WPF_WCF_Client.Helpers;

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
            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);
            
            //// Log user settings path
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            //Log.Default.WriteLine(LogLevels.Info, "Local user config path: {0}", config.FilePath);

            //// TODO: if PortableSettingsProvider file doesn't exist, copy config.FilePath to PortableSettingsProvider path

            if (!Directory.Exists(PortableSettingsProvider.SettingsPath))
                Directory.CreateDirectory(PortableSettingsProvider.SettingsPath);
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
                        Log.Default.WriteLine(LogLevels.Info, @"User settings file not found. Rename {0} to {1}", oldPath, newPath);
                        File.Move(oldPath, newPath);
                    }
                    else
                    {
                        // Original config file path
                        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                        if (File.Exists(config.FilePath))
                        {
                            Log.Default.WriteLine(LogLevels.Info, "User settings file not found. Copy {0} to {1}", config.FilePath, newPath);
                            File.Copy(config.FilePath, newPath);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Error while creating new config file from old one. Exception: {0}", ex.ToString());
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
                TextureManager.TextureManager.TexturesSingleInstance.Instance.ReadFromPath(textureFilepath);
            else
                TextureManager.TextureManager.TexturesSingleInstance.Instance.ReadFromFile(textureFilepath);

            //
            base.OnStartup(e);
        }

        private static void ExtractTetrinetTextures()
        {
            TextureManager.TextureManager.TexturesSingleInstance.Instance.ReadFromFile(@"d:\github\TetriNET\Images\tetrinet2.bmp");
            TextureManager.TextureManager.TexturesSingleInstance.Instance.SaveToPath(@"d:\temp\tetrinet2tex\");
        }
    }
}
