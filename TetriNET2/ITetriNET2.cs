using System;

namespace TetriNET2
{
    public interface ITetriNET2
    {
        // Connect/disconnect/keep alive
        void Connect(string name);
        void Disconnect();
        void Heartbeat();

        // Wait+Game room
        void SendDirect(Guid clientId, string message);
        void SendBroadcast(string message);
        void ChangeTeam(string team);

        // Wait room
        void JoinGame(Guid gameId, string password, bool asSpectator);
        void CreateAndJoinGame(string name, string password, bool asSpectator);

        // Game room as server master (player or spectator)
        void StartGame();
        void StopGame();
        void PauseGame();
        void ResumeGame();
        void ChangeOptions(GameOptions options);
        void VoteKick(Guid clientId);
        void VoteKickResponse(bool accepted);
        void ResetWinList();

        // Game room as player or spectator
        void LeaveGame();

        // Game room as player
        void PlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);
        void ModifyGrid(byte[] grid);
        void UseSpecial(Guid targetId, Specials special);
        void ClearLines(int count);
        void GameLost();
        void FinishContinuousSpecial(Specials special);
        void EarnAchievement(int achievementId, string achievementTitle);
    }
}
