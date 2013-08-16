using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Tetris.Model.UI.DisplayBehaviours
{
    internal class DisplayFadeIn : IDisplayBehaviour
    {
        private readonly OverlayUserControl _this;

        public DisplayFadeIn(OverlayUserControl control)
        {
            _this = control;
        }

        /// <summary>
        /// Slowly display the control by increasing its opacity
        /// </summary>
        public void Show()
        {
            _this.Visibility = Visibility.Visible;

            #region Create the animation

            var animation = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(1000),
                    From = 0,
                    To = 1,
                };

            #endregion

            #region Create the completed event

            var completed = new EventHandler((obj, args) =>
                {
                    //empty to override the hide-completed-event
                });

            #endregion

            ExecuteAnimation(animation, completed);
        }

        /// <summary>
        /// Makes the control fade out slowly
        /// </summary>
        public void Hide()
        {
            #region Create the animation

            var animation = new DoubleAnimation
                {
                    Duration = TimeSpan.FromMilliseconds(500),
                    From = 1,
                    To = 0,
                };

            #endregion

            #region Create the completed event

            var completed = new EventHandler((obj, args) =>
                {
                    _this.Visibility = Visibility.Hidden;
                });

            #endregion

            ExecuteAnimation(animation, completed);
        }

        private void ExecuteAnimation(DoubleAnimation animation, EventHandler completed)
        {
            #region Add the animation to the storyboard

            var storyboard = new Storyboard();

            storyboard.Children.Add(animation);
            Storyboard.SetTarget(animation, _this);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Control.Opacity)"));

            #endregion

            storyboard.Completed += completed;

            storyboard.Begin();
        }
    }
}
