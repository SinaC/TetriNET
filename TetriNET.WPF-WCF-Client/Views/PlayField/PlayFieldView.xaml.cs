using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Strategy;
using TetriNET.WPF_WCF_Client.AI;
using TetriNET.WPF_WCF_Client.ViewModels.Options;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for PlayFieldView.xaml
    /// </summary>
    public partial class PlayFieldView : UserControl
    {
        private GenericBot _bot;
        private GameController.GameController _controller;
        //private int _playerId;

        public PlayFieldView()
        {
            InitializeComponent();

            //http://stackoverflow.com/questions/15241118/keydown-event-not-raising-from-a-grid
            Focusable = true; // This is needed to catch KeyUp/KeyDown
            Loaded += (sender, args) => Focus(); // This is needed to catch KeyUp/KeyDown
        }

        #region IClient events handler

        //private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        //{
        //    OpponentGridControl grid = GetOpponentGrid(playerId);
        //    if (grid != null)
        //    {
        //        grid.PlayerId = -1;
        //        grid.PlayerName = "Not playing";
        //    }
        //}

        //private void OnPlayerJoined(int playerId, string playerName)
        //{
        //    OpponentGridControl grid = GetOpponentGrid(playerId);
        //    if (grid != null)
        //    {
        //        grid.PlayerId = playerId;
        //        grid.PlayerName = playerName;
        //    }
        //}

        //private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        //{
        //    if (result == RegistrationResults.RegistrationSuccessful)
        //        _playerId = playerId;
        //    else
        //        _playerId = -1;
        //}

        private void OnGameStarted()
        {
            if (ClientOptionsViewModel.Instance.DropSensibilityActivated)
                _controller.RemoveSensibility(Commands.Drop);
            else
                _controller.AddSensibility(Commands.Drop, ClientOptionsViewModel.Instance.DropSensibility);
            if (ClientOptionsViewModel.Instance.DownSensibilityActivated)
                _controller.RemoveSensibility(Commands.Down);
            else
                _controller.AddSensibility(Commands.Down, ClientOptionsViewModel.Instance.DownSensibility);
            if (ClientOptionsViewModel.Instance.LeftSensibilityActivated)
                _controller.RemoveSensibility(Commands.Left);
            else
                _controller.AddSensibility(Commands.Left, ClientOptionsViewModel.Instance.LeftSensibility);
            if (ClientOptionsViewModel.Instance.RightSensibilityActivated)
                _controller.RemoveSensibility(Commands.Right);
            else
                _controller.AddSensibility(Commands.Down, ClientOptionsViewModel.Instance.RightSensibility);
        }

        #endregion

        #region UI events handler

        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            PlayFieldViewModel vm = DataContext as PlayFieldViewModel;
            if (e.Key == Key.S )
            {
                if (vm != null)
                    vm.Client.StartGame();
            }
            else if (e.Key == Key.T)
            {
                if (vm != null)
                    vm.Client.StopGame();
            }
            else if (e.Key == Key.A && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) )
            {
                _bot.Activated = !_bot.Activated;
            }
            else if (e.Key == Key.H && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                PlayerGrid.ToggleHint();
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                PlayerGrid.ToggleDropLocation();
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

        private Commands MapKeyToCommand(Key key)
        {
            KeySetting keySetting = ClientOptionsViewModel.Instance.KeySettings.FirstOrDefault(x => x.Key == key);
            if (keySetting != null)
                return keySetting.Command;
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

        //private OpponentGridControl GetOpponentGrid(int playerId)
        //{
        //    if (playerId == _playerId)
        //        return null;
        //    // playerId -> id mapping rule
        //    // 0 1 [2] 3 4 5 -> 1 2 / 3 4 5
        //    // [0] 1 2 3 4 5 -> / 1 2 3 4 5 
        //    // 0 1 2 3 4 [5] -> 1 2 3 4 5 /
        //    int id;
        //    if (playerId < _playerId)
        //        id = playerId + 1;
        //    else
        //        id = playerId;
        //    if (id == 1)
        //        return OpponentGrid1;
        //    if (id == 2)
        //        return OpponentGrid2;
        //    if (id == 3)
        //        return OpponentGrid3;
        //    if (id == 4)
        //        return OpponentGrid4;
        //    if (id == 5)
        //        return OpponentGrid5;
        //    return null;
        //}

        private void PlayFieldView_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Crappy workaround MVVM -> code behind
            PlayFieldViewModel vm = DataContext as PlayFieldViewModel;
            if (vm != null)
            {
                vm.ClientChanged += OnClientChanged;

                if (vm.Client != null)
                    OnClientChanged(null, vm.Client);
            }
        }

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            // Remove old handlers
            if (oldClient != null)
            {
                //oldClient.OnPlayerJoined -= OnPlayerJoined;
                //oldClient.OnPlayerLeft -= OnPlayerLeft;
                //oldClient.OnPlayerRegistered -= OnPlayerRegistered;
                oldClient.OnGameStarted -= OnGameStarted;

                if (_controller != null)
                    _controller.UnsubscribeFromClientEvents();
                if (_bot != null)
                    _bot.UnsubscribeFromClientEvents();
            }
            // Set new client
            Inventory.Client = newClient;
            NextPiece.Client = newClient;
            //PlayerGrid.Client = newClient;
            //OpponentGrid1.Client = newClient;
            //OpponentGrid2.Client = newClient;
            //OpponentGrid3.Client = newClient;
            //OpponentGrid4.Client = newClient;
            //OpponentGrid5.Client = newClient;
            // Add new handlers
            if (newClient != null)
            {
                //newClient.OnPlayerJoined += OnPlayerJoined;
                //newClient.OnPlayerLeft += OnPlayerLeft;
                //newClient.OnPlayerRegistered += OnPlayerRegistered;
                newClient.OnGameStarted += OnGameStarted;

                // And create controller + bot
                _controller = new GameController.GameController(newClient);
                //_bot = new GenericBot(newClient, new LuckyToiletOnePiece(), null);
                _bot = new GenericBot(newClient, new PierreDellacherieOnePiece(), new SinaCSpecials());
            }
            else
            {
                _controller = null;
                _bot = null;
            }
        }
    }
}