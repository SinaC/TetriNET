using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.Game
{
    public class GameViewModel : ViewModelBase
    {
        public GameInfoViewModel GameInfoViewModel { get; set; }
        public InGameChatViewModel InGameChatViewModel { get; set; }

        public GameViewModel()
        {
            GameInfoViewModel = new GameInfoViewModel();
            InGameChatViewModel = new InGameChatViewModel();
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            // TODO
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            // TODO
        }

        public override void OnClientAssigned(IClient newClient)
        {
            GameInfoViewModel.Client = newClient;
            InGameChatViewModel.Client = newClient;
        }
        #endregion
    }
}
