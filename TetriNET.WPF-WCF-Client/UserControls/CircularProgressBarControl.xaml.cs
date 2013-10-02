using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Threading;

//http://blogs.u2u.be/diederik/post/2010/02/26/Yet-another-Circular-ProgressBar-control-for-WPF.aspx
//http://sachabarbs.wordpress.com/2009/12/29/better-wpf-circular-progress-bar/
namespace TetriNET.WPF_WCF_Client.UserControls
{
    /// <summary>
    /// Spinning Busy Indicator Control.
    /// </summary>
    public partial class CircularProgressBarControl
    {
        /// <summary>
        /// Startup time in miliseconds, default is a second.
        /// </summary>
        public static readonly DependencyProperty StartupDelayProperty =
            DependencyProperty.Register(
                "StartupDelay",
                typeof(int),
                typeof(CircularProgressBarControl),
                new PropertyMetadata(1000));

        /// <summary>
        /// Spinning Speed. Default is 60, that's one rotation per second.
        /// </summary>
        public static readonly DependencyProperty RotationsPerMinuteProperty =
            DependencyProperty.Register(
                "RotationsPerMinute",
                typeof(double),
                typeof(CircularProgressBarControl),
                new PropertyMetadata(60.0));

        /// <summary>
        /// Timer for the Animation.
        /// </summary>
        private readonly DispatcherTimer _animationTimer;

        ///// <summary>
        ///// Mouse Cursor.
        ///// </summary>
        //private Cursor _originalCursor;

        /// <summary>
        /// Initializes a new instance of the CircularProgressBarControl class.
        /// </summary>
        public CircularProgressBarControl()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer(DispatcherPriority.Normal, Dispatcher);
        }

        /// <summary>
        /// Gets or sets the startup time in miliseconds, default is a second.
        /// </summary>
        public int StartupDelay
        {
            get
            {
                return (int)GetValue(StartupDelayProperty);
            }

            set
            {
                SetValue(StartupDelayProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the spinning speed. Default is 60, that's one rotation per second.
        /// </summary>
        public double RotationsPerMinute
        {
            get
            {
                return (double)GetValue(RotationsPerMinuteProperty);
            }

            set
            {
                SetValue(RotationsPerMinuteProperty, value);
            }
        }

        /// <summary>
        /// Startup Delay.
        /// </summary>
        private void StartDelay()
        {
            //_originalCursor = Mouse.OverrideCursor;
            //Mouse.OverrideCursor = Cursors.Wait;

            // Startup
            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, StartupDelay);
            _animationTimer.Tick += StartSpinning;
            _animationTimer.Start();
        }

        /// <summary>
        /// Start Spinning.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event Arguments.</param>
        private void StartSpinning(object sender, EventArgs e)
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= StartSpinning;

            // 60 secs per minute, 1000 millisecs per sec, 10 rotations per full circle:
            _animationTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(6000 / RotationsPerMinute));
            _animationTimer.Tick += HandleAnimationTick;
            _animationTimer.Start();
            Opacity = 1;

            //Mouse.OverrideCursor = _originalCursor;
        }

        /// <summary>
        /// The control became invisible: stop spinning (animation consumes CPU).
        /// </summary>
        private void StopSpinning()
        {
            _animationTimer.Stop();
            _animationTimer.Tick -= HandleAnimationTick;
            Opacity = 0;
        }

        /// <summary>
        /// Apply a single rotation transformation.
        /// </summary>
        /// <param name="sender">Sender of the Event: the Animation Timer.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleAnimationTick(object sender, EventArgs e)
        {
            SpinnerRotate.Angle = (SpinnerRotate.Angle + 36) % 360;
            //double i = C0.Opacity;
            //C0.Opacity = C1.Opacity;
            //C1.Opacity = C2.Opacity;
            //C2.Opacity = C3.Opacity;
            //C3.Opacity = C4.Opacity;
            //C4.Opacity = C5.Opacity;
            //C5.Opacity = C6.Opacity;
            //C6.Opacity = C7.Opacity;
            //C7.Opacity = C8.Opacity;
            //C7.Opacity = C9.Opacity;
            //C9.Opacity = i;
        }

        /// <summary>
        /// Control was loaded: distribute circles.
        /// </summary>
        /// <param name="sender">Sender of the Event: I wish I knew.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            SetPosition(C0, 0.0);
            SetPosition(C1, 1.0);
            SetPosition(C2, 2.0);
            SetPosition(C3, 3.0);
            SetPosition(C4, 4.0);
            SetPosition(C5, 5.0);
            SetPosition(C6, 6.0);
            SetPosition(C7, 7.0);
            SetPosition(C8, 8.0);
            SetPosition(C9, 9.0);
        }

        /// <summary>
        /// Calculate position of a circle.
        /// </summary>
        /// <param name="ellipse">The circle.</param>
        /// <param name="sequence">Sequence number of the circle.</param>
        private static void SetPosition(Ellipse ellipse, double sequence)
        {
            ellipse.SetValue(
                Canvas.LeftProperty,
                50.0 + (Math.Sin(Math.PI * ((0.2 * sequence) + 1.0)) * 50.0));

            ellipse.SetValue(
                Canvas.TopProperty,
                50.0 + (Math.Cos(Math.PI * ((0.2 * sequence) + 1.0)) * 50.0));
        }

        /// <summary>
        /// Control was unloaded: stop spinning.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleUnloaded(object sender, RoutedEventArgs e)
        {
            StopSpinning();
        }

        /// <summary>
        /// Visibility property was changed: start or stop spinning.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Don't give the developer a headache.
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
                return;

            bool isVisible = (bool)e.NewValue;

            if (isVisible)
                StartDelay();
            else
                StopSpinning();
        }
    }
}
