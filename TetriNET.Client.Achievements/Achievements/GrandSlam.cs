using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class GrandSlam : Achievement
    {
        private class Used
        {
            public Used()
            {
                UsedOnOpponent = false;
                TargettedBy = false;
            }

            public bool UsedOnOpponent { get; set; }
            public bool TargettedBy { get; set; }
        }

        private Dictionary<Specials, Used> _specialsUsed;

        public GrandSlam()
        {
            Id = 34;
            Points = 70;
            Title = "Grand Slam";
            Description = "Use on an opponent and be the target of every available specials";
        }

        public override void OnGameStarted(GameOptions options)
        {
            _specialsUsed = options.SpecialOccurancies.Where(x => x.Occurancy > 0).ToDictionary(x => x.Value, x => new Used());
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (playerId != targetId && _specialsUsed.ContainsKey(special))
            {
                _specialsUsed[special].UsedOnOpponent = true;
                CheckAchieved();
            }
        }

        public override void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (targetId == playerId && sourceId != playerId && _specialsUsed.ContainsKey(special))
            {
                _specialsUsed[special].TargettedBy = true;
                CheckAchieved();   
            }
        }

        private void CheckAchieved()
        {
            if (_specialsUsed.All(pair => pair.Value.UsedOnOpponent && pair.Value.TargettedBy))
                Achieve();
        }
    }
}
