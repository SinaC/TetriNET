using System;
using System.ComponentModel;
using System.Configuration;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Controls;

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

        private bool _isGameNotStarted;
        public bool IsGameNotStarted
        {
            get { return _isGameNotStarted; }
            set
            {
                if (_isGameNotStarted != value)
                {
                    _isGameNotStarted = value;
                    OnPropertyChanged();
                }
            }
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

        private readonly Textures _textures;
        public Textures Textures { get { return _textures; } }

        public int TetriminosSum
        {
            get { return Common.Randomizer.RangeRandom.SumOccurancies(Options.ServerOptions.TetriminoOccurancies); }
        }

        public int SpecialsSum
        {
            get { return Common.Randomizer.RangeRandom.SumOccurancies(Options.ServerOptions.TetriminoOccurancies); }
        }

        public bool IsTetriminosSumValid
        {
            get { return TetriminosSum == 100; }
        }

        public bool IsSpecialsSumValid
        {
            get { return SpecialsSum == 100; }
        }

        public bool IsSendOptionsToServerEnabled
        {
            get { return IsTetriminosSumValid && IsSpecialsSumValid; }
        }

        public OptionsView()
        {
            InitializeComponent();

            IsGameNotStarted = true;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _textures = new Textures(new Uri(ConfigurationManager.AppSettings["texture"]));
            }
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
                    oldClient.OnGameFinished -= _this.OnGameFinished;
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnServerMasterModified -= _this.OnServerMasterModified;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameFinished += _this.OnGameFinished;
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnServerMasterModified += _this.OnServerMasterModified;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                }
            }
        }

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            IsServerMaster = false;
            IsGameNotStarted = true;
        }

        private void OnServerMasterModified(int serverMaster)
        {
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnGameStarted()
        {
            IsGameNotStarted = false;
            Models.Options.OptionsSingleton.Instance.ServerOptions = Client.Options;
            OnPropertyChanged("Options");
        }

        private void OnGameFinished()
        {
            IsGameNotStarted = true;
        }

        #endregion

        #region UI events handler
        private void SendOptionsToServer_OnClick(object sender, RoutedEventArgs e)
        {
            // TODO: should be enabled only when an option has been modified
            Client.ChangeOptions(Models.Options.OptionsSingleton.Instance.ServerOptions);
        }

        private void TetriminoOccurancy_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (!Validation.GetHasError(textBox))
                {
                    OnPropertyChanged("TetriminosSum");
                    OnPropertyChanged("IsTetriminosSumValid");
                    OnPropertyChanged("IsSendOptionsToServerEnabled");
                }
            }
        }

        private void SpecialOccurancy_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                if (!Validation.GetHasError(textBox))
                {
                    OnPropertyChanged("SpecialsSum");
                    OnPropertyChanged("IsSpecialsSumValid");
                    OnPropertyChanged("IsSendOptionsToServerEnabled");
                }
            }
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
