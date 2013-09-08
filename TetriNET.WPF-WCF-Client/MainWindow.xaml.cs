using System;
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
            string logFile = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5)+".log";

            Logger.Log.Initialize(@"D:\TEMP\LOG\", logFile);
            ExecuteOnUIThread.Initialize();

            InitializeComponent();

            IClient client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(12, 22));

            client.OnGameStarted += OnGameStarted;
            client.OnGameFinished += OnGameFinished;
            client.OnGameOver += OnGameOver;

            ConnectionView.Client = client;
            OptionsView.Client = client;
            PartyLineView.Client = client;
            GameView.Client = client;
        }

        private void OnGameStarted()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayField)
                ExecuteOnUIThread.Invoke(() =>
                {
                    TabGameView.IsSelected = true;
                });
        }

        private void OnGameFinished()
        {
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayField)
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
            if (Models.Options.OptionsSingleton.Instance.AutomaticallySwitchToPlayField)
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
