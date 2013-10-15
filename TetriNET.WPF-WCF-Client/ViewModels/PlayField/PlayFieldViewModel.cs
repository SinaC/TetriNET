using System;
using System.Timers;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    // PlayerViewModel not used
    public class PlayFieldViewModel : ViewModelBase, ITabIndex
    {
        private const double Epsilon = 0.00001;

        public int OpponentCount = 5;

        public GameInfoViewModel GameInfoViewModel { get; set; }
        public InGameChatViewModel InGameChatViewModel { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }
        public OpponentViewModel[] OpponentsViewModel { get; set; }

        private readonly Timer _achievementTimer;

        private string _achievement;
        public string Achievement
        {
            get { return _achievement; }
            set
            {
                if (_achievement != value)
                {
                    _achievement = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _achievementOpacity;
        public double AchievementOpacity
        {
            get { return _achievementOpacity; }
            set
            {
                if (Math.Abs(_achievementOpacity - value) > Epsilon)
                {
                    _achievementOpacity = value;
                    OnPropertyChanged();
                }
            }
        }

        private IAchievementManager _achievementManager;
        public IAchievementManager AchievementManager
        {
            get { return _achievementManager; }
            set
            {
                if (_achievementManager != null)
                    _achievementManager.OnAchieved -= OnAchieved;
                _achievementManager = value;
                if (_achievementManager != null)
                    _achievementManager.OnAchieved += OnAchieved;
            }
        }

        public PlayFieldViewModel()
        {
            _achievementTimer = new Timer(250);
            _achievementTimer.Elapsed += TimerOnElapsed;

            GameInfoViewModel = new GameInfoViewModel();
            InGameChatViewModel = new InGameChatViewModel();
            PlayerViewModel = new PlayerViewModel();
            OpponentsViewModel = new OpponentViewModel[OpponentCount];
            for (int i = 0; i < OpponentCount; i++)
                OpponentsViewModel[i] = new OpponentViewModel();

            ClientChanged += OnClientChanged;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            AchievementOpacity -= 0.1;
            if (AchievementOpacity <= 0)
                _achievementTimer.Stop();
        }

        private void OnAchieved(IAchievement achievement, bool firstTime)
        {
            Achievement = achievement.Title;
            _achievementTimer.Stop();
            AchievementOpacity = 1;
            _achievementTimer.Start();
        }

        private OpponentViewModel GetOpponentViewModel(int playerId)
        {
            if (playerId == Client.PlayerId)
                return null;
            // playerId -> id mapping rule
            // 0 1 [2] 3 4 5 -> 0 1 / 2 3 4
            // [0] 1 2 3 4 5 -> / 0 1 2 3 4 
            // 0 1 2 3 4 [5] -> 0 1 2 3 4 /
            int id;
            if (playerId < Client.PlayerId)
                id = playerId + 1;
            else
                id = playerId;
            return OpponentsViewModel[id - 1];
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 4; }
        }

        #endregion

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            GameInfoViewModel.Client = newClient;
            InGameChatViewModel.Client = newClient;
            PlayerViewModel.Client = newClient;
            foreach (OpponentViewModel opponent in OpponentsViewModel)
                opponent.Client = newClient;
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerJoined -= OnPlayerJoined;
            oldClient.OnPlayerLeft -= OnPlayerLeft;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerJoined += OnPlayerJoined;
            newClient.OnPlayerLeft += OnPlayerLeft;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        {
            OpponentViewModel opponent = GetOpponentViewModel(playerId);
            if (opponent != null)
            {
                opponent.PlayerId = -1;
                opponent.PlayerName = "Not playing";
                opponent.Team = "";
            }
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            OpponentViewModel opponent = GetOpponentViewModel(playerId);
            if (opponent != null)
            {
                opponent.PlayerId = playerId;
                opponent.PlayerName = playerName;
            }
        }

        #endregion
    }

    public class PlayFieldViewModelDesignData : PlayFieldViewModel
    {
        public new GameInfoViewModelDesignData GameInfoViewModel { get; private set; }
        public new InGameChatViewModelDesignData InGameChatViewModel { get; private set; }

        public PlayFieldViewModelDesignData()
        {
            GameInfoViewModel = new GameInfoViewModelDesignData();
            InGameChatViewModel = new InGameChatViewModelDesignData();
        }
    }
}