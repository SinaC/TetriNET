using System.Configuration;
using System.Windows;
using System.Windows.Input;
using TetriNET.Client.DefaultBoardAndTetriminos;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.AI;
using TetriNET.WPF_WCF_Client.GameController;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for MainWindow2.xaml
    /// </summary>
    public partial class MainWindow2 : Window
    {
        private IClient _client;
        private GameController.GameController _controller;
        private PierreDellacherieOnePieceBot _bot;
        private int _playerId;

        public MainWindow2()
        {
            Logger.Log.Initialize(@"D:\TEMP\LOG\", "WPF-client-window2.log");
            ExecuteOnUIThread.Initialize();

            InitializeComponent();

            KeyDown += Window_KeyDown;
            KeyUp += Window_KeyUp;

            _client = new Client.Client(CreateTetrimino, () => new Board(12, 22));

            string baseAddress = ConfigurationManager.AppSettings["address"];
            _client.Connect(callback => new WCFProxy.WCFProxy(callback, baseAddress));
            _client.OnPlayerRegistered += OnPlayerRegistered;

            _client.Register("JOEL");

            _controller = new GameController.GameController(_client);
            _bot = new PierreDellacherieOnePieceBot(_client)
            {
                Activated = false,
                SleepTime = 250
            };
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                _playerId = playerId;
                PlayerGrid.Client = _client;
                PlayerGrid.PlayerId = playerId;
            }
            else
            {

                _playerId = -1;
                PlayerGrid.Client = null;
                PlayerGrid.PlayerId = -1;
            }
        }

        //Pass Key-Events to the Game
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                _client.StartGame();
            }
            else if (e.Key == Key.T)
            {
                _client.StopGame();
            }
            else if (e.Key == Key.A)
            {
                _bot.Activated = !_bot.Activated;
            }
            else
            {
                Commands cmd = MapKeyToCommand(e.Key);
                if (cmd != Commands.Invalid)
                    _controller.KeyDown(cmd);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            Commands cmd = MapKeyToCommand(e.Key);
            if (cmd != Commands.Invalid)
                _controller.KeyUp(cmd);
        }

        private static Commands MapKeyToCommand(Key key)
        {
            switch (key)
            {
                case Key.Space:
                    return Commands.Drop;
                case Key.Down:
                    return Commands.Down;
                case Key.Left:
                    return Commands.Left;
                case Key.Right:
                    return Commands.Right;
                case Key.Up:
                    return Commands.RotateClockwise;
                case Key.PageDown:
                    return Commands.RotateCounterclockwise;
                case Key.D:
                    return Commands.DiscardFirstSpecial;
                case Key.NumPad1:
                case Key.D1:
                    return Commands.UseSpecialOn1;
                case Key.NumPad2:
                case Key.D2:
                    return Commands.UseSpecialOn2;
                case Key.NumPad3:
                case Key.D3:
                    return Commands.UseSpecialOn3;
                case Key.NumPad4:
                case Key.D4:
                    return Commands.UseSpecialOn4;
                case Key.NumPad5:
                case Key.D5:
                    return Commands.UseSpecialOn5;
                case Key.NumPad6:
                case Key.D6:
                    return Commands.UseSpecialOn6;
            }
            return Commands.Invalid;
        }

        private static ITetrimino CreateTetrimino(Tetriminos tetrimino, int spawnX, int spawnY, int spawnOrientation)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return new TetriminoI(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoJ:
                    return new TetriminoJ(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoL:
                    return new TetriminoL(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoO:
                    return new TetriminoO(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoS:
                    return new TetriminoS(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoT:
                    return new TetriminoT(spawnX, spawnY, spawnOrientation);
                case Tetriminos.TetriminoZ:
                    return new TetriminoZ(spawnX, spawnY, spawnOrientation);
            }
            return new TetriminoZ(spawnX, spawnY, spawnOrientation); // TODO: sometimes server takes time to send next tetrimino, it should send 2 or 3 next tetriminoes to ensure this never happens
            //return new TetriminoI(spawnX, spawnY, 2);
        }
    }
}
