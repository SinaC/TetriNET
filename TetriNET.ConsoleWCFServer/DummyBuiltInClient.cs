using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;

namespace TetriNET.ConsoleWCFServer
{
    public sealed class DummyBuiltInClient : ITetriNETCallback
    {
        private const int HeartbeatDelay = 300; // in ms
        private const int TimeoutDelay = 500; // in ms
        private const int MaxTimeoutCountBeforeDisconnection = 3;
        private const int Width = 12;
        private const int Height = 20;

        public enum States
        {
            ApplicationStarted, // -> ConnectingToServer
            ConnectingToServer, // -> ConnectedToServer
            ConnectedToServer, // -> Registering
            Registering, // -> WaitingStartGame | ApplicationStarted
            WaitingStartGame, // -> GameStarted
            GameStarted, // -> GameFinished
            GameFinished, // -> WaitingStartGame
        }

        private readonly Func<ITetriNET> _getProxyFunc;
        private ITetriNET Proxy { get; set; }
        private byte[] PlayerGrid { get; set; }
        private int PieceIndex { get; set; }
        private DateTime _lastServerAction;
        private int _timeoutCount;
        private DateTime _lastHeartbeat;

        public string PlayerName { get; private set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }
        public bool IsServerMaster { get; private set; }

        public DummyBuiltInClient(string playerName, Func<ITetriNET> getProxyFunc)
        {
            PlayerName = playerName;
            _getProxyFunc = getProxyFunc;

            PlayerGrid = new byte[Width*Height];
            for (int i = 0; i < Height; i++)
                PlayerGrid[i * Width] = 1;

            IsServerMaster = false;
            PieceIndex = 0;

            _lastHeartbeat = DateTime.Now.AddMilliseconds(-HeartbeatDelay);
            _lastServerAction = DateTime.Now;
            _timeoutCount = 0;

            State = States.ApplicationStarted;
        }

        public void ConnectToServer()
        {
            Log.WriteLine(Log.LogLevels.Info, "Connecting to server");
            State = States.ConnectingToServer;

            Proxy = _getProxyFunc();

            if (Proxy != null)
                State = States.ConnectedToServer;
            else
                State = States.ApplicationStarted;
        }

        public void DisconnectFromServer()
        {
            Log.WriteLine(Log.LogLevels.Info, "Disconnecting from server");

            Proxy.UnregisterPlayer(this);
            //
            State = States.ApplicationStarted;
        }

        public void Test()
        {
            switch (State)
            {
                case States.ApplicationStarted:
                    ConnectToServer();
                    break;
                case States.ConnectingToServer:
                    // NOP: wait connection resolution
                    break;
                case States.ConnectedToServer:
                    Register(PlayerName);
                    break;
                case States.Registering:
                    // NOP: waiting callback OnPlayerRegistered
                    break;
                case States.WaitingStartGame:
                    // NOP: waiting callback OnGameStarted
                    break;
                case States.GameStarted:
                    int rnd = new Random().Next(4);
                    switch (rnd)
                    {
                        case 0:
                            Proxy.PublishMessage(this, "I'll kill you");
                            break;
                        case 1:
                            Proxy.PlacePiece(this, PieceIndex, PieceIndex, Pieces.TetriminoI, 1, 5, 3, PlayerGrid);
                            PieceIndex++;
                            break;
                        case 2:
                            Proxy.UseSpecial(this, PlayerId, Specials.NukeField);
                            break;
                        case 3:
                            Proxy.ModifyGrid(this, PlayerGrid);
                            break;
                    }
                    Thread.Sleep(60);
                    break;
                case States.GameFinished:
                    State = States.WaitingStartGame;
                    break;
            }
            if (State == States.WaitingStartGame || State == States.GameStarted || State == States.GameFinished)
            {
                // Check server timeout
                TimeSpan timespan = DateTime.Now - _lastServerAction;
                if (timespan.TotalMilliseconds > TimeoutDelay)
                {
                    Log.WriteLine(Log.LogLevels.Info, "Timeout++");
                    SetTimeout();
                    if (_timeoutCount >= MaxTimeoutCountBeforeDisconnection)
                        OnServerStopped(); // timeout
                }

                // Send heartbeat if needed // TODO: reset this when sending a message
                TimeSpan delayFromPreviousHeartbeat = DateTime.Now - _lastHeartbeat;
                if (delayFromPreviousHeartbeat.TotalMilliseconds > HeartbeatDelay)
                {
                    Proxy.Heartbeat(this);
                    _lastHeartbeat = DateTime.Now;
                }
            }
        }

        public void Lose()
        {
            Log.WriteLine(Log.LogLevels.Info, "Loseeeeeeeer");
            State = States.WaitingStartGame;

            Proxy.GameLost(this);
        }

        private void ResetTimeout()
        {
            _timeoutCount = 0;
            _lastServerAction = DateTime.Now;
        }

        private void SetTimeout()
        {
            _timeoutCount++;
            _lastServerAction = DateTime.Now;
        }
        
        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            ResetTimeout();
        }

        public void OnServerStopped()
        {
            Log.WriteLine(Log.LogLevels.Info, "OnServerStopped[{0}]", PlayerName);
            State = States.ApplicationStarted;
            ResetTimeout();
        }

        public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlayerRegistered[{0}]:{1} => {2} {3} {4}", PlayerName, result, playerId, gameStarted, isServerMaster);
            ResetTimeout();
            if (result == RegistrationResults.RegistrationSuccessful)
            {
                PlayerId = playerId;
                State = States.WaitingStartGame;
            }
            else
                State = States.ApplicationStarted;
        }

        public void OnPlayerJoined(int playerId, string name)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlayerJoined[{0}]:{1}[{2}]", PlayerName, name, playerId);
            ResetTimeout();
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlayedLeft[{0}]:{1}[{2}] {3}", PlayerName, name, playerId, reason);
            ResetTimeout();
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            Log.WriteLine(Log.LogLevels.Info, "PlayerTeamChanged[{0}]:[1] {2}", PlayerName, playerId, team);
            ResetTimeout();
        }

        public void OnPlayerLost(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlayerLost[{0}]:[{1}]", PlayerName, playerId);
            ResetTimeout();
        }

        public void OnPlayerWon(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlayerWon[{0}]:[{1}]", PlayerName, playerId);
            ResetTimeout();
        }

        public void OnGameStarted(List<Pieces> pieces, GameOptions options)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnGameStarted[{0}]:{1}", PlayerName, pieces.Select(x => x.ToString()).Aggregate((n,i) => n + "," + i));
            ResetTimeout();
            if (State == States.WaitingStartGame)
            {
                State = States.GameStarted;
                PieceIndex = 0;
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Was not waiting start game");
        }

        public void OnGameFinished()
        {
            Log.WriteLine(Log.LogLevels.Info, "OnGameFinished[{0}]", PlayerName);
            ResetTimeout();
            if (State == States.GameStarted)
            {
                Log.WriteLine(Log.LogLevels.Info, "Game finished: #piece: {0}", PieceIndex);
                State = States.GameFinished;
            }
            else
                Log.WriteLine(Log.LogLevels.Info, "Game was not started");
        }

        public void OnGamePaused()
        {
            Log.WriteLine(Log.LogLevels.Info, "OnGamePaused[{0}]", PlayerName);
            ResetTimeout();
        }

        public void OnGameResumed()
        {
            Log.WriteLine(Log.LogLevels.Info, "OnGameResumed[{0}]", PlayerName);
            ResetTimeout();
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnServerAddLines[{0}]:{1}", PlayerName, lineCount);
            ResetTimeout();
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPlayerAddLines[{0}]:{1} [{2}] {3}", PlayerName, specialId, playerId, lineCount);
            ResetTimeout();
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPublishPlayerMessage[{0}]:{1}:{2}", PlayerName, playerName, msg);
            ResetTimeout();
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnPublishServerMessage[{0}]:{1}", PlayerName, msg);
            ResetTimeout();
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnSpecialUsed[{0}]:{1} [{2}] [{3}] {4}", PlayerName, specialId, playerId, targetId, special);
            ResetTimeout();
        }

        public void OnNextPiece(int index, List<Pieces> pieces)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnNextPiece[{0}]:{1} {2}", PlayerName, index, pieces.Select(x => x.ToString()).Aggregate((n, i) => n + "," + i));
            ResetTimeout();
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnGridModified[{0}]:[{1}] [2]", PlayerName, playerId, grid.Count(x => x > 0));
            ResetTimeout();
        }

        public void OnServerMasterChanged(int playerId)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnServerMasterChanged[{0}]:[{1}]", PlayerName, playerId);
            ResetTimeout();
            IsServerMaster = (playerId == PlayerId);
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnWinListModified[{0}]:{1}", PlayerName, winList.Any() ? winList.Select(x => String.Format("{0}:{1}", x.PlayerName, x.Score)).Aggregate((n, i) => n + "|" + i) : "");
            ResetTimeout();
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnContinuousSpecialFinished[{0}]:{1} {2}", PlayerName, playerId, special);
            ResetTimeout();    
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            Log.WriteLine(Log.LogLevels.Info, "OnAchievementEarned[{0}]:{1} {2} {3}", PlayerName, playerId, achievementId, achievementTitle);
        }

        #endregion

        private void Register(string playerName)
        {
            Log.WriteLine(Log.LogLevels.Info, "Registering");
            State = States.Registering;

            PlayerName = playerName;

            Proxy.RegisterPlayer(this, PlayerName);
        }
    }
}
