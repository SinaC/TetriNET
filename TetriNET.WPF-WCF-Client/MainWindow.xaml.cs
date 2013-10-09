using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

            Version version = Assembly.GetEntryAssembly().GetName().Version;
            Title = String.Format("TetriNET {0}.{1}", version.Major, version.Minor);

            ////
            //WindowStyle = WindowStyle.ToolWindow;
            //Background = new SolidColorBrush(Colors.White);
            //AllowsTransparency = false;
            //CloseButton.Visibility = Visibility.Collapsed;
            //MinimizeButton.Visibility = Visibility.Collapsed;


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
    }
}
