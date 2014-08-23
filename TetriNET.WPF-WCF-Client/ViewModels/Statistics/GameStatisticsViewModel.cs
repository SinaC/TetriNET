using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.DynamicGrid;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class SpecialRow : DynamicRow
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
        private GameStatistics _gameStatistics;
        public GameStatistics GameStatistics
        {
            get { return _gameStatistics; }
            set
            {
                if (Set(() => GameStatistics, ref _gameStatistics, value))
                {
                    OnPropertyChanged("First3Players");
                    OnPropertyChanged("Next3Players");
                    OnPropertyChanged("IsNextPlayersNeeded");
                    OnPropertyChanged("MatchTime");
                }
            }
        }

        public List<GameStatisticsByPlayer> First3Players
        {
            get { return _gameStatistics == null ? Enumerable.Empty<GameStatisticsByPlayer>().ToList() : _gameStatistics.Players.Take(3).ToList(); }
        }

        public List<GameStatisticsByPlayer> Next3Players
        {
            get { return _gameStatistics == null ? Enumerable.Empty<GameStatisticsByPlayer>().ToList() : _gameStatistics.Players.Skip(3).Take(3).ToList(); }
        }

        public bool IsNextPlayersNeeded
        {
            get { return _gameStatistics != null && _gameStatistics.Players.Count > 3; }
        }

        public double MatchTime // In seconds
        {
            get { return _gameStatistics == null ? 0 : _gameStatistics.MatchTime; }
        }

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

        private DynamicGrid<SpecialRow, DynamicColumn> _specialsFromSelectedPlayerGrid; // Columns: player  Rows: special
        public DynamicGrid<SpecialRow, DynamicColumn> SpecialsFromSelectedPlayerGrid
        {
            get { return _specialsFromSelectedPlayerGrid; }
            set { Set(() => SpecialsFromSelectedPlayerGrid, ref _specialsFromSelectedPlayerGrid, value); }
        }

        private DynamicGrid<SpecialRow, DynamicColumn> _specialsToSelectedPlayerGrid; // Columns: player  Rows: special
        public DynamicGrid<SpecialRow, DynamicColumn> SpecialsToSelectedPlayerGrid
        {
            get { return _specialsToSelectedPlayerGrid; }
            set { Set(() => SpecialsToSelectedPlayerGrid, ref _specialsToSelectedPlayerGrid, value); }
        }

        public GameStatisticsViewModel()
        {
            //GameStatistics = new GameStatistics
            //{
            //    MatchTime = 123.456,
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
            GameStatistics = statistics;

            PlayerList = GameStatistics.Players.Select(x => x.PlayerName).ToList();
            SelectedPlayer = PlayerList.FirstOrDefault(x => x == Client.Name);
        }

        #endregion

        private void InitializeSpecialsGrid()
        {
            if (SelectedPlayer == null || GameStatistics == null || GameStatistics.Players == null)
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
            GameStatisticsByPlayer byPlayer = GameStatistics.Players.FirstOrDefault(x => x.PlayerName == SelectedPlayer);
            if (byPlayer == null || byPlayer.SpecialsUsed == null)
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
                List<SpecialRow> rows = new List<SpecialRow>();
                foreach (KeyValuePair<Specials, Dictionary<string, int>> specials in byPlayer.SpecialsUsed.OrderBy(kv => kv.Key))
                {
                    SpecialRow row = new SpecialRow
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
                SpecialsFromSelectedPlayerGrid = new DynamicGrid<SpecialRow, DynamicColumn>(rows, columns);
            }
        }

        private void InitializeToSelectedPlayer()
        {
            List<DynamicColumn> columns = GameStatistics.Players.Select(x => new DynamicColumn
                {
                    Name = x.PlayerName,
                    DisplayName = x.PlayerName,
                    Type = typeof(int),
                    IsReadOnly = true,
                }).ToList();
            List<Specials> specials = GameStatistics.Players.Where(p => p.SpecialsUsed != null).SelectMany(p => p.SpecialsUsed.Select(kv => kv.Key)).Distinct().ToList();
            List<SpecialRow> rows = specials.Select(x => new SpecialRow
                {
                    Special = x
                }).ToList();
            foreach (GameStatisticsByPlayer statsByPlayer in GameStatistics.Players)
            {
                foreach (SpecialRow row in rows)
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
            SpecialsToSelectedPlayerGrid = new DynamicGrid<SpecialRow, DynamicColumn>(rows, columns);
        }
    }

    public class GameStatisticsViewModelDesignData : GameStatisticsViewModel
    {
        public GameStatisticsViewModelDesignData()
        {
            GameStatistics = new GameStatistics
                {
                    MatchTime = 123.456,
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
                };
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
            List<SpecialRow> rows = new List<SpecialRow>();
            Random random = new Random();
            foreach(Specials special in EnumHelper.GetSpecials(b => b))
            {
                SpecialRow row = new SpecialRow
                    {
                        Special = special
                    };
                foreach(DynamicColumn column in columns)
                {
                    int value = random.Next(20);
                    row.TryAddProperty(column.Name, value);
                }
                rows.Add(row);
            }
            SpecialsFromSelectedPlayerGrid = new DynamicGrid<SpecialRow, DynamicColumn>(rows, columns);
            SpecialsToSelectedPlayerGrid = new DynamicGrid<SpecialRow, DynamicColumn>(rows, columns);
        }
    }
}
