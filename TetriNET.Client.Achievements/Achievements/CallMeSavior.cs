using System;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class CallMeSavior : Achievement
    {
        public CallMeSavior()
        {
            Id = 4;
            Points = 10;
            Title = "Call me Savior !";
            Description = "Nuke a player in your team";
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (playerId != targetId && !String.IsNullOrWhiteSpace(playerTeam) && !String.IsNullOrWhiteSpace(targetTeam) && playerTeam == targetTeam && special == Specials.NukeField)
                Achieve();
        }
    }
}
