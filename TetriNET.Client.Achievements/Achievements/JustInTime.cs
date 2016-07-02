﻿using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class JustInTime : AchievementBase
    {
        public JustInTime()
        {
            Id = 15;
            Points = 30;
            Title = "Just in time...";
            Description = "Nuke 1 line before death";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
            if (playerId == targetId && special == Specials.NukeField)
            {
                int spawnX = playerBoard.PieceSpawnX;
                int topY = playerBoard.Height - 2;
                if (playerBoard[spawnX, topY] != CellHelper.EmptyCell)
                    Achieve();
            }
        }
    }
}
