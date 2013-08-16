using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tetris.Sounds;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Tetris.Model
{
    [Serializable]
    public class Settings : INotifyPropertyChanged
    {
        #region Fields

        private static Settings _instance;
        private ResourceMediaPlayer _musicPlayer;
        private ParallelSoundPlayer _soundPlayer;
        private ObservableCollection<KeySetting> _keySettings;
        private static string SerializationPath
        {
            get { return Path.Combine(((App) Application.Current).SerializationPath, "settings.dat"); }
        }

        #endregion

        #region Properties

        public static Settings Instance
        {
            get
            {
                _instance = _instance ?? new Settings();
                return _instance;
            }
        }

        /* Both SoundPlayers are kinda misplaced in this class.
         * Nevertheless I put them in here, because anything else would be too much of an effort.
         */

        /// <summary>
        /// Reference to the SoundPlayer singleton to bind the volume
        /// </summary>
        public ParallelSoundPlayer SoundPlayer
        {
            get { return _soundPlayer; }
            set { _soundPlayer = value; }
        }

        /// <summary>
        /// This SoundPlayer is only for the background music
        /// </summary>
        public ResourceMediaPlayer MusicPlayer
        {
            get { return _musicPlayer; }
            set { _musicPlayer = value; }
        }

        public ObservableCollection<KeySetting> KeySettings
        {
            get { return _keySettings; }
            set
            {
                _keySettings = value;
                OnPropertyChanged("KeySettings");
            }
        }

        #endregion

        public Settings()
        {
            //Create the instance with the applications default values
            MusicPlayer = new ResourceMediaPlayer
                {
                    IsRepeating = true
                };
            SoundPlayer = ParallelSoundPlayer.Instance;

            #region Deserialize the container object for KeySettings and SoundVolume

            if (File.Exists(SerializationPath))
            {
                var formatter = new BinaryFormatter();
                var stream = new FileStream(SerializationPath, FileMode.Open);
                var container = (SerializeContainer) formatter.Deserialize(stream);
                stream.Close();

                //Assign the deserialized values
                _keySettings = container.KeySettings;
                MusicPlayer.IsMuted = container.IsMuted;
                MusicPlayer.Volume = container.SoundVolume;
                SoundPlayer.Volume = container.SoundVolume;
            }
            else
            {
                MusicPlayer.Volume = 0.5;
                SoundPlayer.Volume = 0.5;
                KeySettings = new ObservableCollection<KeySetting>
                    {
                        new KeySetting(Key.Down, TetrisCommand.Down),
                        new KeySetting(Key.Left, TetrisCommand.Left),
                        new KeySetting(Key.Right, TetrisCommand.Right),
                        new KeySetting(Key.Up, TetrisCommand.Rotate),
                        new KeySetting(Key.Escape, TetrisCommand.Pause),
                        new KeySetting(Key.A, TetrisCommand.Attack)
                    };
            }
            #endregion
        }

        /// <summary>
        /// Serializes the complete key settings to reload them on a later program start.
        /// </summary>
        public void Serialize()
        {
            #region Create and fill the container class

            var container = new SerializeContainer
                {
                    KeySettings = _keySettings,
                    SoundVolume = SoundPlayer.Volume,
                    IsMuted = MusicPlayer.IsMuted
                };

            #endregion

            try
            {
                var stream = new FileStream(SerializationPath, FileMode.Create);
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, container);
                stream.Close();
            }
            catch (Exception)
            {
            }
        }

        #region OnPropertyChanged Event

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wraps the PropertyChanged-Event.
        /// </summary>
        /// <param name="property">The name of the property that changed.</param>
        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        [Serializable]
        private class SerializeContainer
        {
            public ObservableCollection<KeySetting> KeySettings;
            public double SoundVolume;
            public bool IsMuted;
        }
    }
}