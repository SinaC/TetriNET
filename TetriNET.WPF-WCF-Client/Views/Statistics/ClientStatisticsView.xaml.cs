using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Annotations;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.Statistics
{
    public class ValuePercentage
    {
        public int Value { get; set; }
        public int Percentage { get; set; }
    }

    /// <summary>
    /// Interaction logic for ClientStatisticsView.xaml
    /// </summary>
    public partial class ClientStatisticsView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("ClientStatisticsViewClientProperty", typeof (IClient), typeof (ClientStatisticsView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient) GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public ObservableDictionary<Tetriminos, ValuePercentage> TetriminoCount
        {
            get
            {
                if (Client == null)
                    return null;
                int sum = Client.Statistics.TetriminoCount.Values.Sum();
                ObservableDictionary<Tetriminos, ValuePercentage> returnValue = new ObservableDictionary<Tetriminos, ValuePercentage>();
                foreach (KeyValuePair<Tetriminos, int> kv in Client.Statistics.TetriminoCount)
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

        public ObservableDictionary<Specials, ValuePercentage> SpecialCount
        {
            get
            {
                if (Client == null)
                    return null;
                int sum = Client.Statistics.SpecialCount.Values.Sum();
                ObservableDictionary<Specials, ValuePercentage> returnValue = new ObservableDictionary<Specials, ValuePercentage>();
                foreach (KeyValuePair<Specials, int> kv in Client.Statistics.SpecialCount)
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

        public ObservableDictionary<Specials, ValuePercentage> SpecialUsed
        {
            get
            {
                if (Client == null)
                    return null;
                int sum = Client.Statistics.SpecialUsed.Values.Sum();
                ObservableDictionary<Specials, ValuePercentage> returnValue = new ObservableDictionary<Specials, ValuePercentage>();
                foreach (KeyValuePair<Specials, int> kv in Client.Statistics.SpecialUsed)
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

        public ObservableDictionary<Specials, ValuePercentage> SpecialDiscarded
        {
            get
            {
                if (Client == null)
                    return null;
                int sum = Client.Statistics.SpecialDiscarded.Values.Sum();
                ObservableDictionary<Specials, ValuePercentage> returnValue = new ObservableDictionary<Specials, ValuePercentage>();
                foreach (KeyValuePair<Specials, int> kv in Client.Statistics.SpecialDiscarded)
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

        public int TetriminosCountSum
        {
            get { return Client == null ? 0 : Client.Statistics.TetriminoCount.Values.Sum(); }
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

        public int EndOfTetriminoQueueReached { get { return Client == null ? 0 : Client.Statistics.EndOfTetriminoQueueReached; }}
        public int NextTetriminoNotYetReceived { get { return Client == null ? 0 : Client.Statistics.NextTetriminoNotYetReceived; } }

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

        public ClientStatisticsView()
        {
            InitializeComponent();
        }

        private void Refresh()
        {
            OnPropertyChanged("TetriminoCount");
            OnPropertyChanged("SpecialCount");
            OnPropertyChanged("SpecialUsed");
            OnPropertyChanged("SpecialDiscarded");
            OnPropertyChanged("TetriminosCountSum");
            OnPropertyChanged("SpecialCountSum");
            OnPropertyChanged("SpecialUsedSum");
            OnPropertyChanged("SpecialDiscardedSum");
            OnPropertyChanged("EndOfTetriminoQueueReached");
            OnPropertyChanged("NextTetriminoNotYetReceived");
            OnPropertyChanged("LinesPerSec");
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ClientStatisticsView _this = sender as ClientStatisticsView;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnGameFinished -= _this.OnGameFinished;
                    oldClient.OnGameOver -= _this.OnGameOver;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnGameFinished += _this.OnGameFinished;
                    newClient.OnGameOver += _this.OnGameOver;
                }
            }
        }

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

        #region UI events handler

        private void RefreshStatistics_OnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}