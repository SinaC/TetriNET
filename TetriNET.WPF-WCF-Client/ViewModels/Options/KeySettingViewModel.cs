﻿using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class KeySettingViewModel : ObservableObject
    {
        private Key _key;
        public Key Key
        {
            get { return _key; }
            set
            {
                if (Set(() => Key, ref _key, value))
                {
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
                return Key.ToString();
            }
        }

        private Client.Interfaces.Commands _command;
        public Client.Interfaces.Commands Command
        {
            get { return _command; }
            set
            {
                if (Set(() => Command, ref _command, value))
                    OnPropertyChanged("CommandDescription");
            }
        }

        public string CommandDescription
        {
            get
            {
                DescriptionAttribute attribute = EnumHelper.GetAttribute<DescriptionAttribute>(Command);
                return attribute?.Description;
            }
        }


        public KeySettingViewModel(Key key, Client.Interfaces.Commands command)
        {
            Key = key;
            Command = command;
        }

        private void SaveKeySetting()
        {
            switch (Command)
            {
                case Client.Interfaces.Commands.Hold:
                    Settings.Default.Hold = (int) Key;
                    break;
                case Client.Interfaces.Commands.Drop:
                    Settings.Default.Drop = (int) Key;
                    break;
                case Client.Interfaces.Commands.Down:
                    Settings.Default.Down = (int) Key;
                    break;
                case Client.Interfaces.Commands.Left:
                    Settings.Default.Left = (int) Key;
                    break;
                case Client.Interfaces.Commands.Right:
                    Settings.Default.Right = (int) Key;
                    break;
                case Client.Interfaces.Commands.RotateClockwise:
                    Settings.Default.RotateClockwise = (int) Key;
                    break;
                case Client.Interfaces.Commands.RotateCounterclockwise:
                    Settings.Default.RotateCounterclockwise = (int) Key;
                    break;
                case Client.Interfaces.Commands.DiscardFirstSpecial:
                    Settings.Default.DiscardFirstSpecial = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOn1:
                    Settings.Default.UseSpecialOn1 = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOn2:
                    Settings.Default.UseSpecialOn2 = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOn3:
                    Settings.Default.UseSpecialOn3 = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOn4:
                    Settings.Default.UseSpecialOn4 = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOn5:
                    Settings.Default.UseSpecialOn5 = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOn6:
                    Settings.Default.UseSpecialOn6 = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOnSelf:
                    Settings.Default.UseSpecialOnSelf = (int) Key;
                    break;
                case Client.Interfaces.Commands.UseSpecialOnRandomOpponent:
                    Settings.Default.UseSpecialOnRandomOpponent = (int) Key;
                    break;
            }
            Settings.Default.Save();
        }
    }
}