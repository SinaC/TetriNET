using System.Linq;
using System.Windows.Input;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class ServerOptionsViewModel : ViewModelBase
    {
        public static ServerOptionsViewModel Instance { get; private set; }

        public bool IsEnabled
        {
            get { return IsGameNotStarted && IsServerMaster; }
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

        private GameOptions _options;
        public GameOptions Options
        {
            get { return _options; }
            set
            {
                if (_options != value)
                {
                    _options = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PiecesSum
        {
            get { return Common.Randomizer.RangeRandom.SumOccurancies(Options.PieceOccurancies); }
        }

        public int SpecialsSum
        {
            get { return Common.Randomizer.RangeRandom.SumOccurancies(Options.SpecialOccurancies); }
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
            Instance = this;

            IsGameNotStarted = true;

            ClientChanged += OnClientChanged;

            SendOptionsToServerCommand = new RelayCommand(SendOptionsToServer);
            ResetOptionsCommand = new RelayCommand(ResetOptions);
            SpecialOccurancyChangedCommand = new RelayCommand(UpdateSpecialOccurancy);
            PieceOccurancyChangedCommand = new RelayCommand(UpdatePieceOccurancy);
        }

        private void SendOptionsToServer()
        {
            Client.ChangeOptions(Options);
            Settings.Default.GameOptions = Options;
            Settings.Default.Save();
        }

        private void ResetOptions()
        {
            Options.ResetToDefault();
            Settings.Default.GameOptions = Options;
            Settings.Default.Save();
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

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            Options = Settings.Default.GameOptions ?? Client.Options;
            // Add defaut values if needed
            foreach (Pieces piece in EnumHelper.GetPieces(available => available).Where(piece => Options.PieceOccurancies.All(x => x.Value != piece)))
                Options.PieceOccurancies.Add(new PieceOccurancy
                    {
                        Value = piece,
                        Occurancy = 0
                    });
            foreach (Specials special in EnumHelper.GetSpecials(available => available).Where(special => Options.SpecialOccurancies.All(x => x.Value != special)))
                Options.SpecialOccurancies.Add(new SpecialOccurancy
                    {
                        Value = special,
                        Occurancy = 0
                    });
            // Remove invalid values
            Options.PieceOccurancies = Options.PieceOccurancies.Where(x => EnumHelper.GetAttribute<PieceAttribute>(x.Value) != null && EnumHelper.GetAttribute<PieceAttribute>(x.Value).Available).ToList();
            Options.SpecialOccurancies = Options.SpecialOccurancies.Where(x => EnumHelper.GetAttribute<SpecialAttribute>(x.Value) != null && EnumHelper.GetAttribute<SpecialAttribute>(x.Value).Available).ToList();
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregister;
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnPlayerUnregistered += OnPlayerUnregister;
            newClient.OnPlayerRegistered += OnPlayerRegistered;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                if (Client.IsServerMaster)
                    Client.ChangeOptions(Options);
                else
                    Options = Client.Options;
                IsGameNotStarted = !Client.IsGameStarted;
            }
        }

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
            Options = Client.Options;
        }

        private void OnGameFinished()
        {
            IsGameNotStarted = true;
        }

        #endregion

        #region Commands

        public ICommand SendOptionsToServerCommand { get; private set; }
        public ICommand ResetOptionsCommand { get; private set; }
        public ICommand SpecialOccurancyChangedCommand { get; private set; }
        public ICommand PieceOccurancyChangedCommand { get; private set; }

        #endregion
    }

    public class ServerOptionsViewModelDesignData : ServerOptionsViewModel
    {
        public new GameOptions Options { get; private set; }
        public new ICommand SpecialOccurancyChangedCommand { get; private set; }
        public new ICommand PieceOccurancyChangedCommand { get; private set; }

        public ServerOptionsViewModelDesignData()
        {
            Options = new GameOptions();
            Options.ResetToDefault();

            SpecialOccurancyChangedCommand = new RelayCommand(() => { });
            PieceOccurancyChangedCommand = new RelayCommand(() => { });
        }
    }
}