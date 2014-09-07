using System;
using System.Collections.Generic;

namespace TetriNET2
{
    public interface ITetriNET2Callback
    {
        void OnConnected(ConnectResults result, Versioning serverVersion, Guid clientId, List<GameDescription> games);
        void OnDisconnected();
        void OnHeartbeatReceived();

        void OnServerStopped();

        void OnClientJoined(Guid clientId, string name, string team);
        void OnClientLeft(Guid clientId, LeaveReasons reason);

        void OnServerMessageReceived(string message);
        void OnClientBroadcastMessageReceived(Guid clientId, string message);
        void OnClientDirectMessageReceived(Guid clientId, string message);
        void OnClientTeamChanged(Guid clientId, string team);

        void OnGameCreatedAndJoined(Guid gameId);
        void OnGameJoined(Guid gameId);
        void OnGameJoinFailed(Guid gameId, GameJoinFailReasons reason);
        void OnClientGameJoined(Guid clientId);
        void OnClientGameLeft(Guid clientId);
        void OnGameStarted();
        void OnGamePaused();
        void OnGameResumed();
        void OnGameFinished(GameFinishedReasons reason, GameStatistics statistics);
        void OnWinListModified(List<WinEntry> winEntries);
        void OnGameOptionsChanged(GameOptions gameOptions);
        void OnVoteKickAsked(Guid sourceClient, Guid targetClient);
        void OnAchievementEarned(Guid playerId, int achievementId, string achievementTitle);

        void OnPiecePlaced(int firstIndex, List<Pieces> nextPieces);
        void OnPlayerWon(Guid playerId);
        void OnPlayerLost(Guid playerId);
        void OnServerLinesAdded(int count);
        void OnPlayerLinesAdded(Guid playerId, int specialId, int count);
        void OnSpecialUsed(Guid playerId, Guid targetId, int specialId, Specials special);
        void OnGridModified(Guid playerId, byte[] grid);
        void OnContinuousSpecialFinished(Guid playerId, Specials special);
    }
}
