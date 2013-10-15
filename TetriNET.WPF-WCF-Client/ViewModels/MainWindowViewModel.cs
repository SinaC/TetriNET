using System.Collections.Generic;
using TetriNET.Client.Achievements;
using TetriNET.Client.Achievements.Achievements;
using TetriNET.Client.Board;
using TetriNET.Client.Pieces;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.ViewModels.Achievements;
using TetriNET.WPF_WCF_Client.ViewModels.Connection;
using TetriNET.WPF_WCF_Client.ViewModels.Options;
using TetriNET.WPF_WCF_Client.ViewModels.PartyLine;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;
using TetriNET.WPF_WCF_Client.ViewModels.Statistics;
using TetriNET.WPF_WCF_Client.ViewModels.WinList;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public WinListViewModel WinListViewModel { get; private set; }
        public ClientStatisticsViewModel ClientStatisticsViewModel { get; private set; }
        public OptionsViewModel OptionsViewModel { get; private set; }
        public PartyLineViewModel PartyLineViewModel { get; private set; }
        public ConnectionViewModel ConnectionViewModel { get; private set; }
        public PlayFieldViewModel PlayFieldViewModel { get; private set; }
        public AchievementsViewModel AchievementsViewModel { get; private set; }

        public IAchievementManager AchievementManager { get; private set; }

        private int _activeTabItemIndex;

        public int ActiveTabItemIndex
        {
            get { return _activeTabItemIndex; }
            set
            {
                if (_activeTabItemIndex != value)
                {
                    _activeTabItemIndex = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindowViewModel()
        {
            // Create sub view models
            WinListViewModel = new WinListViewModel();
            ClientStatisticsViewModel = new ClientStatisticsViewModel();
            OptionsViewModel = new OptionsViewModel();
            PartyLineViewModel = new PartyLineViewModel();
            ConnectionViewModel = new ConnectionViewModel();
            PlayFieldViewModel = new PlayFieldViewModel();
            AchievementsViewModel = new AchievementsViewModel();

            //
            //
            AchievementManager = new AchievementManager
            {
                Achievements = new List<IAchievement>
                {
                    new Sniper(),
                    new Architect(),
                    new Magician(),
                    new FearMyBrain(),
                    new RunBabyRun(),
                    new SerialBuilder(),
                    new TooGoodForYou(),
                    new HitchhikersGuide(),
                    new NewtonsApple(),
                    new TooEasyForMe(),
                    new WhoIsYourDaddy(),
                    new CallMeSavior(),
                    new JustInTime(),
                    new NuclearLaunchDetected()
                },
                //    Achievements = new List<Achievement>
                //    {
                //        new Sniper
                //        {
                //            IsAchieved = true,
                //            FirstTimeAchieved = DateTime.Now.AddDays(-2).AddHours(1),
                //            LastTimeAchieved = DateTime.Now.AddDays(-2).AddHours(1),
                //            AchieveCount = 1,
                //        },
                //        new Architect
                //        {
                //            IsAchieved = false
                //        },
                //        new Magician
                //        {
                //            IsAchieved = false
                //        },
                //        new FearMyBrain
                //        {
                //            IsAchieved = true,
                //            FirstTimeAchieved = DateTime.Now.AddDays(-4),
                //            LastTimeAchieved = DateTime.Now.AddDays(-1),
                //            AchieveCount = 2
                //        },
                //        new RunBabyRun
                //        {
                //            IsAchieved = false
                //        },
                //        new SerialBuilder
                //        {
                //            IsAchieved = false
                //        },
                //        new TooGoodForYou
                //        {
                //            IsAchieved = true,
                //            FirstTimeAchieved = DateTime.Now.AddHours(-4),
                //            LastTimeAchieved = DateTime.Now.AddHours(-4),
                //            AchieveCount = 1,
                //        },
                //        new HitchhikersGuide
                //        {
                //            IsAchieved = false
                //        },
                //        new NewtonsApple
                //        {
                //            IsAchieved = false
                //        },
                //        new TooEasyForMe
                //        {
                //            IsAchieved = false
                //        },
                //        new WhoIsYourDaddy
                //        {
                //            IsAchieved = false
                //        },
                //        new CallMeSavior
                //        {
                //            IsAchieved = false
                //        }
                //    }
            };
            AchievementsViewModel.AchievementManager = AchievementManager;
            PlayFieldViewModel.AchievementManager = AchievementManager;
            // TODO: read/write achievement data from settings

            //
            ClientChanged += OnClientChanged;

            // Create client
            //Client = new Client.Client(Piece.CreatePiece, () => new BoardWithWallKick(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height));
            Client = new Client.Client((pieces, i, arg3, arg4, arg5, arg6) => Piece.CreatePiece(Pieces.TetriminoI, i, arg3, arg4, arg5, arg6), () => new BoardWithWallKick(ClientOptionsViewModel.Width, ClientOptionsViewModel.Height));
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            WinListViewModel.Client = newClient;
            ClientStatisticsViewModel.Client = newClient;
            OptionsViewModel.Client = newClient;
            PartyLineViewModel.Client = newClient;
            ConnectionViewModel.Client = newClient;
            PlayFieldViewModel.Client = newClient;
            AchievementsViewModel.Client = newClient;

            AchievementManager.Client = Client;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerRegistered -= OnPlayerRegistered;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregisted;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameOver -= OnGameOver;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerRegistered += OnPlayerRegistered;
            newClient.OnPlayerUnregistered += OnPlayerUnregisted;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameOver += OnGameOver;
            newClient.OnConnectionLost += OnConnectionLost;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerRegistered(RegistrationResults result, int playerId, bool isServerMaster)
        {
            if (result == RegistrationResults.RegistrationSuccessful && ClientOptionsViewModel.Instance.AutomaticallySwitchToPartyLineOnRegistered)
                if (ActiveTabItemIndex == ConnectionViewModel.TabIndex)
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
        }

        private void OnPlayerUnregisted()
        {
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        private void OnGameStarted()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
                ActiveTabItemIndex = PlayFieldViewModel.TabIndex;
        }

        private void OnGameFinished()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == PlayFieldViewModel.TabIndex)
                {
                    PartyLineViewModel.ChatViewModel.IsInputFocused = true;
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
                }
            }
        }

        private void OnGameOver()
        {
            if (ClientOptionsViewModel.Instance.AutomaticallySwitchToPlayFieldOnGameStarted)
            {
                if (ActiveTabItemIndex == PlayFieldViewModel.TabIndex)
                {
                    PartyLineViewModel.ChatViewModel.IsInputFocused = true;
                    ActiveTabItemIndex = PartyLineViewModel.TabIndex;
                }
            }
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ActiveTabItemIndex = ConnectionViewModel.TabIndex;
        }

        #endregion
    }

    public class MainWindowViewModelDesignData : MainWindowViewModel
    {
        public new WinListViewModelDesignData WinListViewModel { get; private set; }
        public new ClientStatisticsViewModelDesignData ClientStatisticsViewModel { get; private set; }
        public new OptionsViewModelDesignData OptionsViewModel { get; private set; }
        public new PartyLineViewModelDesignData PartyLineViewModel { get; private set; }
        public new ConnectionViewModelDesignData ConnectionViewModel { get; private set; }
        public new PlayFieldViewModelDesignData PlayFieldViewModel { get; private set; }
        public new AchievementsViewModelDesignData AchievementsViewModel { get; private set; }

        public MainWindowViewModelDesignData()
        {
            WinListViewModel = new WinListViewModelDesignData();
            ClientStatisticsViewModel = new ClientStatisticsViewModelDesignData();
            OptionsViewModel = new OptionsViewModelDesignData();
            PartyLineViewModel = new PartyLineViewModelDesignData();
            ConnectionViewModel = new ConnectionViewModelDesignData();
            PlayFieldViewModel = new PlayFieldViewModelDesignData();
            AchievementsViewModel = new AchievementsViewModelDesignData();
        }
    }
}