using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.SpectatorManager
{
    public sealed class Spectator : ISpectator
    {
        public Spectator(int id, string name, ITetriNETCallback callback)
        {
            Id = id;
            Name = name;
            Callback = callback;
            LastActionToClient = DateTime.Now;
            LastActionFromClient = DateTime.Now;
            TimeoutCount = 0;
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                action();
                LastActionToClient = DateTime.Now;
            }
            catch (CommunicationObjectAbortedException)
            {
                ConnectionLost.Do(x => x(this));
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x(this));
            }
        }

        #region IEntity

        public event ConnectionLostEventHandler ConnectionLost;

        public int Id { get; private set; }
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

        #region ISpectator

        // NOP

        #endregion

        #region ITetriNETCallback

        public void OnHeartbeatReceived()
        {
            ExceptionFreeAction(() => Callback.OnHeartbeatReceived());
        }

        public void OnServerStopped()
        {
            ExceptionFreeAction(() => Callback.OnServerStopped());
        }

        public void OnPlayerRegistered(RegistrationResults result, Versioning serverVersion, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnPlayerRegistered(result, serverVersion, playerId, gameStarted, isServerMaster, options));
        }

        public void OnPlayerJoined(int playerId, string name, string team)
        {
            ExceptionFreeAction(() => Callback.OnPlayerJoined(playerId, name, team));
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLeft(playerId, name, reason));
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            ExceptionFreeAction(() => Callback.OnPlayerTeamChanged(playerId, team));
        }

        public void OnPlayerLost(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerLost(playerId));
        }

        public void OnPlayerWon(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnPlayerWon(playerId));
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            ExceptionFreeAction(() => Callback.OnGameStarted(pieces));
        }

        public void OnGameFinished(GameStatistics statistics)
        {
            ExceptionFreeAction(() => Callback.OnGameFinished(statistics));
        }

        public void OnGamePaused()
        {
            ExceptionFreeAction(() => Callback.OnGamePaused());
        }

        public void OnGameResumed()
        {
            ExceptionFreeAction(() => Callback.OnGameResumed());
        }

        public void OnServerAddLines(int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnServerAddLines(lineCount));
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            ExceptionFreeAction(() => Callback.OnPlayerAddLines(specialId, playerId, lineCount));
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishPlayerMessage(playerName, msg));
        }

        public void OnPublishServerMessage(string msg)
        {
            ExceptionFreeAction(() => Callback.OnPublishServerMessage(msg));
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnSpecialUsed(specialId, playerId, targetId, special));
        }

        public void OnNextPiece(int index, List<Pieces> pieces)
        {
            ExceptionFreeAction(() => Callback.OnNextPiece(index, pieces));
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            ExceptionFreeAction(() => Callback.OnGridModified(playerId, grid));
        }

        public void OnServerMasterChanged(int playerId)
        {
            ExceptionFreeAction(() => Callback.OnServerMasterChanged(playerId));
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            ExceptionFreeAction(() => Callback.OnWinListModified(winList));
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            ExceptionFreeAction(() => Callback.OnContinuousSpecialFinished(playerId, special));
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            ExceptionFreeAction(() => Callback.OnAchievementEarned(playerId, achievementId, achievementTitle));
        }

        public void OnOptionsChanged(GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnOptionsChanged(options));
        }

        public void OnSpectatorRegistered(RegistrationResults result, Versioning serverVersion, int spectatorId, bool gameStarted, GameOptions options)
        {
            ExceptionFreeAction(() => Callback.OnSpectatorRegistered(result, serverVersion, spectatorId, gameStarted, options));
        }

        public void OnSpectatorJoined(int spectatorId, string name)
        {
            ExceptionFreeAction(() => Callback.OnSpectatorJoined(spectatorId, name));
        }

        public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
        {
            ExceptionFreeAction(() => Callback.OnSpectatorLeft(spectatorId, name, reason));
        }

        #endregion
    }
}
