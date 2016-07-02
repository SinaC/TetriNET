﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.MVVM;

namespace TetriNET.WPF_WCF_Client.ViewModels.WinList
{
    public class Entry
    {
        public string Name { get; set; }
        public int Score { get; set; }
    }

    public class WinListViewModel : ViewModelBase, ITabIndex
    {
        public bool IsResetEnabled => Client != null && Client.IsServerMaster && !Client.IsGameStarted;

        public ObservableCollection<Entry> PlayerWinList { get; }
        public ObservableCollection<Entry> TeamWinList { get; }

        public WinListViewModel()
        {
            PlayerWinList = new ObservableCollection<Entry>();
            TeamWinList = new ObservableCollection<Entry>();

            ResetWinListCommand = new RelayCommand(() => Client.ResetWinList());
        }

        protected void UpdateWinList(List<WinEntry> winList)
        {
            //
            PlayerWinList.Clear();
            foreach (WinEntry entry in winList.OrderByDescending(x => x.Score))
                PlayerWinList.Add(new Entry
                    {
                        Name = entry.PlayerName + (String.IsNullOrWhiteSpace(entry.Team) ? String.Empty : (" - " + entry.Team)),
                        Score = entry.Score
                    });
            //
            TeamWinList.Clear();
            foreach(Entry entry in winList.GroupBy(x => String.IsNullOrWhiteSpace(x.Team) ? x.PlayerName : x.Team).Select(g => new Entry { Name = g.Key, Score = g.Sum(x => x.Score) }).OrderByDescending(x => x.Score))
                TeamWinList.Add(entry);
        }

        #region ITabIndex

        public int TabIndex => 2;

        #endregion

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.ServerMasterModified -= OnServerMasterModified;
            oldClient.WinListModified -= OnWinListModified;
            oldClient.ConnectionLost -= OnConnectionLost;
            oldClient.GameStarted -= RefreshResetEnable;
            oldClient.GameFinished -= OnGameFinished;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.ServerMasterModified += OnServerMasterModified;
            newClient.WinListModified += OnWinListModified;
            newClient.ConnectionLost += OnConnectionLost;
            newClient.GameStarted += RefreshResetEnable;
            newClient.GameFinished += OnGameFinished;
        }

        #endregion

        #region IClient events handler

        private void OnGameFinished(GameStatistics statistics)
        {
            RefreshResetEnable();
        }

        private void RefreshResetEnable()
        {
            OnPropertyChanged("IsResetEnabled");
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            RefreshResetEnable();
        }

        private void OnWinListModified(List<WinEntry> winList)
        {
            ExecuteOnUIThread.Invoke(() => UpdateWinList(winList));
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            RefreshResetEnable();
        }

        #endregion

        #region Commands

        public ICommand ResetWinListCommand { get; private set; }

        #endregion
    }

    public class WinListViewModelDesignData : WinListViewModel
    {
        public WinListViewModelDesignData()
        {
            UpdateWinList(new List<WinEntry>
                {
                    new WinEntry
                        {
                            PlayerName = "Dummy100",
                            Team = "TEAM1",
                            Score = 100
                        },
                    new WinEntry
                        {
                            PlayerName = "Dummy75",
                            Team = "TEAM2",
                            Score = 75
                        },
                    new WinEntry
                        {
                            PlayerName = "Dummy50",
                            Team = "TEAM2",
                            Score = 50
                        },
                    new WinEntry
                        {
                            PlayerName = "Dummy25",
                            Team = "TEAM2",
                            Score = 25
                        }
                });
        }
    }
}