using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Interfaces
{
    public interface IAchievementManager
    {
        List<IAchievement> Achievements { get; }

        event AchievedHandler Achieved;

        void Reset();
        
        void OnGameStarted();
        void OnGameFinished();
        void OnRoundFinished(int deletedRows, int level, IBoard board);
        void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special);
        void OnGameOver(int moveCount, int linesCleared, int playingOpponentsInCurrentGame, int playingOpponentsLeftInCurrentGame);
        void OnGameWon(int moveCount, int linesCleared, int playingOpponentsInCurrentGame);
    }
}
