using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TetriNET.Common.Interfaces;
using TetriNET.Strategy;

namespace TetriNET.WPF_WCF_Client.AI
{
    public class PierreDellacherieOnePieceBot
    {
        private readonly SinaCSpecials _specialStrategy;
        private readonly PierreDellacherieOnePiece _moveStrategy;
        private readonly IClient _client;
        private readonly Timer _timer;
        private bool _activated;

        public PierreDellacherieOnePieceBot(IClient client)
        {
            _client = client;

            _timer = new Timer(100);
            _timer.Elapsed += _timer_Elapsed;

            _specialStrategy = new SinaCSpecials();
            _moveStrategy = new PierreDellacherieOnePiece();

            _activated = false;
            SleepTime = 100;

            _client.OnRoundStarted += _client_OnRoundStarted;
            _client.OnGameStarted += client_OnGameStarted;
            _client.OnGameFinished += _client_OnGameFinished;
            _client.OnGameOver += _client_OnGameOver;
            _client.OnGamePaused += _client_OnGamePaused;
            _client.OnGameResumed += _client_OnGameResumed;
        }

        public bool Activated
        {
            get { return _activated; }
            set
            {
                _activated = value;
                if (_activated)
                    _timer.Start();
                else
                    _timer.Stop();
            }
        }

        public int SleepTime { get; set; }

        private void _client_OnRoundStarted()
        {
            if (Activated)
                _timer.Start();
        }

        private void client_OnGameStarted()
        {
            if (Activated)
                _timer.Start();
        }

        private void _client_OnGameFinished()
        {
            _timer.Stop();
        }

        private void _client_OnGameOver()
        {
            _timer.Stop();
        }

        private void _client_OnGamePaused()
        {
            _timer.Stop();
        }

        private void _client_OnGameResumed()
        {
            if (Activated)
                _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();

            if (_client.Board == null || _client.CurrentTetrimino == null || _client.NextTetrimino == null)
                return;

            DateTime searchBestMoveStartTime = DateTime.Now;

            // Use specials
            List<SpecialAdvices> advices = new List<SpecialAdvices>();
            _specialStrategy.GetSpecialAdvice(_client.Board, _client.CurrentTetrimino, _client.NextTetrimino, _client.Inventory, _client.InventorySize, _client.Opponents.ToList(), out advices);
            // TODO: use advices

            DateTime specialManaged = DateTime.Now;

            // Get best move
            int bestRotationDelta;
            int bestTranslationDelta;
            _moveStrategy.GetBestMove(_client.Board, _client.CurrentTetrimino, _client.NextTetrimino, out bestRotationDelta, out bestTranslationDelta);

            // Perform move

            // ROTATE
            for (int rotateCount = 0; rotateCount < bestRotationDelta; rotateCount++)
                _client.RotateClockwise();

            // TRANSLATE
            if (bestTranslationDelta < 0)
                for (int translateCount = 0; translateCount > bestTranslationDelta; translateCount--)
                    _client.MoveLeft();
            if (bestTranslationDelta > 0)
                for (int translateCount = 0; translateCount < bestTranslationDelta; translateCount++)
                    _client.MoveRight();

            // DROP (delayed)
            DateTime searchBestModeEndTime = DateTime.Now;

            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "BEST MOVE found in {0} ms and special in {1} ms", (searchBestModeEndTime - specialManaged).TotalMilliseconds, (specialManaged - searchBestMoveStartTime).TotalMilliseconds);

            TimeSpan timeSpan = searchBestModeEndTime - searchBestMoveStartTime;
            double sleepTime = SleepTime - timeSpan.TotalMilliseconds;
            if (sleepTime <= 0)
                sleepTime = 10;
            System.Threading.Thread.Sleep((int) sleepTime); // delay drop instead of animating
            _client.Drop();
        }
    }
}
