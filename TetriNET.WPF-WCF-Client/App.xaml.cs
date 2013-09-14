using System.Windows;
using TetriNET.WPF_WCF_Client.ViewModels;

namespace TetriNET.WPF_WCF_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindowViewModel vm = new MainWindowViewModel();
            MainWindow window = new MainWindow
            {
                DataContext = vm
            };
            window.Initialize();
            window.Show();
        }
    }
}
