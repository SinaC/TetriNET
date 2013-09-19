using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Properties;

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
                    OnPropertyChanged("KeyDescription");
                    SaveKeySetting();
                }
            }
        }

        public string KeyDescription
        {
            get
            {
                //int toto = KeyInterop.VirtualKeyFromKey(Key);
                if (Key >= Key.D0 && Key <= Key.D9)
                    return ((int) Key - (int) Key.D0).ToString(CultureInfo.InvariantCulture);
                else
                    return Key.ToString();
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
                    OnPropertyChanged("CommandDescription");
                }
            }
        }

        public string CommandDescription
        {
            get
            {
                DescriptionAttribute attribute = EnumHelper.GetAttribute<DescriptionAttribute>(Command);
                return attribute == null ? null : attribute.Description;
            }
        }


        public KeySetting(Key key, Commands command)
        {
            Key = key;
            Command = command;
        }

        private void SaveKeySetting()
        {
            switch (Command)
            {
                case Commands.Drop:
                    Settings.Default.Drop = (int)Key;
                    break;
                case Commands.Down:
                    Settings.Default.Down = (int)Key;
                    break;
                case Commands.Left:
                    Settings.Default.Left = (int)Key;
                    break;
                case Commands.Right:
                    Settings.Default.Right = (int)Key;
                    break;
                case Commands.RotateClockwise:
                    Settings.Default.RotateClockwise = (int)Key;
                    break;
                case Commands.RotateCounterclockwise:
                    Settings.Default.RotateCounterclockwise = (int)Key;
                    break;
                case Commands.DiscardFirstSpecial:
                    Settings.Default.DiscardFirstSpecial = (int)Key;
                    break;
                case Commands.UseSpecialOn1:
                    Settings.Default.UseSpecialOn1 = (int)Key;
                    break;
                case Commands.UseSpecialOn2:
                    Settings.Default.UseSpecialOn2 = (int)Key;
                    break;
                case Commands.UseSpecialOn3:
                    Settings.Default.UseSpecialOn3 = (int)Key;
                    break;
                case Commands.UseSpecialOn4:
                    Settings.Default.UseSpecialOn4 = (int)Key;
                    break;
                case Commands.UseSpecialOn5:
                    Settings.Default.UseSpecialOn5 = (int)Key;
                    break;
                case Commands.UseSpecialOn6:
                    Settings.Default.UseSpecialOn6 = (int)Key;
                    break;
            }
            Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
