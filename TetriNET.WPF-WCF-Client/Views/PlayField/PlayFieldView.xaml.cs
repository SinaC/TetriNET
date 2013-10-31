using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
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
        private GameController.GameController _controller;

        public GenericBot Bot { get; private set; }

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
            if (ClientOptionsViewModel.Instance.DropSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Client.Interfaces.Commands.Drop);
            else
                _controller.AddSensibility(Client.Interfaces.Commands.Drop, ClientOptionsViewModel.Instance.DropSensibilityViewModel.Value);
            if (ClientOptionsViewModel.Instance.DownSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Client.Interfaces.Commands.Down);
            else
                _controller.AddSensibility(Client.Interfaces.Commands.Down, ClientOptionsViewModel.Instance.DownSensibilityViewModel.Value);
            if (ClientOptionsViewModel.Instance.LeftSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Client.Interfaces.Commands.Left);
            else
                _controller.AddSensibility(Client.Interfaces.Commands.Left, ClientOptionsViewModel.Instance.LeftSensibilityViewModel.Value);
            if (ClientOptionsViewModel.Instance.RightSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Client.Interfaces.Commands.Right);
            else
                _controller.AddSensibility(Client.Interfaces.Commands.Right, ClientOptionsViewModel.Instance.RightSensibilityViewModel.Value);
        }

        #endregion

        #region UI events handler

        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            PlayFieldViewModel vm = DataContext as PlayFieldViewModel;
            if (e.Key == Key.S)
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
                Bot.Activated = !Bot.Activated;
            }
            else if (e.Key == Key.H && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                PlayerGrid.ToggleHint();
                Inventory.ToggleHint();
            }
            else if (e.Key == Key.D && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                PlayerGrid.ToggleDropLocation();
            }
            else if (e.Key == Key.Add)
            {
                Bot.SleepTime += 100;
            }
            else if (e.Key == Key.Subtract)
            {
                Bot.SleepTime -= 100;
            }
            else
            {
                Client.Interfaces.Commands cmd = MapKeyToCommand(e.Key);
                if (cmd != Client.Interfaces.Commands.Invalid)
                    _controller.KeyDown(cmd);
            }
        }

        private void GameView_KeyUp(object sender, KeyEventArgs e)
        {
            Client.Interfaces.Commands cmd = MapKeyToCommand(e.Key);
            if (cmd != Client.Interfaces.Commands.Invalid)
                _controller.KeyUp(cmd);
        }
        
        #endregion

        private Client.Interfaces.Commands MapKeyToCommand(Key key)
        {
            KeySettingViewModel keySetting = ClientOptionsViewModel.Instance.KeySettings.FirstOrDefault(x => x.Key == key);
            if (keySetting != null)
                return keySetting.Command;
            switch (key)
            {
                case Key.H:
                    return Client.Interfaces.Commands.Hold;
                case Key.Space:
                    return Client.Interfaces.Commands.Drop;
                case Key.Down:
                    return Client.Interfaces.Commands.Down;
                case Key.Left:
                    return Client.Interfaces.Commands.Left;
                case Key.Right:
                    return Client.Interfaces.Commands.Right;
                case Key.Up:
                    return Client.Interfaces.Commands.RotateCounterclockwise;
                case Key.PageDown:
                    return Client.Interfaces.Commands.RotateClockwise;
                case Key.D:
                    return Client.Interfaces.Commands.DiscardFirstSpecial;
                case Key.NumPad1:
                case Key.D1:
                    return Client.Interfaces.Commands.UseSpecialOn1;
                case Key.NumPad2:
                case Key.D2:
                    return Client.Interfaces.Commands.UseSpecialOn2;
                case Key.NumPad3:
                case Key.D3:
                    return Client.Interfaces.Commands.UseSpecialOn3;
                case Key.NumPad4:
                case Key.D4:
                    return Client.Interfaces.Commands.UseSpecialOn4;
                case Key.NumPad5:
                case Key.D5:
                    return Client.Interfaces.Commands.UseSpecialOn5;
                case Key.NumPad6:
                case Key.D6:
                    return Client.Interfaces.Commands.UseSpecialOn6;
                case Key.Enter:
                    return Client.Interfaces.Commands.UseSpecialOn6;
            }
            return Client.Interfaces.Commands.Invalid;
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
                if (Bot != null)
                    Bot.UnsubscribeFromClientEvents();
            }
            // Set new client
            Inventory.Client = newClient;
            NextPiece.Client = newClient;
            HoldPiece.Client = newClient;
            // Add new handlers
            if (newClient != null)
            {
                newClient.OnGameStarted += OnGameStarted;

                // And create controller + bot
                _controller = new GameController.GameController(newClient);
                //Bot = new GenericBot(newClient, new LuckyToiletOnePiece(), null);
                Bot = new GenericBot(newClient, new AdvancedPierreDellacherieOnePiece(), new SinaCSpecials());
                //Bot = new GenericBot(newClient, new ColinFaheyTwoPiece(), new SinaCSpecials());
            }
            else
            {
                _controller = null;
                Bot = null;
            }
        }
    }
}