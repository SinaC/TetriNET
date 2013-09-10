using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Annotations;

namespace TetriNET.WPF_WCF_Client.Views
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("OptionsClientProperty", typeof(IClient), typeof(OptionsView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private bool _isServerMaster;
        public bool IsServerMaster
        {
            get { return _isServerMaster; }
            set
            {
                if (_isServerMaster != value)
                {
                    _isServerMaster = value;
                    OnPropertyChanged();
                }
            }
        }

        public Models.Options Options
        {
            get { return Models.Options.OptionsSingleton.Instance; }
        }

        public OptionsView()
        {
            InitializeComponent();
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            OptionsView _this = sender as OptionsView;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnServerMasterModified -= _this.OnServerMasterModified;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnServerMasterModified += _this.OnServerMasterModified;
                }
            }
        }

        private void OnServerMasterModified(int serverMaster)
        {
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnGameStarted()
        {
            Models.Options.OptionsSingleton.Instance.ServerOptions = Client.Options;
            OnPropertyChanged("Options");
        }

        private void SendOptionsToServer_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: should be enabled only when an option has been modified
            Client.ChangeOptions(Models.Options.OptionsSingleton.Instance.ServerOptions);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
