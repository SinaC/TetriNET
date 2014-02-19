using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class PlayFieldSpectatorViewModel : PlayFieldViewModelBase
    {
        private const int OpponentCount = 6;

        public InGameChatViewModel InGameChatViewModel { get; set; }
        public OpponentViewModel[] OpponentsViewModel { get; set; }
        
        public PlayFieldSpectatorViewModel()
        {
            InGameChatViewModel = new InGameChatViewModel();
            OpponentsViewModel = new OpponentViewModel[OpponentCount];
            for (int i = 0; i < OpponentCount; i++)
                OpponentsViewModel[i] = new OpponentViewModel();

            ClientChanged += OnClientChanged;
        }

        #region ViewModelBase

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            InGameChatViewModel.Client = newClient;
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
            if (!Client.IsSpectator)
                return;

            OpponentViewModel opponent = OpponentsViewModel[playerId];
            if (opponent != null)
            {
                opponent.PlayerId = -1;
                opponent.PlayerName = "Not playing";
                opponent.Team = "";
            }
        }

        private void OnPlayerJoined(int playerId, string playerName, string team)
        {
            if (!Client.IsSpectator)
                return;

            OpponentViewModel opponent = OpponentsViewModel[playerId];
            if (opponent != null)
            {
                opponent.PlayerId = playerId;
                opponent.PlayerName = playerName;
                opponent.Team = team;
            }
        }

        #endregion
    }

    public class PlayFieldSpectatorViewModelDesignData : PlayFieldSpectatorViewModel
    {
        public PlayFieldSpectatorViewModelDesignData()
        {
            InGameChatViewModel = new InGameChatViewModelDesignData();
        }
    }
}
