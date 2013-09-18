using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TetriNET.Client;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.WinList
{
    public class WinListViewModel : ViewModelBase, ITabIndex
    {
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

        private readonly ObservableCollection<WinEntry> _winList = new ObservableCollection<WinEntry>();
        public ObservableCollection<WinEntry> WinList
        {
            get { return _winList; }
        }

        public WinListViewModel()
        {
            ResetWinListCommand = new RelayCommand(() => Client.ResetWinList());
        }

        private void UpdateWinList(IEnumerable<WinEntry> winList)
        {
            _winList.Clear();
            foreach (WinEntry entry in winList.OrderByDescending(x => x.Score))
                _winList.Add(entry);
        }

        #region ITabIndex
        public int TabIndex { get { return 2; } }
        #endregion

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnWinListModified -= OnWinListModified;
            oldClient.OnConnectionLost -= OnConnectionLost;
        }
        
        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnWinListModified += OnWinListModified;
            newClient.OnConnectionLost += OnConnectionLost;
        }
        #endregion

        #region IClient events handler
        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            IsServerMaster = false;
        }

        private void OnWinListModified(List<WinEntry> winList)
        {
            ExecuteOnUIThread.Invoke(() => UpdateWinList(winList));
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            IsServerMaster = Client.IsServerMaster;
        }
        #endregion

        #region Commands

        public ICommand ResetWinListCommand { get; set; }

        #endregion
    }
}
