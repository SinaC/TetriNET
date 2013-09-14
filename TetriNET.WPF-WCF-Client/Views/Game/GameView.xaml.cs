using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.AI;
using TetriNET.WPF_WCF_Client.GameController;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.Game
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("GameViewClientProperty", typeof (IClient), typeof (GameView), new PropertyMetadata(Client_Changed));

        public IClient Client
        {
            get { return (IClient) GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private PierreDellacherieOnePieceBot _bot;
        private GameController.GameController _controller;
        private int _playerId;

        public GameView()
        {
            InitializeComponent();

            //http://stackoverflow.com/questions/15241118/keydown-event-not-raising-from-a-grid
            Focusable = true; // This is needed to catch KeyUp/KeyDown
            Loaded += (sender, args) => Focus(); // This is needed to catch KeyUp/KeyDown
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            GameView _this = sender as GameView;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnPlayerJoined -= _this.OnPlayerJoined;
                    oldClient.OnPlayerLeft -= _this.OnPlayerLeft;

                    // TODO: unregister old GameController+Bot from old client events
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                _this.Inventory.Client = newClient;
                _this.InGameMessages.Client = newClient;
                _this.NextTetrimino.Client = newClient;
                _this.GameInfo.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnPlayerJoined += _this.OnPlayerJoined;
                    newClient.OnPlayerLeft += _this.OnPlayerLeft;

                    _this._controller = new GameController.GameController(newClient);
                    _this._bot = new PierreDellacherieOnePieceBot(newClient, _this._controller);
                }
            }
        }

        #region IClient events handler
        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                _playerId = playerId;
                ExecuteOnUIThread.Invoke(() =>
                {
                    PlayerGrid.Client = Client;
                });
            }
            else
            {
                _playerId = -1;
                ExecuteOnUIThread.Invoke(() =>
                {
                    PlayerGrid.Client = null;
                });
                PlayerGrid.PlayerId = -1;
                PlayerGrid.PlayerName = "Not registered";
            }
        }

        private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        {
            if (playerId != _playerId)
            {
                OpponentGridControl grid = GetOpponentGrid(playerId);
                ExecuteOnUIThread.Invoke(() =>
                {
                    grid.Client = null;
                });
                grid.PlayerId = -1;
                grid.PlayerName = "Not playing";
            }
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            if (playerId != _playerId)
            {
                OpponentGridControl grid = GetOpponentGrid(playerId);
                if (grid.PlayerId == -1)
                {
                    ExecuteOnUIThread.Invoke(() =>
                    {
                        grid.Client = Client;
                    });
                    grid.PlayerId = playerId;
                    grid.PlayerName = playerName;
                }
                else
                {
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Error, "Trying to reassign an opponent {0} {1} to grid {2}", playerId, playerName, grid.PlayerId);
                }
            }
        }
        #endregion

        #region UI events handler
        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                Client.StartGame();
            }
            else if (e.Key == Key.T)
            {
                Client.StopGame();
            }
            else if (e.Key == Key.A && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) )
            {
                _bot.Activated = !_bot.Activated;
            }
            else if (e.Key == Key.H && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                PlayerGrid.ToggleHint();
            }
            else if (e.Key == Key.Add)
            {
                _bot.SleepTime += 100;
            }
            else if (e.Key == Key.Subtract)
            {
                _bot.SleepTime -= 100;
            }
            else
            {
                Commands cmd = MapKeyToCommand(e.Key);
                if (cmd != Commands.Invalid)
                    _controller.KeyDown(cmd);
            }
        }

        private void GameView_KeyUp(object sender, KeyEventArgs e)
        {
            Commands cmd = MapKeyToCommand(e.Key);
            if (cmd != Commands.Invalid)
                _controller.KeyUp(cmd);
        }
        #endregion

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
                    return Commands.RotateCounterclockwise;
                case Key.PageDown:
                    return Commands.RotateClockwise;
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

        private OpponentGridControl GetOpponentGrid(int playerId)
        {
            if (playerId == _playerId)
                return null;
            // playerId -> id mapping rule
            // 0 1 [2] 3 4 5 -> 1 2 / 3 4 5
            // [0] 1 2 3 4 5 -> / 1 2 3 4 5 
            // 0 1 2 3 4 [5] -> 1 2 3 4 5 /
            int id;
            if (playerId < _playerId)
                id = playerId + 1;
            else
                id = playerId;
            if (id == 1)
                return OpponentGrid1;
            if (id == 2)
                return OpponentGrid2;
            if (id == 3)
                return OpponentGrid3;
            if (id == 4)
                return OpponentGrid4;
            if (id == 5)
                return OpponentGrid5;
            return null;
        }
    }
}