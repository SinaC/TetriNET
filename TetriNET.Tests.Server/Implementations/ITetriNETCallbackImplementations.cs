using System;
using System.Collections.Generic;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;

namespace TetriNET.Tests.Server.Implementations
{
    public class NullLoopTetriNETCallback : ITetriNETCallback
    {
        public void OnHeartbeatReceived()
        {
            // NOP
        }

        public void OnServerStopped()
        {
            // NOP
        }

        public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
        {
            // NOP
        }

        public void OnPlayerJoined(int playerId, string name, string team)
        {
            // NOP
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            // NOP
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            // NOP
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            // NOP
        }

        public void OnPublishServerMessage(string msg)
        {
            // NOP
        }

        public void OnPlayerLost(int playerId)
        {
            // NOP
        }

        public void OnPlayerWon(int playerId)
        {
            // NOP
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            // NOP
        }

        public void OnGameFinished(GameStatistics statistics)
        {
            // NOP
        }

        public void OnGamePaused()
        {
            // NOP
        }

        public void OnGameResumed()
        {
            // NOP
        }

        public void OnServerAddLines(int lineCount)
        {
            // NOP
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            // NOP
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            // NOP
        }

        public void OnNextPiece(int firstIndex, List<Pieces> piece)
        {
            // NOP
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            // NOP
        }

        public void OnServerMasterChanged(int playerId)
        {
            // NOP
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            // NOP
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            // NOP
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            // NOP
        }

        public void OnOptionsChanged(GameOptions options)
        {
            // NOP
        }

        public void OnSpectatorRegistered(RegistrationResults result, int spectatorId, bool gameStarted, GameOptions options)
        {
            // NOP
        }

        public void OnSpectatorJoined(int spectatorId, string name)
        {
            // NOP
        }

        public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
        {
            // NOP
        }
    }

    public class RaiseExceptionTetriNETCallback : ITetriNETCallback
    {
        public void OnHeartbeatReceived()
        {
            throw new NotImplementedException();
        }

        public void OnServerStopped()
        {
            throw new NotImplementedException();
        }

        public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerJoined(int playerId, string name, string team)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            throw new NotImplementedException();
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            throw new NotImplementedException();
        }

        public void OnPublishServerMessage(string msg)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerLost(int playerId)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerWon(int playerId)
        {
            throw new NotImplementedException();
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            throw new NotImplementedException();
        }

        public void OnGameFinished(GameStatistics statistics)
        {
            throw new NotImplementedException();
        }

        public void OnGamePaused()
        {
            throw new NotImplementedException();
        }

        public void OnGameResumed()
        {
            throw new NotImplementedException();
        }

        public void OnServerAddLines(int lineCount)
        {
            throw new NotImplementedException();
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            throw new NotImplementedException();
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            throw new NotImplementedException();
        }

        public void OnNextPiece(int firstIndex, List<Pieces> piece)
        {
            throw new NotImplementedException();
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            throw new NotImplementedException();
        }

        public void OnServerMasterChanged(int playerId)
        {
            throw new NotImplementedException();
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            throw new NotImplementedException();
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            throw new NotImplementedException();
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            throw new NotImplementedException();
        }

        public void OnOptionsChanged(GameOptions options)
        {
            throw new NotImplementedException();
        }

        public void OnSpectatorRegistered(RegistrationResults result, int spectatorId, bool gameStarted, GameOptions options)
        {
            throw new NotImplementedException();
        }

        public void OnSpectatorJoined(int spectatorId, string name)
        {
            throw new NotImplementedException();
        }

        public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
        {
            throw new NotImplementedException();
        }
    }
}
