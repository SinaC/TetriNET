using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetriNET.Client.Achievements.Achievements
{
    public class WhoIsYourDaddy : Achievement
    {
        private int _count;

        public WhoIsYourDaddy()
        {
            Title = "Who's your daddy ?";
            Description = "Win 5 time in a row";
            ResetOnGameStarted = false;
        }

        public override void Reset()
        {
            _count = 0;
            base.Reset();
        }

        public override void OnGameWon(double playTime, int moveCount, int lineCount, int playerCount)
        {
              _count++;
            if (_count == 5)
                Achieve();
        }

        public override void OnGameLost(double playTime, int moveCount, int lineCount, int playerCount)
        {
            _count = 0;
        }
    }
}
