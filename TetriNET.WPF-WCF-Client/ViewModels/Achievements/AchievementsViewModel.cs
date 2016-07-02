﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using TetriNET.Client.Achievements;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.MVVM;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.ViewModels.Achievements
{
    public class AchievementsViewModel : ViewModelBase, ITabIndex
    {
        private ObservableCollection<IAchievement> _achievements;
        public ObservableCollection<IAchievement> Achievements
        {
            get { return _achievements; }
            set { Set(() => Achievements, ref _achievements, value); }
        }

        public int TotalPoints
        {
            get { return Achievements.Where(x => x.IsAchieved).Sum(x => x.Points); }
        }

        public int TotalAchievements
        {
            get { return Achievements.Count(x => x.IsAchieved); }
        }

        public bool IsResetEnabled => Client == null || !Client.IsGameStarted;

        public AchievementsViewModel()
        {
            ResetAchievementsCommand = new RelayCommand(Reset);

            ClientChanged += OnClientChanged;
        }

        private IAchievement _selectedItem;
        public IAchievement SelectedItem
        {
            get { return _selectedItem; }
            set { Set(() => SelectedItem, ref _selectedItem, value); }
        }

        public void Select(int achievementId)
        {
            SelectedItem = Achievements.FirstOrDefault(x => x.Id == achievementId);
            //if (achievement != null)
            //{
            //    int index = Achievements.IndexOf(achievement);
            //    SelectedIndex = index;
            //}
        }

        private void Reset()
        {
            Client.ResetAchievements();
            Settings.Default.Achievements.Save(Client.Achievements.ToList());
            Settings.Default.Save();

            ExecuteOnUIThread.Invoke(() => Achievements = BuildAchievementList(Client.Achievements.ToList()));
        }

        protected ObservableCollection<IAchievement> BuildAchievementList(IReadOnlyCollection<IAchievement> achievements)
        {
            if (achievements == null)
                return null;
            // Sort by achieve or not, then by first time achieve
            return new ObservableCollection<IAchievement>(
                achievements.Where(x => x.IsAchieved).OrderByDescending(x => x.FirstTimeAchieved)
                    .Union(
                        achievements.Where(x => !x.IsAchieved).OrderBy(x => x.Title))
                );
            //return new ObservableCollection<IAchievement>(achievements.OrderByDescending(x => x.IsAchieved).ThenByDescending(x => x.FirstTimeAchieved));
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            Achievements = BuildAchievementList(newClient.Achievements.ToList());
            //
            RefreshAchievementsStats();
        }

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.GameStarted -= RefreshResetEnable;
            oldClient.GameFinished -= OnGameFinished;
            oldClient.AchievementEarned -= OnAchievementEarned;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.GameStarted += RefreshResetEnable;
            newClient.GameFinished += OnGameFinished;
            newClient.AchievementEarned += OnAchievementEarned;
        }

        #endregion

        #region IClient events handler

        private void OnAchievementEarned(IAchievement achievement, bool firstTime)
        {
            Log.Default.WriteLine(LogLevels.Info, "Achievement: {0} {1} {2:dd/MM/yyyy HH:mm:ss}", achievement.Title, firstTime, achievement.LastTimeAchieved);

            Settings.Default.Achievements.Save(Client.Achievements.ToList());
            Settings.Default.Save();

            ExecuteOnUIThread.Invoke(() => Achievements = BuildAchievementList(Client.Achievements.ToList()));
            //
            RefreshAchievementsStats();
        }

        private void OnGameFinished(GameStatistics statistics)
        {
            //
            RefreshResetEnable();
            //
            ExecuteOnUIThread.Invoke(() => Achievements = BuildAchievementList(Client.Achievements.ToList()));
            //
            Settings.Default.Achievements.Save(Client.Achievements.ToList());
            Settings.Default.Save();
            //
            RefreshAchievementsStats();
        }

        private void RefreshResetEnable()
        {
            OnPropertyChanged("IsResetEnabled");
        }

        protected void RefreshAchievementsStats()
        {
            OnPropertyChanged("TotalPoints");
            OnPropertyChanged("TotalAchievements");
        }

        #endregion

        #region Commands

        public ICommand ResetAchievementsCommand { get; private set; }

        #endregion

        #region ITabIndex

        public int TabIndex => 5;

        #endregion
    }

    public class AchievementsViewModelDesignData : AchievementsViewModel
    {
        public AchievementsViewModelDesignData()
        {
            AchievementManager manager = new AchievementManager();
            manager.FindAllAchievements(Assembly.Load("TetriNET.Client.Achievements"));
            IAchievement sniper = manager.Achievements.FirstOrDefault(x => x.Title == "Sniper");
            if (sniper != null)
            {
                sniper.IsAchieved = true;
                sniper.FirstTimeAchieved = DateTime.Now.AddDays(-2).AddHours(1);
                sniper.LastTimeAchieved = DateTime.Now.AddDays(-2).AddHours(1);
                sniper.AchieveCount = 2;
            }
            IAchievement fearMyBrain = manager.Achievements.FirstOrDefault(x => x.Title == "Fear my brain !");
            if (fearMyBrain != null)
            {
                fearMyBrain.IsAchieved = true;
                fearMyBrain.FirstTimeAchieved = DateTime.Now.AddDays(-4);
                fearMyBrain.LastTimeAchieved = DateTime.Now.AddDays(-1);
                fearMyBrain.AchieveCount = 3;
            }
            IAchievement tooGoodForYou = manager.Achievements.FirstOrDefault(x => x.Title == "Too good for you");
            if (tooGoodForYou != null)
            {
                tooGoodForYou.IsAchieved = true;
                tooGoodForYou.FirstTimeAchieved = DateTime.Now.AddDays(-4);
                tooGoodForYou.LastTimeAchieved = DateTime.Now.AddDays(-4);
                tooGoodForYou.AchieveCount = 10;
            }
            IAchievement architect = manager.Achievements.FirstOrDefault(x => x.Title == "Architect");
            if (architect != null)
            {
                architect.IsAchieved = true;
                architect.FirstTimeAchieved = DateTime.Now.AddDays(-4);
                architect.LastTimeAchieved = DateTime.Now.AddDays(-4);
                architect.AchieveCount = 1;
            }
            IAchievement tetrisAce = manager.Achievements.FirstOrDefault(x => x.Title == "Tetris Ace");
            if (tetrisAce != null)
            {
                tetrisAce.IsAchieved = false;
                tetrisAce.ExtraData = 25;
            }
            Achievements = BuildAchievementList(manager.Achievements);
            RefreshAchievementsStats();
        }
    }
}