﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.WPF_WCF_Client.Messages;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class ClientOptionsViewModel : ViewModelBase
    {
        public static ClientOptionsViewModel Instance { get; private set; }

        public const int Width = 12; // TODO: should depend on Server
        public const int Height = 22; // TODO: should depend on Server

        private bool _isGameNotStarted;
        public bool IsGameNotStarted
        {
            get { return _isGameNotStarted; }
            set { Set(() => IsGameNotStarted, ref _isGameNotStarted, value); }
        }

        // Automatically switch to play field when game is started and to party line when game is over
        private bool _automaticallySwitchToPlayFieldOnGameStarted;
        public bool AutomaticallySwitchToPlayFieldOnGameStarted
        {
            get { return _automaticallySwitchToPlayFieldOnGameStarted; }
            set
            {
                if (Set(() => AutomaticallySwitchToPlayFieldOnGameStarted, ref _automaticallySwitchToPlayFieldOnGameStarted, value))
                {
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
                if (Set(() => AutomaticallySwitchToPartyLineOnRegistered, ref _automaticallySwitchToPartyLineOnRegistered, value))
                {
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
                if (Set(() => DisplayOpponentsFieldEvenWhenNotPlaying, ref _displayOpponentsFieldEvenWhenNotPlaying, value))
                {
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
                if (Set(() => IsDeveloperModeActivated, ref _isDeveloperModeActivated, value))
                {
                    Settings.Default.IsDeveloperModeActivated = _isDeveloperModeActivated;
                    Settings.Default.Save();
                    Mediator.Send(new IsDeveloperModeModifiedMessage
                    {
                        IsActivated = IsDeveloperModeActivated
                    });
                }
            }
        }

        private bool _displayDropLocation;
        public bool DisplayDropLocation
        {
            get { return _displayDropLocation; }
            set
            {
                if (Set(() => DisplayDropLocation, ref _displayDropLocation, value))
                {
                    Settings.Default.DisplayDropLocation = _displayDropLocation;
                    Settings.Default.Save();
                }
            }
        }

        private bool _displayPieceAnchor;
        public bool DisplayPieceAnchor
        {
            get { return _displayPieceAnchor; }
            set
            {
                if (Set(() => DisplayPieceAnchor, ref _displayPieceAnchor, value))
                {
                    Settings.Default.DisplayPieceAnchor = _displayPieceAnchor;
                    Settings.Default.Save();
                }
            }
        }

        #region Key settings

        public ObservableCollection<KeySettingViewModel> KeySettings { get; protected set; }

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
            DisplayPieceAnchor = Settings.Default.DisplayPieceAnchor;

            DropSensibilityViewModel = new SensibilityViewModel("DropSensibility");
            DownSensibilityViewModel = new SensibilityViewModel("DownSensibility");
            LeftSensibilityViewModel = new SensibilityViewModel("LeftSensibility");
            RightSensibilityViewModel = new SensibilityViewModel("RightSensibility");

            KeySettings = new ObservableCollection<KeySettingViewModel>();

            SetKeySetting(Settings.Default.Down, TetriNET.Client.Interfaces.Commands.Down);
            SetKeySetting(Settings.Default.Drop, TetriNET.Client.Interfaces.Commands.Drop);
            SetKeySetting(Settings.Default.Left, TetriNET.Client.Interfaces.Commands.Left);
            SetKeySetting(Settings.Default.Right, TetriNET.Client.Interfaces.Commands.Right);
            SetKeySetting(Settings.Default.RotateClockwise, TetriNET.Client.Interfaces.Commands.RotateClockwise);
            SetKeySetting(Settings.Default.RotateCounterclockwise, TetriNET.Client.Interfaces.Commands.RotateCounterclockwise);
            SetKeySetting(Settings.Default.Hold, TetriNET.Client.Interfaces.Commands.Hold);
            SetKeySetting(Settings.Default.DiscardFirstSpecial, TetriNET.Client.Interfaces.Commands.DiscardFirstSpecial);
            SetKeySetting(Settings.Default.UseSpecialOn1, TetriNET.Client.Interfaces.Commands.UseSpecialOn1);
            SetKeySetting(Settings.Default.UseSpecialOn2, TetriNET.Client.Interfaces.Commands.UseSpecialOn2);
            SetKeySetting(Settings.Default.UseSpecialOn3, TetriNET.Client.Interfaces.Commands.UseSpecialOn3);
            SetKeySetting(Settings.Default.UseSpecialOn4, TetriNET.Client.Interfaces.Commands.UseSpecialOn4);
            SetKeySetting(Settings.Default.UseSpecialOn5, TetriNET.Client.Interfaces.Commands.UseSpecialOn5);
            SetKeySetting(Settings.Default.UseSpecialOn6, TetriNET.Client.Interfaces.Commands.UseSpecialOn6);
            SetKeySetting(Settings.Default.UseSpecialOnSelf, TetriNET.Client.Interfaces.Commands.UseSpecialOnSelf);
            SetKeySetting(Settings.Default.UseSpecialOnRandomOpponent, TetriNET.Client.Interfaces.Commands.UseSpecialOnRandomOpponent);
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.GameOver -= OnGameOver;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.PlayerUnregistered -= OnPlayerUnregister;
            oldClient.ConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.GameOver += OnGameOver;
            newClient.GameFinished += OnGameFinished;
            newClient.GameStarted += OnGameStarted;
            newClient.PlayerUnregistered += OnPlayerUnregister;
            newClient.ConnectionLost += OnConnectionLost;
        }

        #endregion

        #region IClient events handler

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            IsGameNotStarted = true;
        }

        private void OnPlayerUnregister()
        {
            IsGameNotStarted = true;
        }

        private void OnGameStarted()
        {
            IsGameNotStarted = false;
        }

        private void OnGameFinished(GameStatistics statistics)
        {
            IsGameNotStarted = true;
        }

        private void OnGameOver()
        {
            IsGameNotStarted = true;
        }

        #endregion

        private void SetKeySetting(int key, Commands cmd)
        {
            KeySettingViewModel keySetting = KeySettings.FirstOrDefault(x => x.Command == cmd);
            if (keySetting != null)
                keySetting.Key = (Key) key;
            else
                KeySettings.Add(new KeySettingViewModel((Key) key, cmd));
        }
    }

    public class ClientOptionsViewModelDesignData : ClientOptionsViewModel
    {
        public ClientOptionsViewModelDesignData()
        {
            KeySettings = new ObservableCollection<KeySettingViewModel>
                {
                    new KeySettingViewModel(Key.Space, TetriNET.Client.Interfaces.Commands.Drop),
                    new KeySettingViewModel(Key.Down, TetriNET.Client.Interfaces.Commands.Down),
                    new KeySettingViewModel(Key.Up, TetriNET.Client.Interfaces.Commands.RotateCounterclockwise),
                    new KeySettingViewModel(Key.PageUp, TetriNET.Client.Interfaces.Commands.RotateClockwise),
                    new KeySettingViewModel(Key.Left, TetriNET.Client.Interfaces.Commands.Left),
                    new KeySettingViewModel(Key.Right, TetriNET.Client.Interfaces.Commands.Right),
                    new KeySettingViewModel(Key.H, TetriNET.Client.Interfaces.Commands.Hold),
                    new KeySettingViewModel(Key.D, TetriNET.Client.Interfaces.Commands.DiscardFirstSpecial),
                    new KeySettingViewModel(Key.D1, TetriNET.Client.Interfaces.Commands.UseSpecialOn1),
                    new KeySettingViewModel(Key.D2, TetriNET.Client.Interfaces.Commands.UseSpecialOn2),
                    new KeySettingViewModel(Key.D3, TetriNET.Client.Interfaces.Commands.UseSpecialOn3),
                    new KeySettingViewModel(Key.D4, TetriNET.Client.Interfaces.Commands.UseSpecialOn4),
                    new KeySettingViewModel(Key.D5, TetriNET.Client.Interfaces.Commands.UseSpecialOn5),
                    new KeySettingViewModel(Key.D6, TetriNET.Client.Interfaces.Commands.UseSpecialOn6),
                    new KeySettingViewModel(Key.Enter, TetriNET.Client.Interfaces.Commands.UseSpecialOnSelf),
                    new KeySettingViewModel(Key.D0, TetriNET.Client.Interfaces.Commands.UseSpecialOnRandomOpponent)
                };
        }
    }
}