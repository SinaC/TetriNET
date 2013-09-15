using System.Windows;
using System.Windows.Input;
using TetriNET.WPF_WCF_Client.ViewModels;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // TODO: temporary code to be removed
        public void Initialize()
        {
            // TODO: temporary code to be removed
            PlayFieldView.PlayFieldViewModel = (DataContext as MainWindowViewModel).PlayFieldViewModel;
        }

        #region UI Events
        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        #endregion

        #region Commands
        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}
