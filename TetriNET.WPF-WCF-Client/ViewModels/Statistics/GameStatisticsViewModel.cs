using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Randomizer;
using TetriNET.WPF_WCF_Client.DynamicGrid;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class SpecialStatisticsRow : DynamicRow
    {
        private Specials _special;
        public Specials Special
        {
            get { return _special; }
            set
            {
                if (_special != value)
                {
                    _special = value;
                    OnPropertyChanged();
                }
            }
        }
    }

    public class GameStatisticsViewModel : ViewModelBase
    {
        private const int HistorySize = 10;

        private ObservableCollection<GameStatistics> _gameStatisticsHistory;
        public ObservableCollection<GameStatistics> GameStatisticsHistory
        {
            get { return _gameStatisticsHistory; }
            set { Set(() => GameStatisticsHistory, ref _gameStatisticsHistory, value); }
        }

        public bool IsHistoryVisible => GameStatisticsHistory.Count > 0;

        private GameStatistics _selectedGameStatistics;
        public GameStatistics SelectedGameStatistics
        {
            get { return _selectedGameStatistics; }
            set
            {
                if (Set(() => SelectedGameStatistics, ref _selectedGameStatistics, value))
                {
                    InitializePlayerList();
                    InitializeSpecialsGrid();

                    OnPropertyChanged("First3Players");
                    OnPropertyChanged("Next3Players");
                    OnPropertyChanged("IsNextPlayersNeeded");
                    OnPropertyChanged("MatchTime");
                }
            }
        }

        public List<GameStatisticsByPlayer> First3Players => _selectedGameStatistics?.Players.Take(3).ToList() ?? Enumerable.Empty<GameStatisticsByPlayer>().ToList();

        public List<GameStatisticsByPlayer> Next3Players => _selectedGameStatistics?.Players.Skip(3).Take(3).ToList() ?? Enumerable.Empty<GameStatisticsByPlayer>().ToList();

        public bool IsNextPlayersNeeded => _selectedGameStatistics != null && _selectedGameStatistics.Players.Count > 3;

        public double MatchTime // In seconds
            => _selectedGameStatistics == null ? 0 : (_selectedGameStatistics.GameFinished - _selectedGameStatistics.GameStarted).TotalSeconds;

        private List<string> _playerList;
        public List<string> PlayerList
        {
            get { return _playerList; }
            set { Set(() => PlayerList, ref _playerList, value); }
        }

        private string _selectedPlayer;
        public string SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                if (Set(() => SelectedPlayer, ref _selectedPlayer, value))
                    InitializeSpecialsGrid();
            }
        }

        private DynamicGrid<SpecialStatisticsRow, DynamicColumn> _specialsFromSelectedPlayerGrid; // Columns: player  Rows: special
        public DynamicGrid<SpecialStatisticsRow, DynamicColumn> SpecialsFromSelectedPlayerGrid
        {
            get { return _specialsFromSelectedPlayerGrid; }
            set { Set(() => SpecialsFromSelectedPlayerGrid, ref _specialsFromSelectedPlayerGrid, value); }
        }

        private DynamicGrid<SpecialStatisticsRow, DynamicColumn> _specialsToSelectedPlayerGrid; // Columns: player  Rows: special
        public DynamicGrid<SpecialStatisticsRow, DynamicColumn> SpecialsToSelectedPlayerGrid
        {
            get { return _specialsToSelectedPlayerGrid; }
            set { Set(() => SpecialsToSelectedPlayerGrid, ref _specialsToSelectedPlayerGrid, value); }
        }

        public GameStatisticsViewModel()
        {
            GameStatisticsHistory = new ObservableCollection<GameStatistics>();
            //SelectedGameStatistics = new SelectedGameStatistics
            //{
            //    GameStarted = DateTime.Now.AddMinutes(-10),
            //    GameFinished = DateTime.Now.AddMinutes(-5),
            //    Players = new List<GameStatisticsByPlayer>
            //            {
            //                new GameStatisticsByPlayer
            //                    {
            //                        PlayerName = "Player1",
            //                        SingleCount = 4,
            //                        DoubleCount = 3,
            //                        TripleCount = 2,
            //                        TetrisCount = 1,
            //                    },
            //                    new GameStatisticsByPlayer
            //                    {
            //                        PlayerName = "Player2",
            //                        SingleCount = 8,
            //                        DoubleCount = 6,
            //                        TripleCount = 4,
            //                        TetrisCount = 0,
            //                    },
            //                    new GameStatisticsByPlayer
            //                    {
            //                        PlayerName = "Player3",
            //                        SingleCount = 1,
            //                        DoubleCount = 1,
            //                        TripleCount = 0,
            //                        TetrisCount = 0,
            //                    },
            //                    new GameStatisticsByPlayer
            //                    {
            //                        PlayerName = "Player4",
            //                        SingleCount = 0,
            //                        DoubleCount = 0,
            //                        TripleCount = 0,
            //                        TetrisCount = 1,
            //                    },
            //                    new GameStatisticsByPlayer
            //                    {
            //                        PlayerName = "Player5",
            //                        SingleCount = 12,
            //                        DoubleCount = 7,
            //                        TripleCount = 0,
            //                        TetrisCount = 0,
            //                    },
            //                    //new GameStatisticsByPlayer
            //                    //{
            //                    //    PlayerName = "Player6",
            //                    //    SingleCount = 20,
            //                    //    DoubleCount = 17,
            //                    //    TripleCount = 5,
            //                    //    TetrisCount = 2,
            //                    //},
            //            }
            //};
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.GameFinished -= OnGameFinished;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.GameFinished += OnGameFinished;
        }

        #endregion

        #region IClient events handler

        private void OnGameFinished(GameStatistics statistics)
        {
            SelectedPlayer = null; // reset
            ExecuteOnUIThread.Invoke(() => AddStatistics(statistics));
            SelectedGameStatistics = statistics;
        }

        #endregion

        private void AddStatistics(GameStatistics statistics)
        {
            // Remove oldest entry if no place left
            if (GameStatisticsHistory.Count >= HistorySize)
                GameStatisticsHistory.RemoveAt(HistorySize-1);
            // Add entry
            GameStatisticsHistory.Insert(0, statistics);
            OnPropertyChanged("IsHistoryVisible");
        }

        private void InitializePlayerList()
        {
            if (SelectedGameStatistics?.Players != null)
            {
                PlayerList = SelectedGameStatistics.Players.Select(x => x.PlayerName).ToList();
                if (IsInDesignMode)
                    SelectedPlayer = PlayerList.FirstOrDefault();
                else
                    SelectedPlayer = PlayerList.FirstOrDefault(x => x == Client.Name);
            }
            else
            {
                PlayerList = null;
                SelectedPlayer = null;
            }
        }

        private void InitializeSpecialsGrid()
        {
            if (SelectedPlayer == null || SelectedGameStatistics?.Players == null)
            {
                SpecialsFromSelectedPlayerGrid = null;
                SpecialsToSelectedPlayerGrid = null;
            }
            else
            {
                InitializeFromSelectedPlayer();
                InitializeToSelectedPlayer();
            }
        }

        private void InitializeFromSelectedPlayer()
        {
            // From selected player
            GameStatisticsByPlayer byPlayer = SelectedGameStatistics.Players.FirstOrDefault(x => x.PlayerName == SelectedPlayer);
            if (byPlayer?.SpecialsUsed == null)
                SpecialsFromSelectedPlayerGrid = null;
            else
            {
                List<DynamicColumn> columns = byPlayer.SpecialsUsed.SelectMany(kv => kv.Value).Select(x => x.Key).Distinct().Select(x => new DynamicColumn
                {
                    Name = x,
                    DisplayName = x,
                    Type = typeof(string),
                    IsReadOnly = true,
                }).ToList();
                List<SpecialStatisticsRow> rows = new List<SpecialStatisticsRow>();
                foreach (KeyValuePair<Specials, Dictionary<string, int>> specials in byPlayer.SpecialsUsed.OrderBy(kv => kv.Key))
                {
                    SpecialStatisticsRow row = new SpecialStatisticsRow
                    {
                        Special = specials.Key
                    };
                    foreach (DynamicColumn column in columns)
                    {
                        KeyValuePair<string, int> kv = specials.Value.FirstOrDefault(x => x.Key == column.Name);
                        int value = kv.Equals(default(KeyValuePair<string, int>)) ? 0 : kv.Value;
                        row.TryAddProperty(column.Name, value);
                    }
                    rows.Add(row);
                }
                SpecialsFromSelectedPlayerGrid = new DynamicGrid<SpecialStatisticsRow, DynamicColumn>(rows, columns);
            }
        }

        private void InitializeToSelectedPlayer()
        {
            List<DynamicColumn> columns = SelectedGameStatistics.Players.Select(x => new DynamicColumn
                {
                    Name = x.PlayerName,
                    DisplayName = x.PlayerName,
                    Type = typeof(int),
                    IsReadOnly = true,
                }).ToList();
            List<Specials> specials = SelectedGameStatistics.Players.Where(p => p.SpecialsUsed != null).SelectMany(p => p.SpecialsUsed.Select(kv => kv.Key)).Distinct().ToList();
            List<SpecialStatisticsRow> rows = specials.Select(x => new SpecialStatisticsRow
                {
                    Special = x
                }).ToList();
            foreach (GameStatisticsByPlayer statsByPlayer in SelectedGameStatistics.Players)
            {
                foreach (SpecialStatisticsRow row in rows)
                {
                    int value = 0;
                    if (statsByPlayer.SpecialsUsed != null && statsByPlayer.SpecialsUsed.ContainsKey(row.Special))
                    {
                        KeyValuePair<string, int> kv = statsByPlayer.SpecialsUsed[row.Special].FirstOrDefault(x => x.Key == SelectedPlayer);
                        value = kv.Equals(default(KeyValuePair<string, int>)) ? 0 : kv.Value;
                    }
                    row.TryAddProperty(statsByPlayer.PlayerName, value);
                }
            }
            SpecialsToSelectedPlayerGrid = new DynamicGrid<SpecialStatisticsRow, DynamicColumn>(rows, columns);
        }
    }

    public class GameStatisticsViewModelDesignData : GameStatisticsViewModel
    {
        public GameStatisticsViewModelDesignData()
        {
            GameStatisticsHistory = new ObservableCollection<GameStatistics>
                {
                    new GameStatistics
                        {
                            GameStarted = DateTime.Now.AddMinutes(-10),
                            GameFinished = DateTime.Now.AddMinutes(-5),
                            Players = new List<GameStatisticsByPlayer>
                                {
                                    new GameStatisticsByPlayer
                                        {
                                            PlayerName = "Player1",
                                            SingleCount = 4,
                                            DoubleCount = 3,
                                            TripleCount = 2,
                                            TetrisCount = 1,
                                        },
                                    new GameStatisticsByPlayer
                                        {
                                            PlayerName = "Player2",
                                            SingleCount = 8,
                                            DoubleCount = 6,
                                            TripleCount = 4,
                                            TetrisCount = 0,
                                        },
                                    new GameStatisticsByPlayer
                                        {
                                            PlayerName = "Player3",
                                            SingleCount = 1,
                                            DoubleCount = 1,
                                            TripleCount = 0,
                                            TetrisCount = 0,
                                        },
                                    new GameStatisticsByPlayer
                                        {
                                            PlayerName = "Player4",
                                            SingleCount = 0,
                                            DoubleCount = 0,
                                            TripleCount = 0,
                                            TetrisCount = 1,
                                        },
                                    new GameStatisticsByPlayer
                                        {
                                            PlayerName = "Player5",
                                            SingleCount = 12,
                                            DoubleCount = 7,
                                            TripleCount = 0,
                                            TetrisCount = 0,
                                        },
                                    //new GameStatisticsByPlayer
                                    //{
                                    //    PlayerName = "Player6",
                                    //    SingleCount = 20,
                                    //    DoubleCount = 17,
                                    //    TripleCount = 5,
                                    //    TetrisCount = 2,
                                    //},
                                }
                        },
                        new GameStatistics
                        {
                            GameStarted = DateTime.Now.AddMinutes(-20),
                            GameFinished = DateTime.Now.AddMinutes(-18),
                        },
                        new GameStatistics
                        {
                            GameStarted = DateTime.Now.AddMinutes(-30),
                            GameFinished = DateTime.Now.AddMinutes(-22),
                        }
                };
            SelectedGameStatistics = GameStatisticsHistory[0];

            PlayerList = new List<string>
                {
                    "Player1",
                    "Player2withaverylongname",
                    "Player3",
                };
            SelectedPlayer = PlayerList[2];
            List<DynamicColumn> columns = new List<DynamicColumn>
                {
                    new DynamicColumn
                        {
                            Name = "Player1",
                            DisplayName = "Player1",
                            Type = typeof(string),
                            IsReadOnly = true,
                        },
                        new DynamicColumn
                        {
                            Name = "Player2withaverylongname",
                            DisplayName = "Player2withaverylongname",
                            Type = typeof(string),
                            IsReadOnly = true,
                        },
                        new DynamicColumn
                        {
                            Name = "Player3",
                            DisplayName = "Player3",
                            Type = typeof(string),
                            IsReadOnly = true,
                        },
                        new DynamicColumn
                        {
                            Name = "Player4",
                            DisplayName = "Player4",
                            Type = typeof(string),
                            IsReadOnly = true,
                        },
                        new DynamicColumn
                        {
                            Name = "Player5",
                            DisplayName = "Player5",
                            Type = typeof(string),
                            IsReadOnly = true,
                        },
                        new DynamicColumn
                        {
                            Name = "Player6",
                            DisplayName = "Player6",
                            Type = typeof(string),
                            IsReadOnly = true,
                        },
                };
            List<SpecialStatisticsRow> rows = new List<SpecialStatisticsRow>();
            foreach (Specials special in EnumHelper.GetSpecials(b => b))
            {
                SpecialStatisticsRow row = new SpecialStatisticsRow
                    {
                        Special = special
                    };
                foreach (DynamicColumn column in columns)
                {
                    int value = Randomizer.Instance.Next(20);
                    row.TryAddProperty(column.Name, value);
                }
                rows.Add(row);
            }
            SpecialsFromSelectedPlayerGrid = new DynamicGrid<SpecialStatisticsRow, DynamicColumn>(rows, columns);
            SpecialsToSelectedPlayerGrid = new DynamicGrid<SpecialStatisticsRow, DynamicColumn>(rows, columns);
        }
    }
}
