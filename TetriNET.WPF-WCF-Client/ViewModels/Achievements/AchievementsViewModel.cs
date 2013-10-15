using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Client.Achievements.Achievements;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.Achievements
{
    public class AchievementsViewModel : ViewModelBase
    {
        private ObservableCollection<IAchievement> _achievements;

        public ObservableCollection<IAchievement> Achievements
        {
            get
            {
                return _achievements;
            }
            set
            {
                if (_achievements != value)
                {
                    _achievements = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsResetEnabled
        {
            get { return Client == null || !Client.IsGameStarted; }
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
                {
                    _achievementManager.OnAchieved += OnAchieved;
                    Achievements = BuildAchievementList(_achievementManager.Achievements);
                }
            }
        }

        public AchievementsViewModel()
        {
            ResetAchievementsCommand = new RelayCommand(() => _achievementManager.Reset());
        }

        private void OnAchieved(IAchievement achievement, bool firstTime)
        {
            Log.WriteLine(Log.LogLevels.Info, "Achievement: {0} {1} {2:dd/MM/yyyy HH:mm:ss}", achievement.Title, firstTime, achievement.LastTimeAchieved);

            // TODO: Should be replaced with a client anonymous message or a new server API
            Client.PublishMessage(String.Format("has earned the achievement [{0}]", achievement.Title));

            ExecuteOnUIThread.Invoke(() => Achievements = BuildAchievementList(_achievementManager.Achievements));
        }

        protected ObservableCollection<IAchievement> BuildAchievementList(IEnumerable<IAchievement> achievements)
        {
            // Sort by achieve or not, then by first time achieve
            return new ObservableCollection<IAchievement>(achievements.OrderByDescending(x => x.IsAchieved).ThenByDescending(x => x.FirstTimeAchieved));
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameStarted -= RefreshResetEnable;
            oldClient.OnGameFinished -= RefreshResetEnable;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameStarted += RefreshResetEnable;
            newClient.OnGameFinished += RefreshResetEnable;
        }

        #endregion

        #region IClient events handler

        private void RefreshResetEnable()
        {
            OnPropertyChanged("IsResetEnabled");
        }

        #endregion

        #region Commands

        public ICommand ResetAchievementsCommand { get; private set; }

        #endregion
    }

    public class AchievementsViewModelDesignData : AchievementsViewModel
    {
        public AchievementsViewModelDesignData()
        {
            Achievements = BuildAchievementList(new List<IAchievement>
            {
                new Sniper
                {
                    IsAchieved = true,
                    FirstTimeAchieved = DateTime.Now.AddDays(-2).AddHours(1),
                    LastTimeAchieved = DateTime.Now.AddDays(-2).AddHours(1),
                    AchieveCount = 1,
                },
                new Architect
                {
                    IsAchieved = false
                },
                new Magician
                {
                    IsAchieved = false
                },
                new FearMyBrain
                {
                    IsAchieved = true,
                    FirstTimeAchieved = DateTime.Now.AddDays(-4),
                    LastTimeAchieved = DateTime.Now.AddDays(-1),
                    AchieveCount = 2
                },
                new RunBabyRun
                {
                    IsAchieved = false
                },
                new SerialBuilder
                {
                    IsAchieved = false
                },
                new TooGoodForYou
                {
                    IsAchieved = true,
                    FirstTimeAchieved = DateTime.Now.AddHours(-4),
                    LastTimeAchieved = DateTime.Now.AddHours(-4),
                    AchieveCount = 1,
                },
                new HitchhikersGuide
                {
                    IsAchieved = false
                },
                new NewtonsApple
                {
                    IsAchieved = false
                },
                new TooEasyForMe
                {
                    IsAchieved = false
                },
                new WhoIsYourDaddy
                {
                    IsAchieved = false
                },
                new CallMeSavior
                {
                    IsAchieved = false
                }
            });
        }
    }
}