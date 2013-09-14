using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.Game
{
    /// <summary>
    /// Interaction logic for GameInfoControl.xaml
    /// </summary>
    public partial class GameInfoControl : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("InfoClientProperty", typeof(IClient), typeof(GameInfoControl), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private int _level;
        public int Level
        {
            get { return _level; }
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _linesCleared;
        public int LinesCleared
        {
            get { return _linesCleared; }
            set
            {
                if (_linesCleared != value)
                {
                    _linesCleared = value;
                    OnPropertyChanged();
                }
            }
        }

        public GameInfoControl()
        {
            InitializeComponent();
        }

        private void DisplayLevel()
        {
            Level = Client.Level;
        }

        private void DisplayClearedLines()
        {
            LinesCleared = Client.LinesCleared;
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            GameInfoControl _this = sender as GameInfoControl;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnLinesClearedChanged -= _this.OnLinesClearedChanged;
                    oldClient.OnLevelChanged -= _this.OnLevelChanged;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnLinesClearedChanged += _this.OnLinesClearedChanged;
                    newClient.OnLevelChanged += _this.OnLevelChanged;
                    newClient.OnGameStarted += _this.OnGameStarted;
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                DisplayLevel();
                DisplayClearedLines();
            });
        }

        private void OnLevelChanged()
        {
            ExecuteOnUIThread.Invoke(DisplayLevel);
        }

        private void OnLinesClearedChanged()
        {
            ExecuteOnUIThread.Invoke(DisplayClearedLines);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
