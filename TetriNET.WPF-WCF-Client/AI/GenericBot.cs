using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Client.Strategy;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Logger;

namespace TetriNET.WPF_WCF_Client.AI
{
    public class GenericBot
    {
        private readonly IClient _client;
        private readonly ManualResetEvent _handleNextPieceEvent;
        private readonly ManualResetEvent _stopEvent;

        private bool _isConfusionActive;

        public ISpecialStrategy SpecialStrategy { get; private set; }
        public IMoveStrategy MoveStrategy { get; private set; }

        private bool _activated;
        public bool Activated { get { return _activated; }
            set
            {
                Log.WriteLine(Log.LogLevels.Debug, "Bot activation {0}", value);
                _activated = value;
                if (_activated)
                {
                    _handleNextPieceEvent.Set();
                    Task.Factory.StartNew(BotTask);
                }
                else
                    _stopEvent.Set();
            }
        }

        private int _sleepTime;
        public int SleepTime {
            get { return _sleepTime; }
            set
            {
                if (_sleepTime != value && value > 50)
                    _sleepTime = value;
            }
        }

        public GenericBot(IClient client, IMoveStrategy moveStrategy, ISpecialStrategy specialStrategy)
        {
            _client = client;

            SpecialStrategy = specialStrategy;
            MoveStrategy = moveStrategy;

            _client.OnRoundStarted += _client_OnRoundStarted;
            _client.OnGameStarted += client_OnGameStarted;
            _client.OnGameFinished += _client_OnGameFinished;
            _client.OnGameOver += _client_OnGameOver;
            _client.OnContinuousEffectToggled += _client_OnContinuousEffectToggled;

            _stopEvent = new ManualResetEvent(false);
            _handleNextPieceEvent = new ManualResetEvent(false);

            SleepTime = 75;
            Activated = false;
        }

        public void UnsubscribeFromClientEvents()
        {
            _client.OnRoundStarted -= _client_OnRoundStarted;
            _client.OnGameStarted -= client_OnGameStarted;
            _client.OnGameFinished -= _client_OnGameFinished;
            _client.OnGameOver -= _client_OnGameOver;
            _client.OnContinuousEffectToggled -= _client_OnContinuousEffectToggled;
        }

        private void _client_OnContinuousEffectToggled(Specials special, bool active, double duration)
        {
            if (special == Specials.Confusion)
            {
                _isConfusionActive = active;
                if (!_isConfusionActive)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "Confusion ended, raise next piece event");
                    _handleNextPieceEvent.Set();
                }
            }
        }

        private void _client_OnRoundStarted()
        {
            if (Activated)
            {
                Log.WriteLine(Log.LogLevels.Debug, "Raise next piece event");
                _handleNextPieceEvent.Set();
            }
        }

        private void client_OnGameStarted()
        {
            _isConfusionActive = false;
            if (Activated)
                _handleNextPieceEvent.Set();
        }

        private void _client_OnGameFinished()
        {
            //_stopEvent.Set();
        }

        private void _client_OnGameOver()
        {
            //_stopEvent.Set();
        }

        private void BotTask()
        {
            WaitHandle[] waitHandles =
            {
                _handleNextPieceEvent,
                _stopEvent
            };
            while (true)
            {
                int handle = WaitHandle.WaitAny(waitHandles, SleepTime);
                if (!_handleNextPieceEvent.WaitOne(0) && _client.IsPlaying && !_isConfusionActive)
                {
                    Log.WriteLine(Log.LogLevels.Warning, "!!!!!!!!!!! NextPieceEvent not raised  {0}  {1}", handle, _client.CurrentPiece == null ? -1 : _client.CurrentPiece.Index);
                }
                _handleNextPieceEvent.Reset();
                _stopEvent.Reset();

                if (handle == 1) // stop event
                    break;
                if (handle == 0 /*next piece event*/ && _client.IsPlaying && _client.Board != null && _client.CurrentPiece != null && _client.NextPiece != null)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "NextPieceEvent was raised");

                    int currentPieceIndex = _client.CurrentPiece.Index;
                    //Log.WriteLine(Log.LogLevels.Debug, "Searching best move for Piece {0} {1}", _client.CurrentPiece.Value, _client.CurrentPiece.Index);

                    DateTime searchBestMoveStartTime = DateTime.Now;

                    // Use specials
                    if (SpecialStrategy != null)
                    {
                        List<SpecialAdvice> advices;
                        SpecialStrategy.GetSpecialAdvices(_client.Board, _client.CurrentPiece, _client.NextPiece, _client.Inventory, _client.InventorySize, _client.Opponents.ToList(), out advices);
                        foreach (SpecialAdvice advice in advices)
                        {
                            bool continueLoop = true;
                            switch (advice.SpecialAdviceAction)
                            {
                                case SpecialAdvice.SpecialAdviceActions.Wait:
                                    continueLoop = false;
                                    break;
                                case SpecialAdvice.SpecialAdviceActions.Discard:
                                    _client.DiscardFirstSpecial();
                                    continueLoop = true;
                                    break;
                                case SpecialAdvice.SpecialAdviceActions.UseSelf:
                                    continueLoop = _client.UseSpecial(_client.PlayerId);
                                    break;
                                case SpecialAdvice.SpecialAdviceActions.UseOpponent:
                                    continueLoop = _client.UseSpecial(advice.OpponentId);
                                    break;
                            }
                            if (!continueLoop)
                                break;
                            Thread.Sleep(10); // delay next special use
                        }
                    }

                    DateTime specialManaged = DateTime.Now;

                    // No move evaluation if confusion is active
                    if (_isConfusionActive)
                    {
                        Log.WriteLine(Log.LogLevels.Info, "Confusion is active, no move evaluated");
                        continue;
                    }

                    // Get best move
                    if (MoveStrategy != null)
                    {
                        int bestRotationDelta;
                        int bestTranslationDelta;
                        bool rotationBeforeTranslation;
                        MoveStrategy.GetBestMove(_client.Board, _client.CurrentPiece, _client.NextPiece, out bestRotationDelta, out bestTranslationDelta, out rotationBeforeTranslation);

                        // TODO: could use an event linked to Client.OnRoundFinished
                        if (_client.CurrentPiece.Index != currentPieceIndex)
                        {
                            Log.WriteLine(Log.LogLevels.Warning, "BOT IS TOO SLOW COMPARED TO AUTOMATIC DROP, skipping to next piece {0} != {1}", _client.CurrentPiece.Index, currentPieceIndex);
                            continue;
                        }

                        DateTime searchBestModeEndTime = DateTime.Now;

                        //Log.WriteLine(Log.LogLevels.Debug, "Rotation: {0} Translation {1}  {2}", bestRotationDelta, bestTranslationDelta, rotationBeforeTranslation);

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
                        if (sleepTime < 10)
                            sleepTime = 10; // at least 10 ms
                        //Thread.Sleep((int)sleepTime); // delay drop instead of animating
                        bool stopped = _stopEvent.WaitOne((int) sleepTime);
                        if (stopped)
                        {
                            //Log.WriteLine(Log.LogLevels.Debug, "Stop bot received while sleeping before next move");
                            break;
                        }
                        // TODO: could use an event linked to Client.OnRoundFinished
                        if (_client.CurrentPiece.Index != currentPieceIndex)
                        {
                            Log.WriteLine(Log.LogLevels.Warning, "BOT IS TOO SLOW COMPARED TO AUTOMATIC DROP, skipping to next piece {0} != {1}", _client.CurrentPiece.Index, currentPieceIndex);
                            continue;
                        }
                        // Drop
                        Drop();
                    }
                    //
                    //Log.WriteLine(Log.LogLevels.Debug, "BEST MOVE found in {0} ms and special in {1} ms  {2} {3}", (searchBestModeEndTime - specialManaged).TotalMilliseconds, (specialManaged - searchBestMoveStartTime).TotalMilliseconds, _client.CurrentPiece.Value, _client.CurrentPiece.Index);
                }
            }
        }

        private void Rotate(int rotationDelta)
        {
            for (int rotateCount = 0; rotateCount < rotationDelta; rotateCount++)
            {
                _client.RotateClockwise();
                //System.Threading.Thread.Sleep(250);
            }
        }

        private void Translate(int translationDelta)
        {
            if (translationDelta < 0)
                for (int translateCount = 0; translateCount > translationDelta; translateCount--)
                {
                    _client.MoveLeft();
                    //System.Threading.Thread.Sleep(250);
                }
            if (translationDelta > 0)
                for (int translateCount = 0; translateCount < translationDelta; translateCount++)
                {
                    _client.MoveRight();
                    //System.Threading.Thread.Sleep(250);
                }
        }

        private void Drop()
        {
            _client.Drop();
        }
    }
}
