using System.Collections.ObjectModel;
using System.Linq;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.Models
{
    public class Options
    {
        public const int Width = 12;
        public const int Height = 22;

        public GameOptions ServerOptions { get; set; } // Modified by UI and by Server on each game started

        // Automatically switch to play field when game is started and to party line when game is over
        private bool _automaticallySwitchToPlayFieldOnGameStarted;
        public bool AutomaticallySwitchToPlayFieldOnGameStarted
        {
            get { return _automaticallySwitchToPlayFieldOnGameStarted; }
            set
            {
                if (_automaticallySwitchToPlayFieldOnGameStarted != value)
                {
                    _automaticallySwitchToPlayFieldOnGameStarted = value;
                    Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted = _automaticallySwitchToPlayFieldOnGameStarted;
                    Settings.Default.Save();
                }
            }
        }

        // Automatically switch to party line when registering successfully
        private bool _automaticallySwitchToPartyLineOnRegistered;
        public bool AutomaticallySwitchToPartyLineOnRegistered
        {
            get { return _automaticallySwitchToPartyLineOnRegistered; }
            set
            {
                if (_automaticallySwitchToPartyLineOnRegistered != value)
                {
                    _automaticallySwitchToPartyLineOnRegistered = value;
                    Settings.Default.AutomaticallySwitchToPartyLineOnRegistered = _automaticallySwitchToPartyLineOnRegistered;
                    Settings.Default.Save();
                }
            }
        }

        private bool _displayOpponentsFieldEvenWhenNotPlaying;
        public bool DisplayOpponentsFieldEvenWhenNotPlaying
        {
            get { return _displayOpponentsFieldEvenWhenNotPlaying; }
            set
            {
                if (_displayOpponentsFieldEvenWhenNotPlaying != value)
                {
                    _displayOpponentsFieldEvenWhenNotPlaying = value;
                    Settings.Default.DisplayOpponentsFieldEvenWhenNotPlaying = _displayOpponentsFieldEvenWhenNotPlaying;
                    Settings.Default.Save();
                }
            }
        }

        private bool _isDeveloperModeActivated;
        public bool IsDeveloperModeActivated
        {
            get { return _isDeveloperModeActivated; }
            set
            {
                if (_isDeveloperModeActivated != value)
                {
                    _isDeveloperModeActivated = value;
                    Settings.Default.IsDeveloperModeActivated = _isDeveloperModeActivated;
                    Settings.Default.Save();
                }
            }
        }

        private bool _displayDropLocation;
        public bool DisplayDropLocation
        {
            get { return _displayDropLocation; }
            set
            {
                if (_displayDropLocation != value)
                {
                    _displayDropLocation = value;
                    Settings.Default.DisplayDropLocation = _displayDropLocation;
                    Settings.Default.Save();
                }
            }
        }

        #region Key settings

        public ObservableCollection<KeySetting> KeySettings { get; set; }

        #endregion

        #region Sensibility

        private bool _dropSensibilityActivated;
        public bool DropSensibilityActivated
        {
            get { return _dropSensibilityActivated; }
            set
            {
                if (_dropSensibilityActivated != value)
                {
                    _dropSensibilityActivated = value;
                    Settings.Default.DropSensibilityActivated = _dropSensibilityActivated;
                    Settings.Default.Save();
                }
            }
        }

        private int _dropSensibility;
        public int DropSensibility
        {
            get { return _dropSensibility; }
            set
            {
                if (_dropSensibility != value)
                {
                    _dropSensibility = value;
                    Settings.Default.DropSensibility = _dropSensibility;
                    Settings.Default.Save();
                }
            }
        }

        private bool _downSensibilityActivated;
        public bool DownSensibilityActivated
        {
            get { return _downSensibilityActivated; }
            set
            {
                if (_downSensibilityActivated != value)
                {
                    _downSensibilityActivated = value;
                    Settings.Default.DownSensibilityActivated = _downSensibilityActivated;
                    Settings.Default.Save();
                }
            }
        }

        private int _downSensibility;
        public int DownSensibility
        {
            get { return _downSensibility; }
            set
            {
                if (_downSensibility != value)
                {
                    _downSensibility = value;
                    Settings.Default.DownSensibility = _downSensibility;
                    Settings.Default.Save();
                }
            }
        }

        private bool _leftSensibilityActivated;
        public bool LeftSensibilityActivated
        {
            get { return _leftSensibilityActivated; }
            set
            {
                if (_leftSensibilityActivated != value)
                {
                    _leftSensibilityActivated = value;
                    Settings.Default.LeftSensibilityActivated = _leftSensibilityActivated;
                    Settings.Default.Save();
                }
            }
        }

        private int _leftSensibility;
        public int LeftSensibility
        {
            get { return _leftSensibility; }
            set
            {
                if (_leftSensibility != value)
                {
                    _leftSensibility = value;
                    Settings.Default.LeftSensibility = _leftSensibility;
                    Settings.Default.Save();
                }
            }
        }

        private bool _rightSensibilityActivated;
        public bool RightSensibilityActivated
        {
            get { return _rightSensibilityActivated; }
            set
            {
                if (_rightSensibilityActivated != value)
                {
                    _rightSensibilityActivated = value;
                    Settings.Default.RightSensibilityActivated = _rightSensibilityActivated;
                    Settings.Default.Save();
                }
            }
        }

        private int _rightSensibility;
        public int RightSensibility
        {
            get { return _rightSensibility; }
            set
            {
                if (_rightSensibility != value)
                {
                    _rightSensibility = value;
                    Settings.Default.RightSensibility = _rightSensibility;
                    Settings.Default.Save();
                }
            }
        }

        #endregion

        #region Singleton

        public static readonly ThreadSafeSingleton<Options> OptionsSingleton = new ThreadSafeSingleton<Options>(() => new Options());

        private Options()
        {
            // Singleton
            KeySettings = new ObservableCollection<KeySetting>();
        }

        #endregion

        public void GetSavedOptions()
        {
            AutomaticallySwitchToPartyLineOnRegistered = Settings.Default.AutomaticallySwitchToPartyLineOnRegistered;
            AutomaticallySwitchToPlayFieldOnGameStarted = Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted;
            DisplayOpponentsFieldEvenWhenNotPlaying = Settings.Default.DisplayOpponentsFieldEvenWhenNotPlaying;
            IsDeveloperModeActivated = Settings.Default.IsDeveloperModeActivated;
            DisplayDropLocation = Settings.Default.DisplayDropLocation;
            DropSensibilityActivated = Settings.Default.DropSensibilityActivated;
            DropSensibility = Settings.Default.DropSensibility;
            DownSensibilityActivated = Settings.Default.DownSensibilityActivated;
            DownSensibility = Settings.Default.DownSensibility;
            LeftSensibilityActivated = Settings.Default.LeftSensibilityActivated;
            LeftSensibility = Settings.Default.LeftSensibility;
            RightSensibilityActivated = Settings.Default.RightSensibilityActivated;
            RightSensibility = Settings.Default.RightSensibility;

            SetSetting(Settings.Default.Down, Commands.Down);
            SetSetting(Settings.Default.Drop, Commands.Drop);
            SetSetting(Settings.Default.Left, Commands.Left);
            SetSetting(Settings.Default.Right, Commands.Right);
            SetSetting(Settings.Default.RotateClockwise, Commands.RotateClockwise);
            SetSetting(Settings.Default.RotateCounterclockwise, Commands.RotateCounterclockwise);
            SetSetting(Settings.Default.DiscardFirstSpecial, Commands.DiscardFirstSpecial);
            SetSetting(Settings.Default.UseSpecialOn1, Commands.UseSpecialOn1);
            SetSetting(Settings.Default.UseSpecialOn2, Commands.UseSpecialOn2);
            SetSetting(Settings.Default.UseSpecialOn3, Commands.UseSpecialOn3);
            SetSetting(Settings.Default.UseSpecialOn4, Commands.UseSpecialOn4);
            SetSetting(Settings.Default.UseSpecialOn5, Commands.UseSpecialOn5);
            SetSetting(Settings.Default.UseSpecialOn6, Commands.UseSpecialOn6);
        }

        private void SetSetting(int key, Commands cmd)
        {
            KeySetting keySetting = KeySettings.FirstOrDefault(x => x.Command == cmd);
            if (keySetting != null)
                keySetting.Key = (System.Windows.Input.Key) key;
            else
                KeySettings.Add(new KeySetting((System.Windows.Input.Key) key, cmd));
        }

        //public void AddDefaultForMissingOptions()
        //{
        //}
    }
}