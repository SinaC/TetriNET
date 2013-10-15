using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client.Achievements.Achievements
{
    public class TooEasyForMe : Achievement
    {
        private bool _nukeUsed;

        public TooEasyForMe()
        {
            Title = "Too easy for me";
            Description = "'Nuke Field' an ennemy (and win the game)";
        }

        public override void Reset()
        {
            _nukeUsed = false;
            base.Reset();
        }

        public override void OnSpecialUsed(int playerId, int sourceId, string sourceTeam, IBoard sourceBoard, int targetId, string targetTeam, IBoard targetBoard, Specials special)
        {
            if (sourceId == playerId && targetId != playerId && special == Specials.NukeField)
                _nukeUsed = true;
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
            if (_nukeUsed)
                Achieve();
        }
    }
}
