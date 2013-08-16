using System.Windows.Controls;
using System.ComponentModel;

namespace Tetris.Model.UI
{
    public class OverlayUserControl : UserControl, INotifyPropertyChanged
    {
        #region Fields/Properties

        private bool _isDisplayed;
        /// <summary>
        /// In addition to the Visibility property this can be used to set the visibility including the duration of animations.
        /// </summary>
        public bool IsDisplayed
        {
            get { return _isDisplayed; }
            set
            {
                _isDisplayed = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsDisplayed"));
            }
        }
        public IDisplayBehaviour DisplayBehaviour { get; set; }

        #endregion

        #region Methods

        public void Show()
        {
            DisplayBehaviour.Show();
        }

        public void Hide()
        {
            DisplayBehaviour.Hide();
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
