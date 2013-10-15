using System;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    public class CallMeSavior : Achievement
    {
        public CallMeSavior()
        {
            Title = "Call me Savior !";
            Description = "'Nuke Field' a friend (only in team)";
        }

        public override void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (sourceId == playerId && playerId != targetId && !String.IsNullOrWhiteSpace(sourceTeam) && !String.IsNullOrWhiteSpace(targetTeam) && sourceTeam == targetTeam)
                Achieve();
        }
    }
}
