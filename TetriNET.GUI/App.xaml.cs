using System;
using System.Windows;
using System.IO;

namespace Tetris
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        public string SerializationPath 
        {
            get
            {
                var serializationPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Sekagra\Tetris";
                if (!Directory.Exists(serializationPath))
                    Directory.CreateDirectory(serializationPath);

                return serializationPath;
            }
        }
    }
}
