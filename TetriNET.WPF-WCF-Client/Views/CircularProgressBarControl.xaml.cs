using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TetriNET.WPF_WCF_Client.Views
{
    public partial class CircularProgressBarControl
    {
        private readonly DispatcherTimer _animationTimer;

        public CircularProgressBarControl()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 75)
            };
        }

        private void Start()
        {
            //Mouse.OverrideCursor = Cursors.Wait;
            _animationTimer.Tick += HandleAnimationTick;
            _animationTimer.Start();
        }

        private void Stop()
        {
            _animationTimer.Stop();
            //Mouse.OverrideCursor = Cursors.Arrow;
            _animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            const double length = 50;
            const double step = Math.PI * 2 / 10.0;
            const double offset = Math.PI;

            C0.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 0.0 * step) * length);
            C0.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 0.0 * step) * length);

            C1.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 1.0 * step) * length);
            C1.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 1.0 * step) * length);

            C2.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 2.0 * step) * length);
            C2.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 2.0 * step) * length);

            C3.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 3.0 * step) * length);
            C3.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 3.0 * step) * length);

            C4.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 4.0 * step) * length);
            C4.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 4.0 * step) * length);

            C5.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 5.0 * step) * length);
            C5.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 5.0 * step) * length);

            C6.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 6.0 * step) * length);
            C6.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 6.0 * step) * length);

            C7.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 7.0 * step) * length);
            C7.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 7.0 * step) * length);

            C8.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 8.0 * step) * length);
            C8.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 8.0 * step) * length);
        }

        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void HandleVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this)) 
                return;

            bool isVisible = (bool)e.NewValue;

            if (isVisible)
                Start();
            else
                Stop();
        }
    }
}
