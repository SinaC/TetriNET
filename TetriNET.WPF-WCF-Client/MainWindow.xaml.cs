using System.ComponentModel;
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

            if (!DesignerProperties.GetIsInDesignMode(this))
                DataContext = new MainWindowViewModel();
        }

        #region UI Events
        
        private void MainWindow_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #endregion

        #region Commands
        
        private void CloseCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        private void TabControl_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // TODO:
            e.Handled = false;
        }
    }
}
