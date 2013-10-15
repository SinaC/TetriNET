using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
using TetriNET.Common.Logger;

namespace TetriNET.ConsoleWCFClient.AI
{
    public class PierreDellacherieOnePieceBot : IBot
    {
        private readonly Timer _timer;
        private bool _activated;

        public PierreDellacherieOnePieceBot(IClient client)
        {
            Client = client;

            _timer = new Timer(10);
            _timer.Elapsed += _timer_Elapsed;

            SpecialStrategy = new SinaCSpecials();
            MoveStrategy = new PierreDellacherieOnePiece();

            _activated = false;
            SleepTime = 50;

            Client.OnRoundStarted += _client_OnRoundStarted;
            Client.OnGameStarted += client_OnGameStarted;
            Client.OnGameFinished += _client_OnGameFinished;
            Client.OnGameOver += _client_OnGameOver;
            Client.OnGamePaused += _client_OnGamePaused;
            Client.OnGameResumed += _client_OnGameResumed;
        }

        #region IBot

        public string Name
        {
            get { return "Pierre Dellacherie 1 piece"; }
        }

        public ISpecialStrategy SpecialStrategy { get; private set; }
        public IMoveStrategy MoveStrategy { get; private set; }
        public IClient Client { get; private set; }

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

        #endregion

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

            if (Client.IsRegistered && Client.Board == null || Client.CurrentPiece == null || Client.NextPiece == null)
                return;

            DateTime searchBestMoveStartTime = DateTime.Now;

            // Use specials
            List<SpecialAdvice> advices;
            SpecialStrategy.GetSpecialAdvices(Client.Board, Client.CurrentPiece, Client.NextPiece, Client.Inventory, Client.InventorySize, Client.Opponents.ToList(), out advices);
            foreach (SpecialAdvice advice in advices)
            {
                bool continueLoop = true;
                switch (advice.SpecialAdviceAction)
                {
                    case SpecialAdvice.SpecialAdviceActions.Wait:
                        continueLoop = false;
                        break;
                    case SpecialAdvice.SpecialAdviceActions.Discard:
                        Client.DiscardFirstSpecial();
                        continueLoop = true;
                        break;
                    case SpecialAdvice.SpecialAdviceActions.UseSelf:
                        continueLoop = Client.UseSpecial(Client.PlayerId);
                        break;
                    case SpecialAdvice.SpecialAdviceActions.UseOpponent:
                        continueLoop = Client.UseSpecial(advice.OpponentId);
                        break;
                }
                if (!continueLoop)
                    break;
                System.Threading.Thread.Sleep(10); // delay next special use
            }

            DateTime specialManaged = DateTime.Now;

            // Get best move
            int bestRotationDelta;
            int bestTranslationDelta;
            bool rotationBeforeTranslation;
            MoveStrategy.GetBestMove(Client.Board, Client.CurrentPiece, Client.NextPiece, out bestRotationDelta, out bestTranslationDelta, out rotationBeforeTranslation);

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
            Client.Drop();
            //
            Log.WriteLine(Log.LogLevels.Info, "BEST MOVE found in {0} ms and special in {1} ms", (searchBestModeEndTime - specialManaged).TotalMilliseconds, (specialManaged - searchBestMoveStartTime).TotalMilliseconds);
        }

        private void Rotate(int rotationDelta)
        {
            for (int rotateCount = 0; rotateCount < rotationDelta; rotateCount++)
                Client.RotateClockwise();
        }

        private void Translate(int translationDelta)
        {
            if (translationDelta < 0)
                for (int translateCount = 0; translateCount > translationDelta; translateCount--)
                    Client.MoveLeft();
            if (translationDelta > 0)
                for (int translateCount = 0; translateCount < translationDelta; translateCount++)
                    Client.MoveRight();

        }
    }
}
