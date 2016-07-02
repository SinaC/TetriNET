﻿using System;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class CallMeSavior : AchievementBase
    {
        public CallMeSavior()
        {
            Id = 4;
            Points = 10;
            Title = "Call me Savior !";
            Description = "Nuke a player in your team";
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
            if (playerId != targetId && !String.IsNullOrWhiteSpace(playerTeam) && !String.IsNullOrWhiteSpace(targetTeam) && playerTeam == targetTeam && special == Specials.NukeField)
                Achieve();
        }
    }
}
