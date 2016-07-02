﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;

namespace TetriNET.ConsoleWCFClient.AI
{
    public class ColinFaheyTwoPiecesBot : IBot
    {
        private readonly Timer _timer;
        private bool _activated;

        public ColinFaheyTwoPiecesBot(IClient client)
        {
            Client = client;

            _timer = new Timer(10);
            _timer.Elapsed += _timer_Elapsed;

            SpecialStrategy = new SinaCSpecials();
            MoveStrategy = new ColinFaheyTwoPieces();

            _activated = false;
            SleepTime = 50;

            Client.RoundStarted += _client_OnRoundStarted;
            Client.GameStarted += client_OnGameStarted;
            Client.GameFinished += _client_OnGameFinished;
            Client.GameOver += _client_OnGameOver;
            Client.GamePaused += _client_OnGamePaused;
            Client.GameResumed += _client_OnGameResumed;
        }

        #region IBot

        public string Name => "Colin Fahey 2 pieces";

        public ISpecialStrategy SpecialStrategy { get; }
        public IMoveStrategy MoveStrategy { get; }
        public IClient Client { get; }

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

        private void _client_OnGameFinished(GameStatistics statistics)
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
                        continueLoop = Client.UseFirstSpecial(Client.PlayerId);
                        break;
                    case SpecialAdvice.SpecialAdviceActions.UseOpponent:
                        continueLoop = Client.UseFirstSpecial(advice.OpponentId);
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
            Log.Default.WriteLine(LogLevels.Info, "BEST MOVE found in {0} ms and special in {1} ms", (searchBestModeEndTime - specialManaged).TotalMilliseconds, (specialManaged - searchBestMoveStartTime).TotalMilliseconds);
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
