using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.ViewModels.Statistics
{
    public class GameStatisticsViewModel : ViewModelBase
    {
        private GameStatistics _gameStatistics;

        private List<string> _playerList;
        public List<string> PlayerList
        {
            get { return _playerList; }
            set
            {
                if (_playerList != value)
                {
                    _playerList = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _selectedPlayer;
        public string SelectedPlayer
        {
            get { return _selectedPlayer; }
            set
            {
                if (_selectedPlayer != value)
                {
                    _selectedPlayer = value;
                    OnPropertyChanged();
                }
            }
        }

        // TODO: dynamic grid initialized when SelectedPlayer is modified  rows: special  column: player name   value: special count

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
            _gameStatistics = statistics;

            PlayerList = _gameStatistics.Players.Select(x => x.PlayerName).ToList();
            SelectedPlayer = PlayerList.FirstOrDefault(x => x == Client.Name);
        }

        #endregion
    }

    public class GameStatisticsViewModelDesignData : GameStatisticsViewModel
    {
        public GameStatisticsViewModelDesignData()
        {
            // TODO
        }
    }
}
