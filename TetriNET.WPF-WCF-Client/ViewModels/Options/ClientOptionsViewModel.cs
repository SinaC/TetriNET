using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Client.Interfaces;
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

        public SensibilityViewModel DropSensibilityViewModel { get; private set; }
        public SensibilityViewModel DownSensibilityViewModel { get; private set; }
        public SensibilityViewModel LeftSensibilityViewModel { get; private set; }
        public SensibilityViewModel RightSensibilityViewModel { get; private set; } 

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

            DropSensibilityViewModel = new SensibilityViewModel("DropSensibility");
            DownSensibilityViewModel = new SensibilityViewModel("DownSensibility");
            LeftSensibilityViewModel = new SensibilityViewModel("LeftSensibility");
            RightSensibilityViewModel = new SensibilityViewModel("RightSensibility");

            KeySettings = new ObservableCollection<KeySetting>();

            SetKeySetting(Settings.Default.Down, TetriNET.Client.Interfaces.Commands.Down);
            SetKeySetting(Settings.Default.Drop, TetriNET.Client.Interfaces.Commands.Drop);
            SetKeySetting(Settings.Default.Left, TetriNET.Client.Interfaces.Commands.Left);
            SetKeySetting(Settings.Default.Right, TetriNET.Client.Interfaces.Commands.Right);
            SetKeySetting(Settings.Default.RotateClockwise, TetriNET.Client.Interfaces.Commands.RotateClockwise);
            SetKeySetting(Settings.Default.RotateCounterclockwise, TetriNET.Client.Interfaces.Commands.RotateCounterclockwise);
            SetKeySetting(Settings.Default.DiscardFirstSpecial, TetriNET.Client.Interfaces.Commands.DiscardFirstSpecial);
            SetKeySetting(Settings.Default.UseSpecialOn1, TetriNET.Client.Interfaces.Commands.UseSpecialOn1);
            SetKeySetting(Settings.Default.UseSpecialOn2, TetriNET.Client.Interfaces.Commands.UseSpecialOn2);
            SetKeySetting(Settings.Default.UseSpecialOn3, TetriNET.Client.Interfaces.Commands.UseSpecialOn3);
            SetKeySetting(Settings.Default.UseSpecialOn4, TetriNET.Client.Interfaces.Commands.UseSpecialOn4);
            SetKeySetting(Settings.Default.UseSpecialOn5, TetriNET.Client.Interfaces.Commands.UseSpecialOn5);
            SetKeySetting(Settings.Default.UseSpecialOn6, TetriNET.Client.Interfaces.Commands.UseSpecialOn6);
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

        private void SetKeySetting(int key, Client.Interfaces.Commands cmd)
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
                    new KeySetting(Key.Space, TetriNET.Client.Interfaces.Commands.Drop),
                    new KeySetting(Key.Down, TetriNET.Client.Interfaces.Commands.Down),
                    new KeySetting(Key.Up, TetriNET.Client.Interfaces.Commands.RotateCounterclockwise),
                    new KeySetting(Key.Left, TetriNET.Client.Interfaces.Commands.Left),
                    new KeySetting(Key.Right, TetriNET.Client.Interfaces.Commands.Right),
                };
        }
    }
}