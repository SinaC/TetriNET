using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class ClientOptionsViewModel : ViewModelBase
    {
        public static ClientOptionsViewModel Instance { get; private set; }

        public const int Width = 12;
        public const int Height = 22;

        private bool _isGameNotStarted;
        public bool IsGameNotStarted
        {
            get { return _isGameNotStarted; }
            set
            {
                if (_isGameNotStarted != value)
                {
                    _isGameNotStarted = value;
                    OnPropertyChanged();
                }
            }
        }

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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                    Settings.Default.DisplayDropLocation = _displayDropLocation;
                    Settings.Default.Save();
                }
            }
        }

        #region Key settings

        public ObservableCollection<KeySetting> KeySettings { get; private set; }

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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                    Settings.Default.RightSensibility = _rightSensibility;
                    Settings.Default.Save();
                }
            }
        }

        #endregion

        public ClientOptionsViewModel()
        {
            Instance = this;

            IsGameNotStarted = true;
            IsDeveloperModeActivated = false;

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

            KeySettings = new ObservableCollection<KeySetting>();

            SetKeySetting(Settings.Default.Down, Common.Interfaces.Commands.Down);
            SetKeySetting(Settings.Default.Drop, Common.Interfaces.Commands.Drop);
            SetKeySetting(Settings.Default.Left, Common.Interfaces.Commands.Left);
            SetKeySetting(Settings.Default.Right, Common.Interfaces.Commands.Right);
            SetKeySetting(Settings.Default.RotateClockwise, Common.Interfaces.Commands.RotateClockwise);
            SetKeySetting(Settings.Default.RotateCounterclockwise, Common.Interfaces.Commands.RotateCounterclockwise);
            SetKeySetting(Settings.Default.DiscardFirstSpecial, Common.Interfaces.Commands.DiscardFirstSpecial);
            SetKeySetting(Settings.Default.UseSpecialOn1, Common.Interfaces.Commands.UseSpecialOn1);
            SetKeySetting(Settings.Default.UseSpecialOn2, Common.Interfaces.Commands.UseSpecialOn2);
            SetKeySetting(Settings.Default.UseSpecialOn3, Common.Interfaces.Commands.UseSpecialOn3);
            SetKeySetting(Settings.Default.UseSpecialOn4, Common.Interfaces.Commands.UseSpecialOn4);
            SetKeySetting(Settings.Default.UseSpecialOn5, Common.Interfaces.Commands.UseSpecialOn5);
            SetKeySetting(Settings.Default.UseSpecialOn6, Common.Interfaces.Commands.UseSpecialOn6);
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregister;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameOver += OnGameOver;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnPlayerUnregistered += OnPlayerUnregister;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregister()
        {
            IsGameNotStarted = true;
        }

        private void OnGameStarted()
        {
            IsGameNotStarted = false;
        }

        private void OnGameFinished()
        {
            IsGameNotStarted = true;
        }

        private void OnGameOver()
        {
            IsGameNotStarted = true;
        }

        #endregion

        private void SetKeySetting(int key, Common.Interfaces.Commands cmd)
        {
            KeySetting keySetting = KeySettings.FirstOrDefault(x => x.Command == cmd);
            if (keySetting != null)
                keySetting.Key = (System.Windows.Input.Key) key;
            else
                KeySettings.Add(new KeySetting((System.Windows.Input.Key) key, cmd));
        }
    }

    public class ClientOptionsViewModelDesignData : ClientOptionsViewModel
    {
        public new ObservableCollection<KeySetting> KeySettings { get; private set; }

        public ClientOptionsViewModelDesignData()
        {
            KeySettings = new ObservableCollection<KeySetting>
                {
                    new KeySetting(Key.Space, Common.Interfaces.Commands.Drop),
                    new KeySetting(Key.Down, Common.Interfaces.Commands.Down),
                    new KeySetting(Key.Up, Common.Interfaces.Commands.RotateCounterclockwise),
                    new KeySetting(Key.Left, Common.Interfaces.Commands.Left),
                    new KeySetting(Key.Right, Common.Interfaces.Commands.Right),
                };
        }
    }
}