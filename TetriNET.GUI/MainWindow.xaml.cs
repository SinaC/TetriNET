using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tetris.Model;
using System.ComponentModel;

namespace Tetris
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        private Settings _settings;
        public Settings Settings
        {
            get { return _settings; }
            set
            {
                _settings = value;
                OnPropertyChanged("Settings");
            }
        }

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
            Settings = Settings.Instance;
        }

        #endregion

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Start playing the background music
            Settings.MusicPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Music/Gee.mp3", UriKind.Relative));
        }

        #region Execute incoming registered RoutedCommands

        private void StartGame_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StartGame_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewGame.Tetris = new Model.Tetris();
            viewGame.Show();
            viewGame.Tetris.StartGame();
        }


        private void EnterSettings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void EnterSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewSettings.Show();
        }


        private void QuitApplication_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void QuitApplication_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EnterScores_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void EnterScores_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewScores.Show();
        }

        private void EnterCredits_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void EnterCredits_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            viewCredits.Show();
        }

        #endregion

        //Pass Key-Events to the Game
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            var key = Settings.KeySettings.FirstOrDefault(k => k.Key == e.Key);
            if (viewGame.Tetris != null && key != null)
                viewGame.Game_KeyDown(key.Command);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            var key = Settings.KeySettings.FirstOrDefault(k => k.Key == e.Key);
            if (viewGame.Tetris != null && key != null)
                viewGame.Game_KeyUp(key.Command);
        }

        /// <summary>
        /// Recalculate the height or width to keep the ratio of the game and the menu always the same
        /// </summary>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //The Tetris grid itself has a ratio of 10:18 and is 66% of the complete width

            #region Get all necessary values to calculate

            var paddingHeight = border.Padding.Top + border.Padding.Bottom;
            var paddingWidth = border.Padding.Left + border.Padding.Right;

            var height = e.NewSize.Height - paddingHeight;
            var width = e.NewSize.Width - paddingWidth;

            #endregion

            #region Check the ratio. The smaller value sets the limit

            if (height < width*1.8*0.66666666)
            {
                border.Width = (height/1.8*1.51515151);
                border.Height = height - 30; //idk why, but this seems to be necessary to prevent the content from being cut off at the bottom
            }
            else
            {
                border.Height = (width*1.8*0.66666666);
                border.Width = width - 10; //idk why, but this seems to be necessary to prevent the content from being cut off on the right
            }

            #endregion
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (viewGame.IsDisplayed && !viewGame.Tetris.IsPaused)
                viewGame.Tetris.PauseGame();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Serialize the settings
            Settings.Instance.Serialize();

            //Delete the temp files
            Settings.Instance.MusicPlayer.Dispose();
            Settings.Instance.SoundPlayer.Dispose();
        }

        #region OnPropertyChanged Event

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Wraps the PropertyChanged-Event.
        /// </summary>
        /// <param name="property">The name of the property that changed.</param>
        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }

        #endregion

        #endregion
    }
}