using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public interface IAchievementManager
    {
        List<IAchievement> Achievements { get; }

        event AchievedHandler Achieved;

        void Reset();
        
        void OnGameStarted(GameOptions options);
        void OnGameFinished();
        void OnRoundFinished(int deletedRows, int level, int moveCount, int score, IBoard board, List<Pieces> collapsedPieces);
        void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnGameOver(int moveCount, int linesCleared, int playingOpponentsInCurrentGame, int playingOpponentsLeftInCurrentGame, IEnumerable<Specials> inventory);
        void OnGameWon(int moveCount, int linesCleared, int playingOpponentsInCurrentGame);
    }
}
