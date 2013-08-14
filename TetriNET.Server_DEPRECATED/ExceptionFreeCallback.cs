using System;
using System.Collections.Generic;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class ExceptionFreeCallback : ITetriNETCallback
    {
        private readonly IPlayerManager _playerManager;
        private readonly ITetriNETCallback _callback;
        
        public ExceptionFreeCallback(ITetriNETCallback callback, IPlayerManager playerManager)
        {
            _callback = callback;
            _playerManager = playerManager;
        }

        public ITetriNETCallback Callback { get { return _callback; } }

        public event EventHandler<IPlayer> OnPlayerDisconnected;

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                IPlayer player = _playerManager[this];
                player.LastAction = DateTime.Now; // if action didn't raise an exception, client is still alive
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Log.WriteLine("CommunicationObjectAbortedException:" + actionName);
                IPlayer player = _playerManager[this];
                if (player != null && OnPlayerDisconnected != null)
                    OnPlayerDisconnected(this, player);
            }
        }

        public void OnPingReceived()
        {
            ExceptionFreeAction(() => _callback.OnPingReceived(), "OnPingReceived");
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => _callback.OnServerStopped(), "OnServerStopped");
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            ExceptionFreeAction(() => _callback.OnPlayerRegistered(succeeded, playerId), "OnPlayerRegistered");
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
        {
            ExceptionFreeAction(() => _callback.OnGameStarted(firstTetrimino, secondTetrimino, players), "OnGameStarted");
        }

        public void OnGameFinished()
        {
            ExceptionFreeAction(() => _callback.OnGameFinished(), "OnGameFinished");
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => _callback.OnServerAddLines(lineCount), "OnServerAddLines");
        }

        public void OnPlayerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => _callback.OnPlayerAddLines(lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => _callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => _callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnAttackReceived(Attacks attack)
        {
            ExceptionFreeAction(() => _callback.OnAttackReceived(attack), "OnAttackReceived");
        }

        public void OnAttackMessageReceived(string msg)
        {
            ExceptionFreeAction(() => _callback.OnAttackMessageReceived(msg), "OnAttackMessageReceived");
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            ExceptionFreeAction(() => _callback.OnNextTetrimino(index, tetrimino), "OnNextTetrimino");
        }
    }
}