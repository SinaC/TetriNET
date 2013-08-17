using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class Player : IPlayer
    {
        public Player(string name, ITetriNETCallback callback)
        {
            Name = name;
            Callback = callback;
            TetriminoIndex = 0;
            LastAction = DateTime.Now;
            State = PlayerStates.Registered;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                LastAction = DateTime.Now; // if action didn't raise an exception, client is still alive
            }
            //catch (CommunicationObjectAbortedException)
            //{
            //    Log.WriteLine("CommunicationObjectAbortedException:{0}", actionName);
            //    if (OnConnectionLost != null)
            //        OnConnectionLost(this);
            //}
            catch (Exception)
            {
                Log.WriteLine("Exception:{0}", actionName);
                if (OnConnectionLost != null)
                    OnConnectionLost(this);
            }
        }

        #region IPlayer

        public event ConnectionLostHandler OnConnectionLost;

        public string Name { get; private set; }
        public int TetriminoIndex { get; set; }
        public PlayerGrid Grid { get; set; }
        public DateTime LastAction { get; set; }
        public ITetriNETCallback Callback { get; private set; }
        public PlayerStates State { get; set; }

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            ExceptionFreeAction(() => Callback.OnHeartbeatReceived(), "OnHeartbeatReceived");
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped(), "OnServerStopped");
        }

        public void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted)
        {
            ExceptionFreeAction(() => Callback.OnPlayerRegistered(succeeded, playerId, gameStarted), "OnPlayerRegistered");
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            ExceptionFreeAction(() => Callback.OnPlayerJoined(playerId, name), "OnPlayerJoined");
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLeft(playerId, name, reason), "OnPlayerLeft");
        }

        public void OnPlayerLost(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLost(playerId), "OnPlayerLost");
        }

        public void OnPlayerWon(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerWon(playerId), "OnPlayerWon");
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(firstTetrimino, secondTetrimino, options), "OnGameStarted");
        }

        public void OnGameFinished()
        {
            ExceptionFreeAction(() => Callback.OnGameFinished(), "OnGameFinished");
        }

        public void OnGamePaused()
        {
            ExceptionFreeAction(() => Callback.OnGamePaused(), "OnGamePaused");
        }

        public void OnGameResumed()
        {
            ExceptionFreeAction(() => Callback.OnGameResumed(), "OnGameResumed");
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnServerAddLines(lineCount), "OnServerAddLines");
        }

        public void OnPlayerAddLines(int attackId, int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnPlayerAddLines(attackId, lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnPublishAttackMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishAttackMessage(msg), "OnPublishAttackMessage");
        }

        public void OnAttackReceived(int attackId, Attacks attack)
        {
            ExceptionFreeAction(() => Callback.OnAttackReceived(attackId, attack), "OnAttackReceived");
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ExceptionFreeAction(() => Callback.OnNextTetrimino(index, tetrimino), "OnNextTetrimino");
        }

        public void OnGridModified(int playerId, PlayerGrid grid)
        {
            ExceptionFreeAction(() => Callback.OnGridModified(playerId, grid), "OnGridModified");
        }

        public void OnServerMasterChanged(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnServerMasterChanged(playerId), "OnServerMasterChanged");
        }

        #endregion
    }
}
