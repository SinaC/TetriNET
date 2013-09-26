using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void OnGameStarted()
        {
            if (ClientOptionsViewModel.Instance.DropSensibilityActivated)
                _controller.RemoveSensibility(Common.Interfaces.Commands.Drop);
            else
                _controller.AddSensibility(Common.Interfaces.Commands.Drop, ClientOptionsViewModel.Instance.DropSensibility);
            if (ClientOptionsViewModel.Instance.DownSensibilityActivated)
                _controller.RemoveSensibility(Common.Interfaces.Commands.Down);
            else
                _controller.AddSensibility(Common.Interfaces.Commands.Down, ClientOptionsViewModel.Instance.DownSensibility);
            if (ClientOptionsViewModel.Instance.LeftSensibilityActivated)
                _controller.RemoveSensibility(Common.Interfaces.Commands.Left);
            else
                _controller.AddSensibility(Common.Interfaces.Commands.Left, ClientOptionsViewModel.Instance.LeftSensibility);
            if (ClientOptionsViewModel.Instance.RightSensibilityActivated)
                _controller.RemoveSensibility(Common.Interfaces.Commands.Right);
            else
                _controller.AddSensibility(Common.Interfaces.Commands.Right, ClientOptionsViewModel.Instance.RightSensibility);
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
                Common.Interfaces.Commands cmd = MapKeyToCommand(e.Key);
                if (cmd != Common.Interfaces.Commands.Invalid)
                    _controller.KeyDown(cmd);
            }
        }

        private void GameView_KeyUp(object sender, KeyEventArgs e)
        {
            Common.Interfaces.Commands cmd = MapKeyToCommand(e.Key);
            if (cmd != Common.Interfaces.Commands.Invalid)
                _controller.KeyUp(cmd);
        }
        
        #endregion

        private Common.Interfaces.Commands MapKeyToCommand(Key key)
        {
            KeySetting keySetting = ClientOptionsViewModel.Instance.KeySettings.FirstOrDefault(x => x.Key == key);
            if (keySetting != null)
                return keySetting.Command;
            switch (key)
            {
                case Key.Space:
                    return Common.Interfaces.Commands.Drop;
                case Key.Down:
                    return Common.Interfaces.Commands.Down;
                case Key.Left:
                    return Common.Interfaces.Commands.Left;
                case Key.Right:
                    return Common.Interfaces.Commands.Right;
                case Key.Up:
                    return Common.Interfaces.Commands.RotateCounterclockwise;
                case Key.PageDown:
                    return Common.Interfaces.Commands.RotateClockwise;
                case Key.D:
                    return Common.Interfaces.Commands.DiscardFirstSpecial;
                case Key.NumPad1:
                case Key.D1:
                    return Common.Interfaces.Commands.UseSpecialOn1;
                case Key.NumPad2:
                case Key.D2:
                    return Common.Interfaces.Commands.UseSpecialOn2;
                case Key.NumPad3:
                case Key.D3:
                    return Common.Interfaces.Commands.UseSpecialOn3;
                case Key.NumPad4:
                case Key.D4:
                    return Common.Interfaces.Commands.UseSpecialOn4;
                case Key.NumPad5:
                case Key.D5:
                    return Common.Interfaces.Commands.UseSpecialOn5;
                case Key.NumPad6:
                case Key.D6:
                    return Common.Interfaces.Commands.UseSpecialOn6;
            }
            return Common.Interfaces.Commands.Invalid;
        }

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
                oldClient.OnGameStarted -= OnGameStarted;

                if (_controller != null)
                    _controller.UnsubscribeFromClientEvents();
                if (_bot != null)
                    _bot.UnsubscribeFromClientEvents();
            }
            // Set new client
            Inventory.Client = newClient;
            NextPiece.Client = newClient;
            // Add new handlers
            if (newClient != null)
            {
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