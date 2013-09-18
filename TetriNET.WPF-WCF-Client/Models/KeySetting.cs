using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Models
{
    public class KeySetting : INotifyPropertyChanged
    {
        private Key _key;
        public Key Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    OnPropertyChanged();
                }
            }
        }

        private Commands _command;

        public Commands Command
        {
            get { return _command; }
            set
            {
                if (_command != value)
                {
                    _command = value;
                    OnPropertyChanged();
                }
            }
        }

        public KeySetting(Key key, Commands command)
        {
            Key = key;
            Command = command;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
