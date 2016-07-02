﻿using System.Linq;
using System.Windows.Input;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class ServerOptionsViewModel : ViewModelBase
    {
        public static ServerOptionsViewModel Instance { get; private set; }

        public bool IsEnabled => IsGameNotStarted && IsServerMaster;

        private bool _isGameNotStarted;
        public bool IsGameNotStarted
        {
            get { return _isGameNotStarted; }
            set
            {
                if (Set(() => IsGameNotStarted, ref _isGameNotStarted, value))
                    OnPropertyChanged("IsEnabled");
            }
        }

        private bool _isServerMaster;
        public bool IsServerMaster
        {
            get { return _isServerMaster; }
            set
            {
                if (Set(() => IsServerMaster, ref _isServerMaster, value))
                    OnPropertyChanged("IsEnabled");
            }
        }

        private GameOptions _options;
        public GameOptions Options
        {
            get { return _options; }
            set { Set(() => Options, ref _options, value); }
        }

        public int PiecesSum => Common.Randomizer.RangeRandom.SumOccurancies(Options.PieceOccurancies);

        public int SpecialsSum => Common.Randomizer.RangeRandom.SumOccurancies(Options.SpecialOccurancies);

        public bool IsPiecesSumValid => PiecesSum == 100;

        public bool IsSpecialsSumValid => SpecialsSum == 100;

        public bool IsSendOptionsToServerEnabled => IsPiecesSumValid && IsSpecialsSumValid;

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
            oldClient.OptionsChanged -= OnOptionsChanged;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.ServerMasterModified -= OnServerMasterModified;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.PlayerUnregistered -= OnPlayerUnregister;
            oldClient.RegisteredAsSpectator -= OnRegisteredAsSpectator;
            oldClient.RegisteredAsPlayer -= OnRegisteredAsPlayer;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OptionsChanged += OnOptionsChanged;
            newClient.GameFinished += OnGameFinished;
            newClient.GameStarted += OnGameStarted;
            newClient.ServerMasterModified += OnServerMasterModified;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.PlayerUnregistered += OnPlayerUnregister;
            newClient.RegisteredAsSpectator += OnRegisteredAsSpectator;
            newClient.RegisteredAsPlayer += OnRegisteredAsPlayer;
        }

        #endregion

        #region IClient events handler

        private void OnRegisteredAsPlayer(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
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

        private void OnRegisteredAsSpectator(RegistrationResults result, Versioning serverVersion, int spectatorId)
        {
            if (result == RegistrationResults.RegistrationSuccessful)
            {
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
        }

        private void OnGameFinished(GameStatistics statistics)
        {
            IsGameNotStarted = true;
        }

        private void OnOptionsChanged()
        {
            Options = Client.Options;
        }

        #endregion

        #region Commands

        public ICommand SendOptionsToServerCommand { get; protected set; }
        public ICommand ResetOptionsCommand { get; protected set; }
        public ICommand SpecialOccurancyChangedCommand { get; protected set; }
        public ICommand PieceOccurancyChangedCommand { get; protected set; }

        #endregion
    }

    public class ServerOptionsViewModelDesignData : ServerOptionsViewModel
    {
        public ServerOptionsViewModelDesignData()
        {
            Options = new GameOptions();
            Options.ResetToDefault();

            SpecialOccurancyChangedCommand = new RelayCommand(() => { });
            PieceOccurancyChangedCommand = new RelayCommand(() => { });
        }
    }
}