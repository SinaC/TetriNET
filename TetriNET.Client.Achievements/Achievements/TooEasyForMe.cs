using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    internal class TooEasyForMe : Achievement
    {
        private bool _nukeUsed;

        public TooEasyForMe()
        {
            Id = 27;
            Points = 20;
            Title = "Too easy for me";
            Description = "Nuke an enemy (and win the game)";
        }

        public override void Reset()
        {
            _nukeUsed = false;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IBoard playerBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (targetId != playerId && special == Specials.NukeField)
                _nukeUsed = true;
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (_nukeUsed)
                Achieve();
        }
    }
}
