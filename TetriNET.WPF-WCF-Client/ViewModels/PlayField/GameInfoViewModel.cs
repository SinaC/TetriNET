using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Data;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
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
                    TimeLeft = value;
                    Opacity = 1.0;
                    // Restart timer
                    _timer.Stop();
                    _timerStarted = DateTime.Now;
                    _timer.Start();
                }
            }
        }

        public double TimeLeft { get; private set; }

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
            TimeLeft = TotalSeconds - elapsedSeconds;
            Opacity = 1.0 - elapsedSeconds/TotalSeconds;
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

        private int _score;
        public int Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
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

        public List<ContinuousEffect> Effects { get; private set; }
        public ICollectionView EffectsView { get; private set; }

        public GameInfoViewModel()
        {
            _timer = new Timer(250);
            _timer.Elapsed += TimerOnElapsed;

            Effects = new List<ContinuousEffect>();
            EffectsView = CollectionViewSource.GetDefaultView(Effects);
            EffectsView.SortDescriptions.Add(new SortDescription("TimeLeft", ListSortDirection.Descending));
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            ElapsedTime = DateTime.Now - _gameStartTime;
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnContinuousEffectToggled -= OnContinuousEffectToggled;
            oldClient.OnLinesClearedChanged -= DisplayClearedLines;
            oldClient.OnLevelChanged -= DisplayLevel;
            oldClient.OnScoreChanged -= DisplayScore;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameOver -= StopTimerAndComputeTime;
            oldClient.OnGameFinished -= StopTimerAndComputeTime;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnContinuousEffectToggled += OnContinuousEffectToggled;
            newClient.OnLinesClearedChanged += DisplayClearedLines;
            newClient.OnLevelChanged += DisplayLevel;
            newClient.OnScoreChanged += DisplayScore;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameOver += StopTimerAndComputeTime;
            newClient.OnGameFinished += StopTimerAndComputeTime;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregistered()
        {
            StopTimer(false);
        }

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
            DisplayLevel(0);
            DisplayClearedLines(0);
            DisplayScore(0);
            Effects.Clear();
            _gameStartTime = DateTime.Now;
            ElapsedTime = TimeSpan.FromSeconds(0);
            _timer.Start();
        }

        private void DisplayLevel(int level)
        {
            Level = level == 0 ? Client.Level : level;
        }

        private void DisplayClearedLines(int linesCleared)
        {
            LinesCleared = linesCleared == 0 ? Client.LinesCleared : linesCleared;
        }

        private void DisplayScore(int score)
        {
            Score = score == 0 ? Client.Score : score;
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
                    ExecuteOnUIThread.Invoke(EffectsView.Refresh);
                }
                else
                {
                    effect = new ContinuousEffect
                    {
                        Special = special,
                        TotalSeconds = duration
                    };
                    Effects.Add(effect);
                    ExecuteOnUIThread.Invoke(EffectsView.Refresh);
                }
            }
            else
            {
                ContinuousEffect effect = Effects.FirstOrDefault(x => x.Special == special);
                if (effect != null)
                {
                    Effects.Remove(effect);
                    ExecuteOnUIThread.Invoke(EffectsView.Refresh);
                }
            }
        }
    }

    // http://www.codewrecks.com/blog/index.php/2012/11/07/wpf-and-design-time-data-part-2use-a-concrete-class-2/
    public class GameInfoViewModelDesignData : GameInfoViewModel
    {
        public new ObservableCollection<ContinuousEffect> EffectsView { get; set; }

        public GameInfoViewModelDesignData()
        {
            EffectsView = new ObservableCollection<ContinuousEffect>
                {
                    new ContinuousEffect
                        {
                            Special = Specials.Confusion,
                            TotalSeconds = 30
                        },
                    new ContinuousEffect
                        {
                            Special = Specials.Darkness,
                            TotalSeconds = 30
                        }
                };
        }
    }
}