using System.Windows.Input;

namespace Tetris.Model.UI
{
    public static class Commands
    {
        public static readonly RoutedCommand StartGame = new RoutedCommand();
        public static readonly RoutedCommand QuitApplication = new RoutedCommand();
        public static readonly RoutedCommand QuitGame = new RoutedCommand();
        public static readonly RoutedCommand PauseGame = new RoutedCommand();
        public static readonly RoutedCommand ResumeGame = new RoutedCommand();
        public static readonly RoutedCommand EnterSettings = new RoutedCommand();
        public static readonly RoutedCommand EnterScores = new RoutedCommand();
        public static readonly RoutedCommand EnterCredits = new RoutedCommand();
    }
}
