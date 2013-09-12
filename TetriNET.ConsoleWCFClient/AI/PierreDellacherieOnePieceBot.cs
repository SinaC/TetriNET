using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TetriNET.Common.Interfaces;
using TetriNET.Strategy;

namespace TetriNET.ConsoleWCFClient.AI
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

            _timer = new Timer(10);
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
            //List<SpecialAdvices> advices;
            //_specialStrategy.GetSpecialAdvice(_client.Board, _client.CurrentTetrimino, _client.NextTetrimino, _client.Inventory, _client.InventorySize, _client.Opponents.ToList(), out advices);
            //foreach (SpecialAdvices advice in advices)
            //{
            //    bool continueLoop = true;
            //    switch (advice.SpecialAdviceAction)
            //    {
            //        case SpecialAdvices.SpecialAdviceActions.Wait:
            //            continueLoop = false;
            //            break;
            //        case SpecialAdvices.SpecialAdviceActions.Discard:
            //            _client.DiscardFirstSpecial();
            //            continueLoop = true;
            //            break;
            //        case SpecialAdvices.SpecialAdviceActions.UseSelf:
            //            continueLoop = _client.UseSpecial(_client.PlayerId);
            //            break;
            //        case SpecialAdvices.SpecialAdviceActions.UseOpponent:
            //            continueLoop = _client.UseSpecial(advice.OpponentId);
            //            break;
            //    }
            //    if (!continueLoop)
            //        break;
            //    System.Threading.Thread.Sleep(10); // delay next special use
            //}

            DateTime specialManaged = DateTime.Now;

            // Get best move
            int bestRotationDelta;
            int bestTranslationDelta;
            bool rotationBeforeTranslation;
            _moveStrategy.GetBestMove(_client.Board, _client.CurrentTetrimino, _client.NextTetrimino, out bestRotationDelta, out bestTranslationDelta, out rotationBeforeTranslation);

            DateTime searchBestModeEndTime = DateTime.Now;

            // Perform move
            if (rotationBeforeTranslation)
            {
                // Rotate
                Rotate(bestRotationDelta);
                // Translate
                Translate(bestTranslationDelta);
            }
            else
            {
                // Translate
                Translate(bestTranslationDelta);
                // Rotate
                Rotate(bestRotationDelta);
            }
            // Drop (delayed)
            TimeSpan timeSpan = DateTime.Now - searchBestMoveStartTime;
            double sleepTime = SleepTime - timeSpan.TotalMilliseconds;
            if (sleepTime <= 0)
                sleepTime = 10;
            System.Threading.Thread.Sleep((int) sleepTime); // delay drop instead of animating
            _client.Drop();
            //
            Logger.Log.WriteLine(Logger.Log.LogLevels.Info, "BEST MOVE found in {0} ms and special in {1} ms", (searchBestModeEndTime - specialManaged).TotalMilliseconds, (specialManaged - searchBestMoveStartTime).TotalMilliseconds);
        }

        private void Rotate(int rotationDelta)
        {
            for (int rotateCount = 0; rotateCount < rotationDelta; rotateCount++)
                _client.RotateClockwise();
        }

        private void Translate(int translationDelta)
        {
            if (translationDelta < 0)
                for (int translateCount = 0; translateCount > translationDelta; translateCount--)
                    _client.MoveLeft();
            if (translationDelta > 0)
                for (int translateCount = 0; translateCount < translationDelta; translateCount++)
                    _client.MoveRight();

        }
    }
}
