using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class Slam : Achievement
    {
        private Dictionary<Specials, bool> _specialsUsed;

        public Slam()
        {
            Id = 33;
            Points = 10;
            Title = "Slam";
            Description = "Use every available specials in one game";
        }

        public override void OnGameStarted(GameOptions options)
        {
            _specialsUsed = options.SpecialOccurancies.Where(x => x.Occurancy > 0).ToDictionary(x => x.Value, x => false);
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (_specialsUsed.ContainsKey(special))
            {
                _specialsUsed[special] = true;
                if (_specialsUsed.All(pair => pair.Value))
                    Achieve();
            }
        }
    }
}
