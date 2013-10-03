using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    // PlayerViewModel not used
    public class PlayFieldViewModel : ViewModelBase, ITabIndex
    {
        public int OpponentCount = 5;

        public GameInfoViewModel GameInfoViewModel { get; set; }
        public InGameChatViewModel InGameChatViewModel { get; set; }
        public PlayerViewModel PlayerViewModel { get; set; }
        public OpponentViewModel[] OpponentsViewModel { get; set; }

        public PlayFieldViewModel()
        {
            GameInfoViewModel = new GameInfoViewModel();
            InGameChatViewModel = new InGameChatViewModel();
            PlayerViewModel = new PlayerViewModel();
            OpponentsViewModel = new OpponentViewModel[OpponentCount];
            for (int i = 0; i < OpponentCount; i++)
                OpponentsViewModel[i] = new OpponentViewModel();

            ClientChanged += OnClientChanged;
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
}