using System.Collections.Generic;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;

namespace TetriNET.Tests.Server.Mocking
{
    public class CountCallTetriNETCallback : ITetriNETCallback
    {
        private readonly Dictionary<string, int> _callCount = new Dictionary<string, int>();

        private void UpdateCallCount(string callbackName)
        {
            if (!_callCount.ContainsKey(callbackName))
                _callCount.Add(callbackName, 1);
            else
                _callCount[callbackName]++;
        }

        public int GetCallCount(string callbackName)
        {
            int value;
            _callCount.TryGetValue(callbackName, out value);
            return value;
        }

        public void OnHeartbeatReceived()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerStopped()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerRegistered(RegistrationResults result, Versioning clientVersion, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerJoined(int playerId, string name, string team)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerTeamChanged(int playerId, string team)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPublishServerMessage(string msg)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerLost(int playerId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerWon(int playerId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameStarted(List<Pieces> pieces)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameFinished(GameStatistics statistics)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGamePaused()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGameResumed()
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerAddLines(int lineCount)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnNextPiece(int firstIndex, List<Pieces> piece)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnGridModified(int playerId, byte[] grid)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnServerMasterChanged(int playerId)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnWinListModified(List<WinEntry> winList)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnOptionsChanged(GameOptions options)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnSpectatorRegistered(RegistrationResults result, Versioning clientVersion, int spectatorId, bool gameStarted, GameOptions options)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnSpectatorJoined(int spectatorId, string name)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }

        public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
        {
            UpdateCallCount(System.Reflection.MethodBase.GetCurrentMethod().Name);
        }
    }
}
