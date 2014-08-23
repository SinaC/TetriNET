using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows.Data;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class GameInfoViewModel : ViewModelBase
    {
        private int _level;
        public int Level
        {
            get { return _level; }
            set { Set(() => Level, ref _level, value); }
        }

        private int _linesCleared;
        public int LinesCleared
        {
            get { return _linesCleared; }
            set { Set(() => LinesCleared, ref _linesCleared, value); }
        }

        private int _score;
        public int Score
        {
            get { return _score; }
            set { Set(() => Score, ref _score, value); }
        }

        private readonly Timer _timer;
        private DateTime _gameStartTime;
        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { Set(() => ElapsedTime, ref _elapsedTime, value); }
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
            oldClient.ContinuousEffectToggled -= OnContinuousEffectToggled;
            oldClient.LinesClearedChanged -= DisplayClearedLines;
            oldClient.LevelChanged -= DisplayLevel;
            oldClient.ScoreChanged -= DisplayScore;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.GameOver -= StopTimerAndComputeTime;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.PlayerUnregistered -= OnPlayerUnregistered;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.ContinuousEffectToggled += OnContinuousEffectToggled;
            newClient.LinesClearedChanged += DisplayClearedLines;
            newClient.LevelChanged += DisplayLevel;
            newClient.ScoreChanged += DisplayScore;
            newClient.GameStarted += OnGameStarted;
            newClient.GameOver += StopTimerAndComputeTime;
            newClient.GameFinished += OnGameFinished;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.PlayerUnregistered += OnPlayerUnregistered;
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

        private void OnGameFinished(GameStatistics statistics)
        {
            StopTimerAndComputeTime();
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