using System;
using System.Collections.Generic;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class BuiltInPlayer : IPlayer
    {
        public BuiltInPlayer(string name, ITetriNETCallback callback)
        {
            Name = name;
            Callback = callback;
            TetriminoIndex = 0;
            LastAction = DateTime.Now;
        }

        private void UpdateTimerOnAction(Action action)
        {
            action();
            LastAction = DateTime.Now;
        }

        #region IPlayer

        public event ConnectionLostHandler OnConnectionLost;

        public string Name { get; private set; }
        public int TetriminoIndex { get; set; }
        public DateTime LastAction { get; set; }
        public ITetriNETCallback Callback { get; private set; }

        public void OnPingReceived()
        {
            UpdateTimerOnAction(Callback.OnPingReceived);
        }

        public void OnServerStopped()
        {
            UpdateTimerOnAction(Callback.OnServerStopped);
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            UpdateTimerOnAction(() => Callback.OnPlayerRegistered(succeeded, playerId));
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
        {
            UpdateTimerOnAction(() => Callback.OnGameStarted(firstTetrimino, secondTetrimino, players));
        }

        public void OnGameFinished()
        {
            UpdateTimerOnAction(Callback.OnGameFinished);
        }

        public void OnServerAddLines(int lineCount)
        {
            UpdateTimerOnAction(() => Callback.OnServerAddLines(lineCount));
        }

        public void OnPlayerAddLines(int lineCount)
        {
            UpdateTimerOnAction(() => Callback.OnPlayerAddLines(lineCount));
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            UpdateTimerOnAction(() => Callback.OnPublishPlayerMessage(playerName, msg));
        }

        public void OnPublishServerMessage(string msg)
        {
            UpdateTimerOnAction(() => Callback.OnPublishServerMessage(msg));
        }

        public void OnAttackReceived(Attacks attack)
        {
            UpdateTimerOnAction(() => Callback.OnAttackReceived(attack));
        }

        public void OnAttackMessageReceived(string msg)
        {
            UpdateTimerOnAction(() => Callback.OnAttackMessageReceived(msg));
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            UpdateTimerOnAction(() => Callback.OnNextTetrimino(index, tetrimino));
        }

        #endregion
    }
}
