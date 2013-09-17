using System;
using System.Configuration;
using System.IO;
using System.Windows;
using TetriNET.Logger;
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
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);

            // Log user settings path
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Log.WriteLine(Log.LogLevels.Info, "Local user config path: {0}", config.FilePath);

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
                Textures.Textures.TexturesSingleton.Instance.ReadFromPath(textureFilepath);
            else
            {
                Textures.Textures.TexturesSingleton.Instance.ReadFromFile(textureFilepath);
                //Textures.Textures.TexturesSingleton.Instance.SaveToPath(@"d:\temp\x2mod\");
            }

            //
            base.OnStartup(e);
        }
    }
}
