using System;
using System.Configuration;
using System.Windows;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

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

            Logger.Log.Initialize(ConfigurationManager.AppSettings["logpath"], logFilename);
            ExecuteOnUIThread.Initialize();

            InitializeComponent();

            IClient client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(12, 22));

            client.OnPlayerRegistered += OnPlayerRegistered;
            client.OnGameStarted += OnGameStarted;
            client.OnGameFinished += OnGameFinished;
            client.OnGameOver += OnGameOver;

            ConnectionView.Client = client;
            OptionsView.Client = client;
            WinListView.Client = client;
            PartyLineView.Client = client;
            GameView.Client = client;
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded && Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                ExecuteOnUIThread.Invoke(() =>
                {
                    TabPartyLine.IsSelected = true;
                });
        }

        private void OnGameStarted()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ExecuteOnUIThread.Invoke(() =>
                {
                    TabGameView.IsSelected = true;
                });
        }

        private void OnGameFinished()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
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
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                ExecuteOnUIThread.Invoke(() =>
                {
                    if (TabGameView.IsSelected)
                        TabPartyLine.IsSelected = true;
                });
            }
        }
    }
}
