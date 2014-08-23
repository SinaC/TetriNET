using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using TetriNET.Common.DataContracts;
using TetriNET.WPF_WCF_Client.MVVM;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class ContinuousEffect : ObservableObject
    {
        private const double Epsilon = 0.00001;

        private Specials _special;
        public Specials Special
        {
            get { return _special; }
            set { Set(() => Special, ref _special, value); }
        }

        private double _opacity;
        public double Opacity
        {
            get { return _opacity; }
            set { Set(() => Opacity, ref _opacity, value); }
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
            Opacity = 1.0 - elapsedSeconds / TotalSeconds;
        }
    }
}
