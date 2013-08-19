using System;
using System.Collections.Generic;
using TetriNET.Common;
using TetriNET.Common.Contracts;

namespace TetriNET.Server.Player
{
    public sealed class Player : IPlayer
    {
        public Player(string name, ITetriNETCallback callback)
        {
            Name = name;
            Callback = callback;
            TetriminoIndex = 0;
            LastAction = DateTime.Now;
            State = PlayerStates.Registered;
            TimeoutCount = 0;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                //RefreshLastAction(); // if action didn't raise an exception, client is still alive // TODO: only true with WCF
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
        public byte[] Grid { get; set; }
        //
        public ITetriNETCallback Callback { get; private set; }
        //
        public PlayerStates State { get; set; }
        public DateTime LossTime { get; set; }
        // Timeout management
        public DateTime LastAction { get; private set; }
        public int TimeoutCount { get; private set; }

        public void ResetTimeout()
        {
            TimeoutCount = 0;
            LastAction = DateTime.Now;
        }

        public void SetTimeout()
        {
            TimeoutCount++;
            LastAction = DateTime.Now;
        }

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

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnPlayerAddLines(specialId, playerId, lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnSpecialUsed(specialId, playerId, targetId, special), "OnSpecialUsed");
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ExceptionFreeAction(() => Callback.OnNextTetrimino(index, tetrimino), "OnNextTetrimino");
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            ExceptionFreeAction(() => Callback.OnGridModified(playerId, grid), "OnGridModified");
        }

        public void OnServerMasterChanged(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnServerMasterChanged(playerId), "OnServerMasterChanged");
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            ExceptionFreeAction(() => Callback.OnWinListModified(winList), "OnWinListModified");
        }

        #endregion
    }
}
