//using TetriNET.Common.Interfaces;

//namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
//{
//    public class GameInfoViewModel : ViewModelBase
//    {
//        private int _level;
//        public int Level
//        {
//            get { return _level; }
//            set
//            {
//                if (_level != value)
//                {
//                    _level = value;
//                    OnPropertyChanged();
//                }
//            }
//        }

//        private int _linesCleared;
//        public int LinesCleared
//        {
//            get { return _linesCleared; }
//            set
//            {
//                if (_linesCleared != value)
//                {
//                    _linesCleared = value;
//                    OnPropertyChanged();
//                }
//            }
//        }

//        private void DisplayLevel()
//        {
//            Level = Client.Level;
//        }

//        private void DisplayClearedLines()
//        {
//            LinesCleared = Client.LinesCleared;
//        }

//        #region ViewModelBase
//        public override void UnsubscribeFromClientEvents(IClient oldClient)
//        {
//            oldClient.OnLinesClearedChanged -= OnLinesClearedChanged;
//            oldClient.OnLevelChanged -= OnLevelChanged;
//            oldClient.OnGameStarted -= OnGameStarted;
//        }

//        public override void SubscribeToClientEvents(IClient newClient)
//        {
//            newClient.OnLinesClearedChanged += OnLinesClearedChanged;
//            newClient.OnLevelChanged += OnLevelChanged;
//            newClient.OnGameStarted += OnGameStarted;
//        }
//        #endregion

//        #region IClient events handler
//        private void OnGameStarted()
//        {
//            DisplayLevel();
//            DisplayClearedLines();
//        }

//        private void OnLevelChanged()
//        {
//            DisplayLevel();
//        }

//        private void OnLinesClearedChanged()
//        {
//            DisplayClearedLines();
//        }
//        #endregion
//    }
//}
