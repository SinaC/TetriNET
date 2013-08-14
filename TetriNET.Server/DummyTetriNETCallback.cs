using System;
using System.Collections.Generic;
using TetriNET.Common;

namespace TetriNET.Server
{
    public class DummyTetriNETCallback : ITetriNETCallback
    {
        public void OnPingReceived()
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnServerStopped()
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnGameFinished()
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnServerAddLines(int lineCount)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnPlayerAddLines(int lineCount)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnPublishServerMessage(string msg)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnAttackReceived(Attacks attack)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnAttackMessageReceived(string msg)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Console.WriteLine("TO IMPLEMENT");
        }
    }
}
