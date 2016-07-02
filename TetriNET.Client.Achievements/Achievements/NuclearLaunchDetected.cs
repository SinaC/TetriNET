﻿using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class NuclearLaunchDetected : AchievementBase
    {
        public NuclearLaunchDetected()
        {
            Id = 21;
            Points = 30;
            Title = "Nuclear launch detected";
            Description = "Explode 3 (or more) Bombs in one attack";
            BronzeLevel = 1; 
            SilverLevel = 5; 
            GoldLevel = 10;
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
            if (special == Specials.BlockBomb)
            {
                int targetBomb = targetBoard.ReadOnlyCells.Count(x => CellHelper.GetSpecial(x) == Specials.BlockBomb);
                if (targetBomb >= 3)
                    Achieve();
            }
        }
    }
}
