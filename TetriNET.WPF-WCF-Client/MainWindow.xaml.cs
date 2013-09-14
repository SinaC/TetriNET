using System.Windows;
using System.Windows.Input;
using TetriNET.Common.GameDatas;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;
using TetriNET.WPF_WCF_Client.ViewModels;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _mainWindowViewModel; // TODO: temporary code to be removed

        public MainWindow()
        {
            InitializeComponent();
        }

        // TODO: temporary code to be removed
        public void Initialize()
        {
            _mainWindowViewModel = DataContext as MainWindowViewModel;

            // TODO: temporary code to be removed
            _mainWindowViewModel.Client.OnPlayerRegistered += OnPlayerRegistered;
            _mainWindowViewModel.Client.OnGameStarted += OnGameStarted;
            _mainWindowViewModel.Client.OnGameFinished += OnGameFinished;
            _mainWindowViewModel.Client.OnGameOver += OnGameOver;
            _mainWindowViewModel.Client.OnConnectionLost += OnConnectionLost;

            //ConnectionView.Client = _mainWindowViewModel.Client;
            //OptionsView.Client = _mainWindowViewModel.Client;
            //WinListView.Client = _mainWindowViewModel.Client;
            //PartyLineView.Client = _mainWindowViewModel.Client;
            GameView.Client = _mainWindowViewModel.Client;
            //ClientStatisticsView.Client = _mainWindowViewModel.Client;
        }

        #region IClient events handler
        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded && Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                ExecuteOnUIThread.Invoke(() =>
                {
                    if (TabConnect.IsSelected)
                        TabPartyLine.IsSelected = true;
                });
            if (succeeded && _mainWindowViewModel.Client.IsServerMaster)
                _mainWindowViewModel.Client.ChangeOptions(Options.OptionsSingleton.Instance.ServerOptions);
        }

        private void OnGameStarted()
        {
            if (Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ExecuteOnUIThread.Invoke(() =>
                {
                    TabGameView.IsSelected = true;
                });
        }

        private void OnGameFinished()
        {
            if (Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                ExecuteOnUIThread.Invoke(() =>
                {
                    if (TabGameView.IsSelected)
                        TabPartyLine.IsSelected = true;
                });
            }
        }

        private void OnGameOver()
        {
            if (Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                ExecuteOnUIThread.Invoke(() =>
                {
                    if (TabGameView.IsSelected)
                        TabPartyLine.IsSelected = true;
                });
            }
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                TabConnect.IsSelected = true;
            });
        }
        #endregion

        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
