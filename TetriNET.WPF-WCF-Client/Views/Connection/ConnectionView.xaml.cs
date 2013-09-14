using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Views.Connection
{
    /// <summary>
    /// Interaction logic for ConnectionView.xaml
    /// </summary>
    public partial class ConnectionView : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("ConnectionViewClientProperty", typeof(IClient), typeof(ConnectionView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public ConnectionView()
        {
            InitializeComponent();

            ServerList.OnServerSelected += OnServerSelected;
        }

        private void OnServerSelected(object sender, string serverAddress)
        {
            Connection.ServerAddress = serverAddress;
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ConnectionView _this = sender as ConnectionView;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                _this.Connection.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                }
            }
        }
    }
}
