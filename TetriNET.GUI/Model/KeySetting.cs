using System;
using System.Windows.Input;
using System.ComponentModel;

namespace Tetris.Model
{
    /// <summary>
    /// Represents a single key assignment in the settings
    /// </summary>
    [Serializable]
    public class KeySetting : INotifyPropertyChanged
    {
        #region Fields

        private Key _key;
        private TetrisCommand _command;

        #endregion

        #region Properties

        public Key Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }
        public TetrisCommand Command
        {
            get { return _command; }
            set
            {
                _command = value;
                OnPropertyChanged("Command");
            }
        }

        #endregion

        public KeySetting(Key key, TetrisCommand command)
        {
            Key = key;
            Command = command;
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
    }

    public enum TetrisCommand
    {
        Down = 1,
        Left = 2,
        Right = 3,
        Rotate = 4,
        Pause = 5,
        Attack = 6,
    }
}
