using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.AI;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GameController.GameController _controller;

        public GenericBot Bot { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Title = String.Format("TetriNET {0}.{1}", version.Major, version.Minor);

            ////
            //WindowStyle = WindowStyle.ToolWindow;
            //Background = new SolidColorBrush(Colors.White);
            //AllowsTransparency = false;
            //CloseButton.Visibility = Visibility.Collapsed;
            //MinimizeButton.Visibility = Visibility.Collapsed;


            if (!DesignMode.IsInDesignModeStatic)
            {
                MainWindowViewModel vm = new MainWindowViewModel();
                DataContext = vm;

                vm.ClientChanged += OnClientChanged;

                if (vm.Client != null)
                    OnClientChanged(null, vm.Client);
            }
        }

        #region UI Events
        
        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GameView_KeyDown(object sender, KeyEventArgs e)
        {
            MainWindowViewModel vm = DataContext as MainWindowViewModel;
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
            else if (e.Key == Key.P)
            {
                if (vm != null)
                    vm.Client.PauseGame();
            }
            else if (e.Key == Key.R)
            {
                if (vm != null)
                    vm.Client.ResumeGame();
            }
            else if (e.Key == Key.A && Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift))
            {
                Bot.Activated = !Bot.Activated;
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
                Commands cmd = MapKeyToCommand(e.Key);
                if (cmd != Commands.Invalid)
                    _controller.KeyDown(cmd);
            }
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
        }

        private void GameView_KeyUp(object sender, KeyEventArgs e)
        {
            Commands cmd = MapKeyToCommand(e.Key);
            if (cmd != Commands.Invalid)
                _controller.KeyUp(cmd);
            if (e.Key == Key.Tab)
            {
                e.Handled = true;
            }
        }

        #endregion

        #region IClient events handler

        private void OnGameStarted()
        {
            if (ClientOptionsViewModel.Instance.DropSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Commands.Drop);
            else
                _controller.AddSensibility(Commands.Drop, ClientOptionsViewModel.Instance.DropSensibilityViewModel.Value);
            if (ClientOptionsViewModel.Instance.DownSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Commands.Down);
            else
                _controller.AddSensibility(Commands.Down, ClientOptionsViewModel.Instance.DownSensibilityViewModel.Value);
            if (ClientOptionsViewModel.Instance.LeftSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Commands.Left);
            else
                _controller.AddSensibility(Commands.Left, ClientOptionsViewModel.Instance.LeftSensibilityViewModel.Value);
            if (ClientOptionsViewModel.Instance.RightSensibilityViewModel.IsActivated)
                _controller.RemoveSensibility(Commands.Right);
            else
                _controller.AddSensibility(Commands.Right, ClientOptionsViewModel.Instance.RightSensibilityViewModel.Value);
        }

        #endregion

        #region Commands

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            // Remove old handlers
            if (oldClient != null)
            {
                oldClient.GameStarted -= OnGameStarted;

                _controller.Do(x => x.UnsubscribeFromClientEvents());
                Bot.Do(x => x.UnsubscribeFromClientEvents());
            }
            
            // Add new handlers
            if (newClient != null)
            {
                newClient.GameStarted += OnGameStarted;
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
        
        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion

        private static Commands MapKeyToCommand(Key key)
        {
            KeySettingViewModel keySetting = ClientOptionsViewModel.Instance.KeySettings.FirstOrDefault(x => x.Key == key);
            if (keySetting != null)
                return keySetting.Command;
            switch (key)
            {
                case Key.H:
                    return Commands.Hold;
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
                case Key.Enter:
                    return Commands.UseSpecialOn6;
            }
            return Commands.Invalid;
        }

    }
}
