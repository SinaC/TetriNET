using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels.Options
{
    public class ClientOptionsViewModel : ViewModelBase
    {
        private bool _isGameNotStarted;
        public bool IsGameNotStarted
        {
            get { return _isGameNotStarted; }
            set
            {
                if (_isGameNotStarted != value)
                {
                    _isGameNotStarted = value;
                    OnPropertyChanged();
                }
            }
        }

        public Models.Options Options
        {
            get { return Models.Options.OptionsSingleton.Instance; }
        }

        public ClientOptionsViewModel()
        {
            IsGameNotStarted = true;
            Options.IsDeveloperModeActivated = false;
        }

        #region ViewModelBase

        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameFinished -= OnGameFinished;
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnPlayerUnregistered -= OnPlayerUnregister;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameFinished += OnGameFinished;
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnPlayerUnregistered += OnPlayerUnregister;
        }

        #endregion

        #region IClient events handler

        private void OnPlayerUnregister()
        {
            IsGameNotStarted = true;
        }

        private void OnGameStarted()
        {
            IsGameNotStarted = false;
            Models.Options.OptionsSingleton.Instance.ServerOptions = Client.Options;
            OnPropertyChanged("Options");
        }

        private void OnGameFinished()
        {
            IsGameNotStarted = true;
        }

        #endregion
    }
}
