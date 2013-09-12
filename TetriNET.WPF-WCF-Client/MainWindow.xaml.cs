using System;
using System.Configuration;
using System.Windows;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string logFilename = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5)+".log";
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);

            ExecuteOnUIThread.Initialize();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Log.WriteLine(Log.LogLevels.Info, "Local user config path: {0}", config.FilePath);

            IClient client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(12, 22));
            // Get default options
            Options.OptionsSingleton.Instance.ServerOptions = client.Options;
            Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered = Settings.Default.AutomaticallySwitchToPartyLineOnRegistered;
            Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted = Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted;

            InitializeComponent();

            client.OnPlayerRegistered += OnPlayerRegistered;
            client.OnGameStarted += OnGameStarted;
            client.OnGameFinished += OnGameFinished;
            client.OnGameOver += OnGameOver;
            client.OnConnectionLost += OnConnectionLost;

            ConnectionView.Client = client;
            OptionsView.Client = client;
            WinListView.Client = client;
            PartyLineView.Client = client;
            GameView.Client = client;
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
    }
}
