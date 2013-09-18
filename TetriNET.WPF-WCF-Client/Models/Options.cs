using System.Collections.Generic;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
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
            get
            {
                return _automaticallySwitchToPlayFieldOnGameStarted;
            }
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

        // TODO: finish following code (imply modification in KeyBox.cs, ClientOptionsViewModel.cs and ClientOptionsView.xaml (.cs)
        #region Key settings

        public List<KeySetting> KeySettings { get; set; }

        #endregion

        #region Sensibility
        private bool _dropSensibilityActivated;
        public bool DropSensibilityActivated
        {
            get { return _dropSensibilityActivated; }
            set { if (_dropSensibilityActivated != value)
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
        }
        #endregion

        public void GetSavedOptions()
        {
            OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered = Settings.Default.AutomaticallySwitchToPartyLineOnRegistered;
            OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted = Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted;
            OptionsSingleton.Instance.DisplayOpponentsFieldEvenWhenNotPlaying = Settings.Default.DisplayOpponentsFieldEvenWhenNotPlaying;
            OptionsSingleton.Instance.IsDeveloperModeActivated = Settings.Default.IsDeveloperModeActivated;
            OptionsSingleton.Instance.DropSensibilityActivated = Settings.Default.DropSensibilityActivated;
            OptionsSingleton.Instance.DropSensibility = Settings.Default.DropSensibility;
            OptionsSingleton.Instance.DownSensibilityActivated = Settings.Default.DownSensibilityActivated;
            OptionsSingleton.Instance.DownSensibility = Settings.Default.DownSensibility;
            OptionsSingleton.Instance.LeftSensibilityActivated = Settings.Default.LeftSensibilityActivated;
            OptionsSingleton.Instance.LeftSensibility = Settings.Default.LeftSensibility;
            OptionsSingleton.Instance.RightSensibilityActivated = Settings.Default.RightSensibilityActivated;
            OptionsSingleton.Instance.RightSensibility = Settings.Default.RightSensibility;
        }

        //public void AddDefaultForMissingOptions()
        //{
        //}
    }
}
