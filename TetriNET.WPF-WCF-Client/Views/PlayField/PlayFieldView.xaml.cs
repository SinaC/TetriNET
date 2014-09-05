using System.Windows;
using System.Windows.Controls;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for PlayFieldView.xaml
    /// </summary>
    public partial class PlayFieldView : UserControl
    {
        public PlayFieldView()
        {
            InitializeComponent();

            ////http://stackoverflow.com/questions/15241118/keydown-event-not-raising-from-a-grid
            //Loaded += (sender, args) =>
            //{
            //    Focusable = true; // This is needed to catch KeyUp/KeyDown
            //    Focus();
            //}; // This is needed to catch KeyUp/KeyDown
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
            // Set new client
            Inventory.Client = newClient;
            NextPiece.Client = newClient;
            HoldPiece.Client = newClient;
        }
    }
}