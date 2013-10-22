using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class ValuePercentage
    {
        public int Value { get; set; }
        public double Percentage { get; set; }

        public string PercentageAsString
        {
            get
            {
                if (Percentage < 0.00001)
                    return "0";
                else if (Percentage < 1)
                    return "<1";
                else
                    return String.Format("{0:0.0}", Percentage);
            }
        }
    }

    public class SpecialPercentages
    {
        public ValuePercentage Count { get; set; }
        public ValuePercentage Used { get; set; }
        public ValuePercentage Discarded { get; set; }
    }

    // TODO: create sub view model with an ObservableDictionary + Matching Sum

    public class ClientStatisticsViewModel : ViewModelBase, ITabIndex
    {
        public ObservableDictionary<Pieces, ValuePercentage> PieceCount
        {
            get
            {
                if (Client == null || Client.Statistics == null)
                    return null;
                return BuildStatistics(Client.Statistics.PieceCount);
            }
        }

        public ObservableDictionary<Specials, ValuePercentage> SpecialCount
        {
            get
            {
                if (Client == null || Client.Statistics == null)
                    return null;
                return BuildStatistics(Client.Statistics.SpecialCount);
            }
        }

        public ObservableDictionary<Specials, ValuePercentage> SpecialUsed
        {
            get
            {
                if (Client == null || Client.Statistics == null)
                    return null;
                return BuildStatistics(Client.Statistics.SpecialUsed);
            }
        }

        public ObservableDictionary<Specials, ValuePercentage> SpecialDiscarded
        {
            get
            {
                if (Client == null || Client.Statistics == null)
                    return null;
                return BuildStatistics(Client.Statistics.SpecialDiscarded);
            }
        }

        public ObservableDictionary<Specials, SpecialPercentages> Specials {
            get
            {
                if (Client == null || Client.Statistics == null)
                    return null;
                return BuildStatistics(Client.Statistics);
            }
        }

        public int PiecesCountSum
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.PieceCount.Values.Sum(); }
        }

        public int SpecialCountSum
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.SpecialCount.Values.Sum(); }
        }

        public int SpecialUsedSum
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.SpecialUsed.Values.Sum(); }
        }

        public int SpecialDiscardedSum
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.SpecialDiscarded.Values.Sum(); }
        }

        public int EndOfPieceQueueReached
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.EndOfPieceQueueReached; }
        }
        
        public int NextPieceNotYetReceived
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.NextPieceNotYetReceived; }
        }

        public int TetrisCount
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.TetrisCount; }
        }

        public int TripleCount
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.TripleCount; }
        }

        public int DoubleCount
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.DoubleCount; }
        }

        public int SingleCount
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.SingleCount; }
        }

        public int GameWon
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.GameWon; }
        }

        public int GameLost
        {
            get { return Client == null || Client.Statistics == null ? 0 : Client.Statistics.GameLost; }
        }

        private bool _gameFinished;
        private DateTime _gameStartedDateTime;
        private DateTime _gameFinishedDateTime;
        public double LinesPerMinute
        {
            get
            {
                if (Client == null)
                    return 0;
                TimeSpan timeSpan = (_gameFinished ? _gameFinishedDateTime : DateTime.Now) - _gameStartedDateTime;
                double totalMinutes = timeSpan.TotalMinutes;
                if (totalMinutes < 0.0001)
                    return 0;
                return Client.LinesCleared/totalMinutes;
            }
        }

        public double MovesPerMinute
        {
            get
            {
                if (Client == null || Client.Statistics == null)
                    return 0;
                TimeSpan timeSpan = (_gameFinished ? _gameFinishedDateTime : DateTime.Now) - _gameStartedDateTime;
                double totalMinutes = timeSpan.TotalMinutes;
                if (totalMinutes < 0.0001)
                    return 0;
                return Client.Statistics.MoveCount / totalMinutes;
            }
        }

        public ClientStatisticsViewModel()
        {
            RefreshStatisticsCommand = new RelayCommand(Refresh);
        }

        private void Refresh()
        {
            OnPropertyChanged("Specials");
            OnPropertyChanged("PieceCount");
            OnPropertyChanged("SpecialCount");
            OnPropertyChanged("SpecialUsed");
            OnPropertyChanged("SpecialDiscarded");
            OnPropertyChanged("PiecesCountSum");
            OnPropertyChanged("SpecialCountSum");
            OnPropertyChanged("SpecialUsedSum");
            OnPropertyChanged("SpecialDiscardedSum");
            OnPropertyChanged("TetrisCount");
            OnPropertyChanged("TripleCount");
            OnPropertyChanged("DoubleCount");
            OnPropertyChanged("SingleCount");
            OnPropertyChanged("EndOfPieceQueueReached");
            OnPropertyChanged("NextPieceNotYetReceived");
            OnPropertyChanged("LinesPerMinute");
            OnPropertyChanged("MovesPerMinute");
            OnPropertyChanged("GameWon");
            OnPropertyChanged("GameLost");
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 6; }
        }

        #endregion

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameOver -= OnGameOver;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameOver += OnGameOver;
        }

        #endregion

        #region IClient events handler

        private void OnGameStarted()
        {
            _gameStartedDateTime = DateTime.Now;
            _gameFinished = false;
        }

        private void OnGameFinished()
        {
            if (!_gameFinished)
            {
                _gameFinishedDateTime = DateTime.Now;
                _gameFinished = true;
                Refresh();
            }
        }

        private void OnGameOver()
        {
            if (!_gameFinished)
            {
                _gameFinishedDateTime = DateTime.Now;
                _gameFinished = true;
                Refresh();
            }
        }

        #endregion

        #region Commands

        public ICommand RefreshStatisticsCommand { get; private set; }

        #endregion

        protected static ObservableDictionary<T, ValuePercentage> BuildStatistics<T>(IDictionary<T, int> dictionary)
        {
            int sum = dictionary.Values.Sum();
            ObservableDictionary<T, ValuePercentage> returnValue = new ObservableDictionary<T, ValuePercentage>();
            foreach (KeyValuePair<T, int> kv in dictionary)
            {
                returnValue.Add(kv.Key, new ValuePercentage
                {
                    Value = kv.Value,
                    Percentage = sum == 0 ? 0 : (double)(100 * kv.Value) / (double)sum
                });
            }
            return returnValue;
        }

        protected static ObservableDictionary<Specials, SpecialPercentages> BuildStatistics(IClientStatistics statistics)
        {
            int countSum = statistics.SpecialCount.Values.Sum();
            int usedSum = statistics.SpecialUsed.Values.Sum();
            int discardedSum = statistics.SpecialDiscarded.Values.Sum();
            ObservableDictionary<Specials, SpecialPercentages> returnValue = new ObservableDictionary<Specials, SpecialPercentages>();
            foreach (Specials special in Common.Helpers.EnumHelper.GetSpecials(b => b))
            {
                returnValue.Add(special, new SpecialPercentages
                {
                    Count = BuildPercentage(statistics.SpecialCount, special, countSum),
                    Used = BuildPercentage(statistics.SpecialUsed, special, usedSum),
                    Discarded = BuildPercentage(statistics.SpecialDiscarded, special, discardedSum),
                });
            }
            return returnValue;
        }

        private static ValuePercentage BuildPercentage(IDictionary<Specials, int> dictionary, Specials special, int sum)
        {
            int value = dictionary.ContainsKey(special) ? dictionary[special] : 0;
            return new ValuePercentage
            {
                Value = value,
                Percentage = sum == 0 ? 0 : (double)(100 * value) / (double)sum
            };
        }
    }

    public class ClientStatisticsViewModelDesignData : ClientStatisticsViewModel
    {
        public class ClientStatistics : IClientStatistics
        {
            public Dictionary<Pieces, int> PieceCount { get; set; }
            public Dictionary<Specials, int> SpecialCount { get; set; }
            public Dictionary<Specials, int> SpecialUsed { get; set; }
            public Dictionary<Specials, int> SpecialDiscarded { get; set; }
            public int MoveCount { get; set; }
            public int SingleCount { get; set; }
            public int DoubleCount { get; set; }
            public int TripleCount { get; set; }
            public int TetrisCount { get; set; }
            public int EndOfPieceQueueReached { get; set; }
            public int NextPieceNotYetReceived { get; set; }
            public int GameWon { get; set; }
            public int GameLost { get; set; }
        }

        public new ObservableDictionary<Pieces, ValuePercentage> PieceCount { get; private set; }
        public new ObservableDictionary<Specials, ValuePercentage> SpecialCount { get; private set; }
        public new ObservableDictionary<Specials, ValuePercentage> SpecialUsed { get; private set; }
        public new ObservableDictionary<Specials, ValuePercentage> SpecialDiscarded { get; private set; }
        public new ObservableDictionary<Specials, SpecialPercentages> Specials { get; private set; }

        public new int PiecesCountSum
        {
            get { return PieceCount.Aggregate(0, (sum, pair) => sum + pair.Value.Value); }
        }

        public new int SpecialCountSum
        {
            get { return SpecialCount.Aggregate(0, (sum, pair) => sum + pair.Value.Value); }
        }

        public new int SpecialUsedSum
        {
            get { return SpecialUsed.Aggregate(0, (sum, pair) => sum + pair.Value.Value); }
        }

        public new int SpecialDiscardedSum
        {
            get { return SpecialDiscarded.Aggregate(0, (sum, pair) => sum + pair.Value.Value); }
        }

        public ClientStatisticsViewModelDesignData()
        {
            ClientStatistics stats = new ClientStatistics
            {
                PieceCount = Common.Helpers.EnumHelper.GetPieces(b => b).Select(piece => new
                {
                    Key = piece,
                    Value = 1
                }).ToDictionary(x => x.Key, x => x.Value),
                SpecialCount = Common.Helpers.EnumHelper.GetSpecials(b => b).Select(special => new
                {
                    Key = special,
                    Value = 1
                }).ToDictionary(x => x.Key, x => x.Value),
                SpecialUsed = Common.Helpers.EnumHelper.GetSpecials(b => b).Select(special => new
                {
                    Key = special,
                    Value = 1
                }).ToDictionary(x => x.Key, x => x.Value),
                SpecialDiscarded = Common.Helpers.EnumHelper.GetSpecials(b => b).Select(special => new
                {
                    Key = special,
                    Value = 1
                }).ToDictionary(x => x.Key, x => x.Value)
            };

            PieceCount = BuildStatistics(stats.PieceCount);
            SpecialCount = BuildStatistics(stats.SpecialCount);
            SpecialUsed = BuildStatistics(stats.SpecialUsed);
            SpecialDiscarded = BuildStatistics(stats.SpecialDiscarded);

            Specials = BuildStatistics(stats);
        }
    }
}