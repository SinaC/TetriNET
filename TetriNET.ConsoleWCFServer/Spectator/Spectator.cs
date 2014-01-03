using System;
using System.Collections.Generic;
using System.ServiceModel;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.ConsoleWCFServer.Spectator
{
    public sealed class Spectator : ISpectator
    {
        public Spectator(string name, ITetriNETCallback callback)
        {
            Name = name;
            Callback = callback;
            LastActionToClient = DateTime.Now;
            LastActionFromClient = DateTime.Now;
            TimeoutCount = 0;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                LastActionToClient = DateTime.Now;
            }
            catch (CommunicationObjectAbortedException)
            {
                OnConnectionLost.Do(x => x(this));
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                OnConnectionLost.Do(x => x(this));
            }
        }

        #region ISpectator + IEntity

        public event SpectatorConnectionLostHandler OnConnectionLost;

        public string Name { get; private set; }

        //
        public ITetriNETCallback Callback { get; private set; }
        //

        // Heartbeat management
        public DateTime LastActionToClient { get; private set; } // used to check if heartbeat is needed
        // Timeout management
        public DateTime LastActionFromClient { get; private set; }
        public int TimeoutCount { get; private set; }

        public void ResetTimeout()
        {
            TimeoutCount = 0;
            LastActionFromClient = DateTime.Now;
        }

        public void SetTimeout()
        {
            TimeoutCount++;
            LastActionFromClient = DateTime.Now;
        }

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            ExceptionFreeAction(() => Callback.OnHeartbeatReceived(), "OnHeartbeatReceived");
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped(), "OnServerStopped");
        }

        public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnPlayerRegistered(result, playerId, gameStarted, isServerMaster, options), "OnPlayerRegistered");
        }

        public void OnPlayerJoined(int playerId, string name, string team)
        {
            ExceptionFreeAction(() => Callback.OnPlayerJoined(playerId, name, team), "OnPlayerJoined");
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLeft(playerId, name, reason), "OnPlayerLeft");
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            ExceptionFreeAction(() => Callback.OnPlayerTeamChanged(playerId, team), "OnPlayerTeamChanged");
        }

        public void OnPlayerLost(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLost(playerId), "OnPlayerLost");
        }

        public void OnPlayerWon(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerWon(playerId), "OnPlayerWon");
        }

        public void OnGameStarted(List<Pieces> pieces, GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(pieces, options), "OnGameStarted");
        }

        public void OnGameFinished()
        {
            ExceptionFreeAction(() => Callback.OnGameFinished(), "OnGameFinished");
        }

        public void OnGamePaused()
        {
            ExceptionFreeAction(() => Callback.OnGamePaused(), "OnGamePaused");
        }

        public void OnGameResumed()
        {
            ExceptionFreeAction(() => Callback.OnGameResumed(), "OnGameResumed");
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnServerAddLines(lineCount), "OnServerAddLines");
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnPlayerAddLines(specialId, playerId, lineCount), "OnPlayerAddLines");
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg), "OnPublishPlayerMessage");
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg), "OnPublishServerMessage");
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnSpecialUsed(specialId, playerId, targetId, special), "OnSpecialUsed");
        }

        public void OnNextPiece(int index, List<Pieces> pieces)
        {
            ExceptionFreeAction(() => Callback.OnNextPiece(index, pieces), "OnNextPiece");
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            ExceptionFreeAction(() => Callback.OnGridModified(playerId, grid), "OnGridModified");
        }

        public void OnServerMasterChanged(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnServerMasterChanged(playerId), "OnServerMasterChanged");
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            ExceptionFreeAction(() => Callback.OnWinListModified(winList), "OnWinListModified");
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnContinuousSpecialFinished(playerId, special), "OnContinuousSpecialFinished");
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            ExceptionFreeAction(() => Callback.OnAchievementEarned(playerId, achievementId, achievementTitle), "OnAchievementEarned");
        }

        public void OnSpectatorRegistered(RegistrationResults result, int spectatorId, bool gameStarted)
        {
            ExceptionFreeAction(() => Callback.OnSpectatorRegistered(result, spectatorId, gameStarted), "OnSpectatorRegistered");
        }

        public void OnSpectatorJoined(int spectatorId, string name)
        {
            ExceptionFreeAction(() => Callback.OnSpectatorJoined(spectatorId, name), "OnSpectatorJoined");
        }

        public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnSpectatorLeft(spectatorId, name, reason), "OnSpectatorLeft");
        }

        #endregion
    }
}
