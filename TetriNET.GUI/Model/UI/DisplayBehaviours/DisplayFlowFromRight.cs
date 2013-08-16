using System;
using System.Windows.Media.Animation;
using System.Windows;

namespace Tetris.Model.UI.DisplayBehaviours
{
    public class DisplayFlowFromRight : IDisplayBehaviour
    {
        private readonly OverlayUserControl _this;

        #region Constructor

        public DisplayFlowFromRight(OverlayUserControl control)
        {
            _this = control;
        }

        #endregion

        /// <summary>
        /// Flow in the control from the right.
        /// </summary>
        public void Show()
        {
            var width = _this.ActualWidth; //The controls width before any animations is the correct value because it is only hidden
            _this.Visibility = Visibility.Visible;

            #region Create the animation

            var animation = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(1000),
                    From = 0,
                    To = width,
                    FillBehavior = FillBehavior.Stop,
                };

            #endregion

            #region Create the completed event

            var completed = new EventHandler((obj, args) =>
                {
                    _this.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _this.IsDisplayed = true;
                });

            #endregion

            ExecuteAnimation(animation, completed);
        }

        /// <summary>
        /// Flow out the given control to the right.
        /// </summary>
        public void Hide()
        {
            #region Create the animation

            var animation = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(1000),
                    From = _this.ActualWidth,
                    To = 0,
                    FillBehavior = FillBehavior.Stop
                };

            #endregion

            #region Create the completed event

            var completed = new EventHandler((obj, args) =>
                {
                    _this.HorizontalAlignment = HorizontalAlignment.Stretch;
                    _this.Visibility = Visibility.Hidden;
                });

            #endregion

            _this.IsDisplayed = false;

            ExecuteAnimation(animation, completed);
        }

        private void ExecuteAnimation(DoubleAnimation animation, EventHandler completed)
        {
            #region Add the animation to the storyboard

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, _this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Control.Width)"));

            #endregion

            //Set the Alignment to right during the animation to make the control flowout to the right
            _this.HorizontalAlignment = HorizontalAlignment.Right;

            storyboard.Completed += completed;
            storyboard.Begin();
        }
    }
}