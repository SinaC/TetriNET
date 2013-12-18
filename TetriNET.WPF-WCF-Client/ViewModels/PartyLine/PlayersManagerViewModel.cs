using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.Commands;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PartyLine
{
    public class PlayerData : ObservableObject
    {
        public int RealPlayerId { get; set; }
        public int DisplayPlayerId
        {
            get { return RealPlayerId + 1; }
        }

        public string PlayerName { get; set; }

        private string _team;
        public string Team
        {
            get { return _team; }
            set
            {
                if (_team != value)
                {
                    _team = value;
                    OnPropertyChanged();
                    OnPropertyChanged("IsTeamNotNullOrEmpty");
                }
            }
        }

        public bool IsTeamNotNullOrEmpty
        {
            get { return !String.IsNullOrWhiteSpace(Team); }
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
                }
            }
        }
    }

    public class SpectatorData : ObservableObject
    {
        public int SpectatorId { get; set; }
        public string SpectatorName { get; set; }
    }

    public class PlayersManagerViewModel : ViewModelBase
    {
        public ObservableCollection<PlayerData> PlayerList { get; private set; }
        public ICollectionView PlayerListView { get; private set; }

        public ObservableCollection<SpectatorData> SpectatorList { get; private set; }
        public ICollectionView SpectatorListView { get; private set; }

        private PlayerData _selectedPlayer;
        public PlayerData SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                if (_selectedPlayer != value)
                {
                    _selectedPlayer = value;
                    if (_selectedPlayer != null)
                        SelectedSpectator = null;
                    OnPropertyChanged();
                }
            }
        }

        private SpectatorData _selectedSpectator;
        public SpectatorData SelectedSpectator { get { return _selectedSpectator; }
            set
            {
                if (_selectedSpectator != value)
                {
                    _selectedSpectator = value;
                    if (_selectedSpectator != null)
                        SelectedPlayer = null;
                    OnPropertyChanged();
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
                }
            }
        }

        public PlayersManagerViewModel()
        {
            PlayerList = new ObservableCollection<PlayerData>();
            PlayerListView = CollectionViewSource.GetDefaultView(PlayerList);
            PlayerListView.SortDescriptions.Add(new SortDescription("DisplayPlayerId", ListSortDirection.Ascending));

            SpectatorList = new ObservableCollection<SpectatorData>();
            SpectatorListView = CollectionViewSource.GetDefaultView(SpectatorList);
            SpectatorListView.SortDescriptions.Add(new SortDescription("SpectatorId", ListSortDirection.Ascending));

            KickPlayerCommand = new RelayCommand(KickPlayer);
            BanPlayerCommand = new RelayCommand(BanPlayer);
            JoinTeamCommand = new RelayCommand<string>(JoinTeam);
        }

        private void SetServerMaster(int serverMasterId)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    foreach (PlayerData p in PlayerList)
                        p.IsServerMaster = p.RealPlayerId == serverMasterId;
                });
        }

        private void SetTeam(int playerId, string team)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    foreach (PlayerData p in PlayerList.Where(x => x.RealPlayerId == playerId))
                        p.Team = team;
                });
        }

        private void ClearEntries()
        {
            ExecuteOnUIThread.Invoke(() => PlayerList.Clear());
        }

        private void AddPlayerEntry(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() => PlayerList.Add(new PlayerData
                {
                    RealPlayerId = playerId,
                    PlayerName = playerName,
                    IsServerMaster = false,
                }));
        }

        private void DeletePlayerEntry(int playerId, string playerName)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    PlayerData p = PlayerList.FirstOrDefault(x => x.RealPlayerId == playerId);
                    if (p != null)
                        PlayerList.Remove(p);
                    else
                        Log.WriteLine(Log.LogLevels.Warning, "Trying to delete unknown player {0}[{1}] from player list", playerId, playerName);
                });
        }

        private void AddSpectatorEntry(int spectatorId, string spectatorName)
        {
            ExecuteOnUIThread.Invoke(() => SpectatorList.Add(new SpectatorData
            {
                SpectatorId = spectatorId,
                SpectatorName = spectatorName,
            }));
        }

        private void DeleteSpectatorEntry(int spectatorId, string spectatorName)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                SpectatorData s = SpectatorList.FirstOrDefault(x => x.SpectatorId == spectatorId);
                if (s != null)
                    SpectatorList.Remove(s);
                else
                    Log.WriteLine(Log.LogLevels.Warning, "Trying to delete unknown spectator {0}[{1}] from spectator list", spectatorId, spectatorName);
            });
        }

        private void KickPlayer()
        {
            if (SelectedPlayer != null && SelectedPlayer.RealPlayerId != Client.PlayerId)
                Client.KickPlayer(SelectedPlayer.RealPlayerId);
        }

        private void BanPlayer()
        {
            if (SelectedPlayer != null && SelectedPlayer.RealPlayerId != Client.PlayerId)
                Client.BanPlayer(SelectedPlayer.RealPlayerId);
        }

        public void JoinTeam(string team)
        {
            if (String.IsNullOrWhiteSpace(team) || Client.Team == team)
                return;
            Client.ChangeTeam(team);
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnPlayerTeamChanged -= OnPlayerTeamChanged;
            oldClient.OnServerMasterModified -= OnServerMasterModified;
            oldClient.OnPlayerJoined -= OnPlayerJoined;
            oldClient.OnPlayerLeft -= OnPlayerLeft;
            oldClient.OnRegisteredAsPlayer -= OnRegisteredAsPlayer;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
            oldClient.OnConnectionLost -= OnConnectionLost;
            oldClient.OnRegisteredAsSpectator -= OnRegisteredAsSpectator;
            oldClient.OnSpectatorLeft -= OnSpectatorLeft;
            oldClient.OnSpectatorJoined -= OnSpectatorJoined;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnPlayerTeamChanged += OnPlayerTeamChanged;
            newClient.OnServerMasterModified += OnServerMasterModified;
            newClient.OnPlayerJoined += OnPlayerJoined;
            newClient.OnPlayerLeft += OnPlayerLeft;
            newClient.OnRegisteredAsPlayer += OnRegisteredAsPlayer;
            newClient.OnPlayerUnregistered += OnPlayerUnregistered;
            newClient.OnConnectionLost += OnConnectionLost;
            newClient.OnRegisteredAsSpectator += OnRegisteredAsSpectator;
            newClient.OnSpectatorLeft += OnSpectatorLeft;
            newClient.OnSpectatorJoined += OnSpectatorJoined;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerTeamChanged(int playerId, string team)
        {
            SetTeam(playerId, team);
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ClearEntries();
            IsServerMaster = false;
        }

        private void OnServerMasterModified(int serverMasterId)
        {
            SetServerMaster(serverMasterId);
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnPlayerJoined(int playerId, string playerName)
        {
            AddPlayerEntry(playerId, playerName);
        }

        private void OnPlayerLeft(int playerId, string playerName, LeaveReasons reason)
        {
            DeletePlayerEntry(playerId, playerName);
        }

        private void OnPlayerUnregistered()
        {
            ClearEntries();
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnRegisteredAsPlayer(RegistrationResults result, int playerId, bool isServerMaster)
        {
            ClearEntries();
            AddPlayerEntry(playerId, Client.Name);
            IsServerMaster = Client.IsServerMaster;
        }

        private void OnRegisteredAsSpectator(RegistrationResults result, int spectatorId)
        {
            ClearEntries();
            AddSpectatorEntry(spectatorId, Client.Name);
        }

        private void OnSpectatorJoined(int spectatorId, string spectatorName)
        {
            AddSpectatorEntry(spectatorId, spectatorName);
        }

        private void OnSpectatorLeft(int spectatorId, string spectatorName, LeaveReasons reason)
        {
            DeleteSpectatorEntry(spectatorId, spectatorName);
        }

        #endregion

        #region Commands

        public ICommand KickPlayerCommand { get; private set; }
        public ICommand BanPlayerCommand { get; private set; }
        public ICommand JoinTeamCommand { get; private set; }

        #endregion
    }

    public class PlayersManagerViewModelDesignData : PlayersManagerViewModel
    {
        public new ObservableCollection<PlayerData> PlayerListView { get; private set; }
        public new ObservableCollection<SpectatorData> SpectatorListView { get; private set; }

        public PlayersManagerViewModelDesignData()
        {
            PlayerListView = new ObservableCollection<PlayerData>
                {
                    new PlayerData
                        {
                            PlayerName = "ServerMaster",
                            Team = "TEAM1",
                            IsServerMaster = true,
                            RealPlayerId = 0
                        },
                    new PlayerData
                        {
                            PlayerName = "Dummy2",
                            Team = "LMA",
                            IsServerMaster = false,
                            RealPlayerId = 2
                        },
                    new PlayerData
                        {
                            PlayerName = "Dummy3",
                            IsServerMaster = false,
                            RealPlayerId = 3
                        },
                };
            SpectatorListView = new ObservableCollection<SpectatorData>
                {
                    new SpectatorData
                        {
                            SpectatorName = "Spectator0",
                            SpectatorId = 0
                        },
                    new SpectatorData
                        {
                            SpectatorName = "Spectator2",
                            SpectatorId = 2
                        },
                    new SpectatorData
                        {
                            SpectatorName = "Spectator3",
                            SpectatorId = 3
                        },
                };
        }
    }
}