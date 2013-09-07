using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.AI;
using TetriNET.WPF_WCF_Client.Controls;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Views;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            string name = "WPF_" + Guid.NewGuid().ToString().Substring(0, 5);

            Logger.Log.Initialize(@"D:\TEMP\LOG\", name+".log");
            ExecuteOnUIThread.Initialize();

            InitializeComponent();

            IClient client = new Client.Client(Tetrimino.CreateTetrimino, () => new Board(12, 22));

            string baseAddress = ConfigurationManager.AppSettings["address"];
            client.Connect(callback => new WCFProxy.WCFProxy(callback, baseAddress));

            client.OnGameStarted += OnGameStarted;

            GameView.Client = client;
            PartyLine.Client = client;

            client.Register(name);
        }

        private void OnGameStarted()
        {
            TabGameView.IsSelected = true; // TODO: only if option 'swap to play fields on game started' is checked (local options)
        }
    }
}
