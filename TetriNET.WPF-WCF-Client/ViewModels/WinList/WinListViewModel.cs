using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.WinList
{
    public class WinListViewModel : ViewModelBase, ITabIndex
    {
        public bool IsResetEnabled
        {
            get { return Client != null && (Client.IsServerMaster && !Client.IsGameStarted); }
        }

        public ObservableCollection<WinEntry> WinList { get; private set; }

        public WinListViewModel()
        {
            WinList = new ObservableCollection<WinEntry>();

            ResetWinListCommand = new RelayCommand(() => Client.ResetWinList());
        }

        private void UpdateWinList(IEnumerable<WinEntry> winList)
        {
            WinList.Clear();
            foreach (WinEntry entry in winList.OrderByDescending(x => x.Score))
                WinList.Add(entry);
        }

        #region ITabIndex

        public int TabIndex
        {
            get { return 2; }
        }

        #endregion

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnWinListModified -= OnWinListModified;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnGameStarted -= RefreshResetEnable;
            oldClient.OnGameFinished -= RefreshResetEnable;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnWinListModified += OnWinListModified;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnGameStarted += RefreshResetEnable;
            newClient.OnGameFinished += RefreshResetEnable;
        }

        #endregion

        #region IClient events handler

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

        public ICommand ResetWinListCommand { get; set; }

        #endregion
    }

    public class WinListViewModelDesignData : WinListViewModel
    {
        public new ObservableCollection<WinEntry> WinList { get; private set; }

        public WinListViewModelDesignData()
        {
            WinList = new ObservableCollection<WinEntry>
                {
                    new WinEntry
                        {
                            PlayerName = "Dummy100",
                            Score = 100
                        },
                    new WinEntry
                        {
                            PlayerName = "Dummy50",
                            Score = 50
                        },
                    new WinEntry
                        {
                            PlayerName = "Dummy25",
                            Score = 25
                        }
                };
        }
    }
}