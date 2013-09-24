using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

//http://blogs.u2u.be/diederik/post/2010/02/26/Yet-another-Circular-ProgressBar-control-for-WPF.aspx
//http://sachabarbs.wordpress.com/2009/12/29/better-wpf-circular-progress-bar/
namespace TetriNET.WPF_WCF_Client.UserControls
{
    public partial class CircularProgressBar
    {
        private readonly DispatcherTimer _animationTimer;

        public CircularProgressBar()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer(DispatcherPriority.ContextIdle, Dispatcher)
            {
                Interval = new TimeSpan(0, 0, 0, 0, 75)
            };
        }

        private void Start()
        {
            _animationTimer.Tick += HandleAnimationTick;
            _animationTimer.Start();
        }

        private void Stop()
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= HandleAnimationTick;
        }

        private void HandleAnimationTick(object sender, EventArgs e)
        {
            double i = C0.Opacity;
            C0.Opacity = C1.Opacity;
            C1.Opacity = C2.Opacity;
            C2.Opacity = C3.Opacity;
            C3.Opacity = C4.Opacity;
            C4.Opacity = C5.Opacity;
            C5.Opacity = C6.Opacity;
            C6.Opacity = C7.Opacity;
            C7.Opacity = C8.Opacity;
            C7.Opacity = C9.Opacity;
            C9.Opacity = i;
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

            C9.SetValue(Canvas.LeftProperty, length + Math.Sin(offset + 9.0 * step) * length);
            C9.SetValue(Canvas.TopProperty, length + Math.Cos(offset + 9.0 * step) * length);
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
