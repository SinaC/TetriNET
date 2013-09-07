using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.AI;
using TetriNET.WPF_WCF_Client.Controls;
using TetriNET.WPF_WCF_Client.GameController;

namespace TetriNET.WPF_WCF_Client.Views
{
    /// <summary>
    /// Interaction logic for GameView.xaml
    /// </summary>
    public partial class GameView : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("GameViewClientProperty", typeof(IClient), typeof(GameView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
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
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnPlayerJoined += _this.OnPlayerJoined;
                    newClient.OnPlayerLeft += _this.OnPlayerLeft;

                    _this._controller = new GameController.GameController(newClient);
                    _this._bot = new PierreDellacherieOnePieceBot(newClient);
                }
            }
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                _playerId = playerId;
                PlayerGrid.Client = Client;
                PlayerGrid.PlayerName = "JOEL";
                PlayerGrid.PlayerId = playerId;
                Inventory.Client = Client;
                InGameMessages.Client = Client;
                NextTetrimino.Client = Client;
                Info.Client = Client;
            }
            else
            {

                _playerId = -1;
                PlayerGrid.Client = null;
                PlayerGrid.PlayerId = -1;
            }
        }

        private void OnPlayerLeft(int playerId, string playerName)
        {
            if (playerId != _playerId)
            {
                OpponentGridCanvas grid = GetOpponentGrid(playerId);
                grid.Client = null;
                grid.PlayerId = -1;
                grid.PlayerName = "Not playing";
            }
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            if (playerId != _playerId)
            {
                OpponentGridCanvas grid = GetOpponentGrid(playerId);
                if (grid.PlayerId == -1)
                {
                    grid.PlayerId = playerId;
                    grid.PlayerName = playerName;
                    grid.Client = Client;
                }
                else
                {
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Error, "Trying to reassign an opponent {0} {1} to grid {2}", playerId, playerName, grid.PlayerId);
                }
            }
        }

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

        private void GameView_KeyUp(object sender, KeyEventArgs e)
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

        private OpponentGridCanvas GetOpponentGrid(int playerId)
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
