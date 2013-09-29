using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    // TODO: opacity linked to time left
    public class ContinuousEffect : INotifyPropertyChanged
    {
        private const double Epsilon = 0.00001;

        private Specials _special;
        public Specials Special
        {
            get { return _special; }
            set
            {
                if (_special != value)
                {
                    _special = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _opacity;
        public double Opacity
        {
            get { return _opacity; }
            set {
                if (Math.Abs(_opacity-value) > Epsilon)
                {
                    _opacity = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _totalSeconds;
        public double TotalSeconds
        {
            get { return _totalSeconds; }
            set
            {
                if (Math.Abs(_totalSeconds - value) > Epsilon)
                {
                    _totalSeconds = value;
                    Opacity = 1.0;
                    // Restart timer
                    _timer.Stop();
                    _timerStarted = DateTime.Now;
                    _timer.Start();
                }
            }
        }

        private DateTime _timerStarted;
        private readonly Timer _timer;

        public ContinuousEffect()
        {
            _timer = new Timer(250);
            _timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            double elapsedSeconds = (DateTime.Now - _timerStarted).TotalSeconds;
            Opacity = 1.0 - elapsedSeconds/TotalSeconds; // TODO: 0 -> 1
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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

        private readonly ObservableCollection<ContinuousEffect> _effects;
        public ObservableCollection<ContinuousEffect> Effects { get { return _effects; }}

        public GameInfoViewModel()
        {
            _timer = new Timer(250);
            _timer.Elapsed += TimerOnElapsed;
            _effects = new ObservableCollection<ContinuousEffect>();
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
            oldClient.OnContinuousEffectToggled -= OnContinuousEffectToggled;
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
            newClient.OnContinuousEffectToggled += OnContinuousEffectToggled;
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

        private void OnContinuousEffectToggled(Specials special, bool active, double duration)
        {
            if (active)
            {
                ContinuousEffect effect = Effects.FirstOrDefault(x => x.Special == special);
                if (effect != null)
                {
                    effect.TotalSeconds = duration;
                }
                else
                {
                    effect = new ContinuousEffect
                    {
                        Special = special,
                        TotalSeconds = duration
                    };
                    ExecuteOnUIThread.Invoke(() => _effects.Add(effect));
                }
            }
            else
            {
                ContinuousEffect effect = Effects.FirstOrDefault(x => x.Special == special);
                if (effect != null)
                    ExecuteOnUIThread.Invoke(() => Effects.Remove(effect));
            }
        }
    }
}