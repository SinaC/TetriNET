using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class ValuePercentage
    {
        public int Value { get; set; }
        public int Percentage { get; set; }
    }

    // TODO: create sub view model with an ObservableDictionary + Matching Sum

    public class ClientStatisticsViewModel : ViewModelBase, ITabIndex
    {
        public ObservableDictionary<Pieces, ValuePercentage> PieceCount
        {
            get
            {
                if (Client == null)
                    return null;
                return BuildStatistics(Client.Statistics.PieceCount);
            }
        }

        public ObservableDictionary<Specials, ValuePercentage> SpecialCount
        {
            get
            {
                if (Client == null)
                    return null;
                return BuildStatistics(Client.Statistics.SpecialCount);
            }
        }

        public ObservableDictionary<Specials, ValuePercentage> SpecialUsed
        {
            get
            {
                if (Client == null)
                    return null;
                return BuildStatistics(Client.Statistics.SpecialUsed);
            }
        }

        public ObservableDictionary<Specials, ValuePercentage> SpecialDiscarded
        {
            get
            {
                if (Client == null)
                    return null;
                return BuildStatistics(Client.Statistics.SpecialDiscarded);
            }
        }

        public int PiecesCountSum
        {
            get { return Client == null ? 0 : Client.Statistics.PieceCount.Values.Sum(); }
        }

        public int SpecialCountSum
        {
            get { return Client == null ? 0 : Client.Statistics.SpecialCount.Values.Sum(); }
        }

        public int SpecialUsedSum
        {
            get { return Client == null ? 0 : Client.Statistics.SpecialUsed.Values.Sum(); }
        }

        public int SpecialDiscardedSum
        {
            get { return Client == null ? 0 : Client.Statistics.SpecialDiscarded.Values.Sum(); }
        }

        public int EndOfPieceQueueReached
        {
            get { return Client == null ? 0 : Client.Statistics.EndOfPieceQueueReached; }
        }
        
        public int NextPieceNotYetReceived
        {
            get { return Client == null ? 0 : Client.Statistics.NextPieceNotYetReceived; }
        }

        public int TetrisCount
        {
            get { return Client == null ? 0 : Client.Statistics.TetrisCount; }
        }

        private DateTime _gameStartedDateTime;
        public double LinesPerSec
        {
            get
            {
                if (Client == null)
                    return 0;
                TimeSpan timeSpan = DateTime.Now - _gameStartedDateTime;
                double totalSeconds = timeSpan.TotalSeconds;
                if (totalSeconds < 0.0001)
                    return 0;
                return Client.LinesCleared/totalSeconds;
            }
        }

        public ClientStatisticsViewModel()
        {
            RefreshStatisticsCommand = new RelayCommand(Refresh);
        }

        private void Refresh()
        {
            OnPropertyChanged("PieceCount");
            OnPropertyChanged("SpecialCount");
            OnPropertyChanged("SpecialUsed");
            OnPropertyChanged("SpecialDiscarded");
            OnPropertyChanged("PiecesCountSum");
            OnPropertyChanged("SpecialCountSum");
            OnPropertyChanged("SpecialUsedSum");
            OnPropertyChanged("SpecialDiscardedSum");
            OnPropertyChanged("TetrisCount");
            OnPropertyChanged("EndOfPieceQueueReached");
            OnPropertyChanged("NextPieceNotYetReceived");
            OnPropertyChanged("LinesPerSec");
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 5; }
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
        }

        private void OnGameFinished()
        {
            Refresh();
        }

        private void OnGameOver()
        {
            Refresh();
        }

        #endregion

        #region Commands

        public ICommand RefreshStatisticsCommand { get; set; }

        #endregion

        private static ObservableDictionary<T, ValuePercentage> BuildStatistics<T>(IDictionary<T, int> dictionary)
        {
            int sum = dictionary.Values.Sum();
            ObservableDictionary<T, ValuePercentage> returnValue = new ObservableDictionary<T, ValuePercentage>();
            foreach (KeyValuePair<T, int> kv in dictionary)
            {
                returnValue.Add(kv.Key, new ValuePercentage
                {
                    Value = kv.Value,
                    Percentage = sum == 0 ? 0 : (100 * kv.Value) / sum
                });
            }
            return returnValue;
        }
    }
}