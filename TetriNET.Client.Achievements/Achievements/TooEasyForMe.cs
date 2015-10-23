﻿using TetriNET.Client.Interfaces;
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
            BronzeLevel = 1;
            SilverLevel = 5;
            GoldLevel = 10;
        }

        public override void Reset()
        {
            _nukeUsed = false;
            base.Reset();
        }

        public override void OnUseSpecial(int playerId, string playerTeam, IReadOnlyBoard playerBoard, int targetId, string targetTeam, IReadOnlyBoard targetBoard, Specials special)
        {
            // TODO: check team ???
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
