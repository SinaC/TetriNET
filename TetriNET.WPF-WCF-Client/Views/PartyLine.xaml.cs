using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Views
{
    /// <summary>
    /// Interaction logic for PartyLine.xaml
    /// </summary>
    public partial class PartyLine : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("PartyLineClientProperty", typeof(IClient), typeof(PartyLine), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public PartyLine()
        {
            InitializeComponent();
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PartyLine _this = sender as PartyLine;

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
                _this.Chat.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                }
            }
        }
    }
}
