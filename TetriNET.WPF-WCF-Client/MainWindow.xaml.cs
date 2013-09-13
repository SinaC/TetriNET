using System;
using System.Configuration;
using System.Linq;
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
        private readonly IClient _client;

        public MainWindow()
        {
            string logFilename = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5)+".log";
            Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);

            ExecuteOnUIThread.Initialize();

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            Log.WriteLine(Log.LogLevels.Info, "Local user config path: {0}", config.FilePath);

            _client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(12, 22));
            // Get default options
            Options.OptionsSingleton.Instance.ServerOptions = Settings.Default.GameOptions ?? _client.Options;
            // TODO: fix this bug  ---- Workaround: remove duplicate key
            Options.OptionsSingleton.Instance.ServerOptions.SpecialOccurancies = Options.OptionsSingleton.Instance.ServerOptions.SpecialOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();
            Options.OptionsSingleton.Instance.ServerOptions.TetriminoOccurancies = Options.OptionsSingleton.Instance.ServerOptions.TetriminoOccurancies.GroupBy(x => x.Value).Select(x => x.First()).ToList();

            Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered = Settings.Default.AutomaticallySwitchToPartyLineOnRegistered;
            Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted = Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted;

            InitializeComponent();

            _client.OnPlayerRegistered += OnPlayerRegistered;
            _client.OnGameStarted += OnGameStarted;
            _client.OnGameFinished += OnGameFinished;
            _client.OnGameOver += OnGameOver;
            _client.OnConnectionLost += OnConnectionLost;

            ConnectionView.Client = _client;
            OptionsView.Client = _client;
            WinListView.Client = _client;
            PartyLineView.Client = _client;
            GameView.Client = _client;
            ClientStatisticsView.Client = _client;
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
            if (succeeded && _client.IsServerMaster)
                _client.ChangeOptions(Options.OptionsSingleton.Instance.ServerOptions);
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
