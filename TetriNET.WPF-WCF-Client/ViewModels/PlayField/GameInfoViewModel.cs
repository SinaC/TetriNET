using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    // TODO: class continuous effect timer + collection of timer sorted by time left

    public class GameInfoViewModel : ViewModelBase
    {
        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _linesCleared;
        public int LinesCleared
        {
            get { return _linesCleared; }
            set
            {
                if (_linesCleared != value)
                {
                    _linesCleared = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly Timer _timer;
        private DateTime _gameStartTime;
        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set
            {
                if (_elapsedTime != value)
                {
                    _elapsedTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public GameInfoViewModel()
        {
            _timer = new Timer(250);
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            ElapsedTime = DateTime.Now - _gameStartTime;
        }

        private void DisplayLevel()
        {
            Level = Client.Level;
        }

        private void DisplayClearedLines()
        {
            LinesCleared = Client.LinesCleared;
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnLinesClearedChanged -= OnLinesClearedChanged;
            oldClient.OnLevelChanged -= OnLevelChanged;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameOver -= StopTimerAndComputeTime;
            oldClient.OnGameFinished -= StopTimerAndComputeTime;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerUnregistered -= StopTimerAndComputeTime;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnLinesClearedChanged += OnLinesClearedChanged;
            newClient.OnLevelChanged += OnLevelChanged;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameOver += StopTimerAndComputeTime;
            newClient.OnGameFinished += StopTimerAndComputeTime;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerUnregistered += StopTimerAndComputeTime;
        }

        #endregion

        #region IClient events handler

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            StopTimer(false);
        }

        private void StopTimerAndComputeTime()
        {
            StopTimer(true);
        }

        private void StopTimer(bool computeTime = true)
        {
            _timer.Stop();
            if (computeTime)
                ElapsedTime = DateTime.Now - _gameStartTime;
        }

        private void OnGameStarted()
        {
            DisplayLevel();
            DisplayClearedLines();
            _gameStartTime = DateTime.Now;
            ElapsedTime = TimeSpan.FromSeconds(0);
            _timer.Start();
        }

        private void OnLevelChanged()
        {
            DisplayLevel();
        }

        private void OnLinesClearedChanged()
        {
            DisplayClearedLines();
        }

        #endregion
    }
}