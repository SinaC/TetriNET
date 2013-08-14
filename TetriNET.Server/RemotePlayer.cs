using System;
using System.Collections.Generic;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class RemotePlayer : IPlayer
    {
        public RemotePlayer(string name, ITetriNETCallback callback)
        {
            Name = name;
            Callback = callback;
            TetriminoIndex = 0;
            LastAction = DateTime.Now;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                LastAction = DateTime.Now; // if action didn't raise an exception, client is still alive
            }
            catch (CommunicationObjectAbortedException)
            {
                Log.WriteLine("CommunicationObjectAbortedException:" + actionName);
                if (OnConnectionLost != null)
                    OnConnectionLost(this);
            }
            catch (Exception)
            {
                Log.WriteLine("Exception:" + actionName);
                if (OnConnectionLost != null)
                    OnConnectionLost(this);
            }
        }

        #region IPlayer

        public event ConnectionLostHandler OnConnectionLost;

        public string Name { get; private set; }
        public int TetriminoIndex { get; set; }
        public DateTime LastAction { get; set; }
        public ITetriNETCallback Callback { get; private set; }

        public void OnPingReceived()
        {
            ExceptionFreeAction(() => Callback.OnPingReceived(), "OnPingReceived");
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped(), "OnServerStopped");
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerRegistered(succeeded, playerId), "OnPlayerRegistered");
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(firstTetrimino, secondTetrimino, players), "OnGameStarted");
        }

        public void OnGameFinished()
        {
            ExceptionFreeAction(() => Callback.OnGameFinished(), "OnGameFinished");
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnServerAddLines(lineCount), "OnServerAddLines");
        }

        public void OnPlayerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnPlayerAddLines(lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnAttackReceived(Attacks attack)
        {
            ExceptionFreeAction(() => Callback.OnAttackReceived(attack), "OnAttackReceived");
        }

        public void OnAttackMessageReceived(string msg)
        {
            ExceptionFreeAction(() => Callback.OnAttackMessageReceived(msg), "OnAttackMessageReceived");
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ExceptionFreeAction(() => Callback.OnNextTetrimino(index, tetrimino), "OnNextTetrimino");
        }

        #endregion
    }
}
