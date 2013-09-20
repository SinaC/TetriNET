using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.Interfaces;
using TetriNET.Logger;
using TetriNET.Strategy;
using TetriNET.Strategy.Move_strategies;
using TetriNET.WPF_WCF_Client.GameController;

namespace TetriNET.WPF_WCF_Client.AI
{
    public class PierreDellacherieOnePieceBot
    {
        private readonly ISpecialStrategy _specialStrategy;
        private readonly IMoveStrategy _moveStrategy;
        private readonly IClient _client;
        private readonly GameController.GameController _controller;
        private readonly ManualResetEvent _handleNextPieceEvent;
        private readonly ManualResetEvent _stopEvent;

        private bool _isConfusionActive;
        private Task _botTask;

        private bool _activated;
        public bool Activated { get { return _activated; }
            set
            {
                Log.WriteLine(Log.LogLevels.Debug, "Bot activation {0}", value);
                _activated = value;
                if (_activated)
                {
                    _handleNextPieceEvent.Set();
                    _botTask = Task.Factory.StartNew(BotTask);
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

        public PierreDellacherieOnePieceBot(IClient client, GameController.GameController controller)
        {
            _client = client;
            _controller = controller;

            _specialStrategy = new SinaCSpecials();
            //_moveStrategy = new AdvancedPierreDellacherieOnePiece();
            _moveStrategy = new LuckyToiletOnePiece();

            _client.OnRoundStarted += _client_OnRoundStarted;
            _client.OnGameStarted += client_OnGameStarted;
            _client.OnGameFinished += _client_OnGameFinished;
            _client.OnGameOver += _client_OnGameOver;
            _client.OnConfusionToggled += _client_OnConfusionToggled;

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
            _client.OnConfusionToggled -= _client_OnConfusionToggled;
        }

        private void _client_OnConfusionToggled(bool active)
        {
            _isConfusionActive = active;
        }

        private void _client_OnRoundStarted()
        {
            if (Activated)
                _handleNextPieceEvent.Set();
        }

        private void client_OnGameStarted()
        {
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
                _handleNextPieceEvent.Reset();
                _stopEvent.Reset();

                if (handle == 1) // stop event
                    break;
                if (handle == 0 /*next piece event*/ && _client.IsPlaying && _client.Board != null && _client.CurrentPiece != null && _client.NextPiece != null)
                {
                    int currentPieceIndex = _client.CurrentPiece.Index;
                    //Log.WriteLine(Log.LogLevels.Debug, "Searching best move for Piece {0} {1}", _client.CurrentPiece.Value, _client.CurrentPiece.Index);

                    DateTime searchBestMoveStartTime = DateTime.Now;

                    // Use specials
                    List<SpecialAdvices> advices;
                    _specialStrategy.GetSpecialAdvice(_client.Board, _client.CurrentPiece, _client.NextPiece, _client.Inventory, _client.InventorySize, _client.Opponents.ToList(), out advices);
                    foreach (SpecialAdvices advice in advices)
                    {
                        bool continueLoop = true;
                        switch (advice.SpecialAdviceAction)
                        {
                            case SpecialAdvices.SpecialAdviceActions.Wait:
                                continueLoop = false;
                                break;
                            case SpecialAdvices.SpecialAdviceActions.Discard:
                                _client.DiscardFirstSpecial();
                                continueLoop = true;
                                break;
                            case SpecialAdvices.SpecialAdviceActions.UseSelf:
                                continueLoop = _client.UseSpecial(_client.PlayerId);
                                break;
                            case SpecialAdvices.SpecialAdviceActions.UseOpponent:
                                continueLoop = _client.UseSpecial(advice.OpponentId);
                                break;
                        }
                        if (!continueLoop)
                            break;
                        Thread.Sleep(10); // delay next special use
                    }

                    DateTime specialManaged = DateTime.Now;

                    // No move evaluation if confusion is active
                    if (_isConfusionActive)
                    {
                        Log.WriteLine(Log.LogLevels.Info, "Confusion is active, no move evaluated");
                        continue;
                    }

                    // Get best move
                    int bestRotationDelta;
                    int bestTranslationDelta;
                    bool rotationBeforeTranslation;
                    _moveStrategy.GetBestMove(_client.Board, _client.CurrentPiece, _client.NextPiece, out bestRotationDelta, out bestTranslationDelta, out rotationBeforeTranslation);

                    // TODO: could use an event linked to Client.OnRoundFinished
                    if (_client.CurrentPiece.Index != currentPieceIndex)
                    {
                        Log.WriteLine(Log.LogLevels.Warning, "BOT IS TOO SLOW COMPARED TO AUTOMATIC DROP, skipping to next piece {0} != {1}", _client.CurrentPiece.Index, currentPieceIndex);
                        continue;
                    }

                    DateTime searchBestModeEndTime = DateTime.Now;

                    //Log.WriteLine(Log.LogLevels.Debug, "Rotation: {0} Translation {1}  {2}", bestRotationDelta, bestTranslationDelta, rotationBeforeTranslation);

                    //// Perform move
                    //if (rotationBeforeTranslation)
                    //{
                    //    // Rotate
                    //    Rotate(bestRotationDelta);
                    //    // Translate
                    //    Translate(bestTranslationDelta);
                    //}
                    //else
                    //{
                    //    // Translate
                    //    Translate(bestTranslationDelta);
                    //    // Rotate
                    //    Rotate(bestRotationDelta);
                    //}
                    ////    drop (delayed)
                    //TimeSpan timeSpan = DateTime.Now - searchBestMoveStartTime;
                    //double sleepTime = SleepTime - timeSpan.TotalMilliseconds;
                    //if (sleepTime < 10)
                    //    sleepTime = 10; // at least 10 ms
                    //System.Threading.Thread.Sleep((int) sleepTime); // delay drop instead of animating
                    //Drop();

                    // Perform move
                    //  Down 1 time
                    DownController(1);

                    if (rotationBeforeTranslation)
                    {
                        // Rotate
                        RotateController(bestRotationDelta);
                        // Translate
                        TranslateController(bestTranslationDelta);
                    }
                    else
                    {
                        // Translate
                        TranslateController(bestTranslationDelta);
                        // Rotate
                        RotateController(bestRotationDelta);
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
                    DropController();

                    //
                    //Log.WriteLine(Log.LogLevels.Debug, "BEST MOVE found in {0} ms and special in {1} ms  {2} {3}", (searchBestModeEndTime - specialManaged).TotalMilliseconds, (specialManaged - searchBestMoveStartTime).TotalMilliseconds, _client.CurrentPiece.Value, _client.CurrentPiece.Index);
                }
            }
        }

        private void Down(int times)
        {
            for (int i = 0; i < times; i++)
                _client.MoveDown();
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

        private void DownController(int times)
        {
            for (int i = 0; i < times; i++)
            {
                _controller.KeyDown(Commands.Down);
                _controller.KeyUp(Commands.Down);
            }
        }

        private void RotateController(int rotationDelta)
        {
            for (int rotateCount = 0; rotateCount < rotationDelta; rotateCount++)
            {
                _controller.KeyDown(Commands.RotateClockwise);
                _controller.KeyUp(Commands.RotateClockwise);
            }
        }

        private void TranslateController(int translationDelta)
        {
            if (translationDelta < 0)
                for (int translateCount = 0; translateCount > translationDelta; translateCount--)
                {
                    _controller.KeyDown(Commands.Left);
                    _controller.KeyUp(Commands.Left);
                }
            if (translationDelta > 0)
                for (int translateCount = 0; translateCount < translationDelta; translateCount++)
                {
                    _controller.KeyDown(Commands.Right);
                    _controller.KeyUp(Commands.Right);
                }

        }

        private void DropController()
        {
            _controller.KeyDown(Commands.Drop);
            _controller.KeyUp(Commands.Drop);
        }
    }
}
