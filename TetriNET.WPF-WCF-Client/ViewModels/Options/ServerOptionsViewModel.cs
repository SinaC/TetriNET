using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class ServerOptionsViewModel : ViewModelBase
    {
        public bool IsEnabled { get { return IsGameNotStarted && IsServerMaster; } }

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
                    OnPropertyChanged("IsEnabled");
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
                    OnPropertyChanged("IsEnabled");
                }
            }
        }

        public Models.Options Options
        {
            get { return Models.Options.OptionsSingleton.Instance; }
        }

        public int PiecesSum
        {
            get { return Common.Randomizer.RangeRandom.SumOccurancies(Options.ServerOptions.PieceOccurancies); }
        }

        public int SpecialsSum
        {
            get { return Common.Randomizer.RangeRandom.SumOccurancies(Options.ServerOptions.SpecialOccurancies); }
        }

        public bool IsPiecesSumValid
        {
            get { return PiecesSum == 100; }
        }

        public bool IsSpecialsSumValid
        {
            get { return SpecialsSum == 100; }
        }

        public bool IsSendOptionsToServerEnabled
        {
            get { return IsPiecesSumValid && IsSpecialsSumValid; }
        }

        public ServerOptionsViewModel()
        {
            IsGameNotStarted = true;

            SendOptionsToServerCommand = new RelayCommand(SendOptionsToServer);
            ResetOptionsCommand = new RelayCommand(ResetOptions);
            SpecialOccurancyChangedCommand = new RelayCommand(UpdateSpecialOccurancy);
            PieceOccurancyChangedCommand = new RelayCommand(UpdatePieceOccurancy);
        }

        private void SendOptionsToServer()
        {
            Client.ChangeOptions(Models.Options.OptionsSingleton.Instance.ServerOptions);
            Settings.Default.GameOptions = Models.Options.OptionsSingleton.Instance.ServerOptions;
            Settings.Default.Save();
        }

        private void ResetOptions()
        {
            Models.Options.OptionsSingleton.Instance.ServerOptions = new GameOptions();
            OnPropertyChanged("Options");
        }

        private void UpdateSpecialOccurancy()
        {
            OnPropertyChanged("SpecialsSum");
            OnPropertyChanged("IsSpecialsSumValid");
            OnPropertyChanged("IsSendOptionsToServerEnabled");
        }

        private void UpdatePieceOccurancy()
        {
            OnPropertyChanged("PiecesSum");
            OnPropertyChanged("IsPiecesSumValid");
            OnPropertyChanged("IsSendOptionsToServerEnabled");
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregister;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerUnregistered += OnPlayerUnregister;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregister()
        {
            IsServerMaster = Client.IsServerMaster;
            IsGameNotStarted = true;
        }

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

        #region Commands

        public ICommand SendOptionsToServerCommand { get; set; }
        public ICommand ResetOptionsCommand { get; set; }
        public ICommand SpecialOccurancyChangedCommand { get; set; }
        public ICommand PieceOccurancyChangedCommand { get; set; }

        #endregion
    }
}
