namespace TetriNET.Client.Achievements.Achievements
{
    public class Architect : Achievement
    {
        private bool _active;

        public Architect()
        {
            Title = "Architect";
            Description = "2 Tetris in a row";
        }

        public override void Reset()
        {
            _active = false;
            base.Reset();
        }

        public override void OnRoundFinished(int lineCompleted)
        {
            //if line completed == 4 and active == true -> VALIDATED; if line completed == 4 and active == false, active = true; if line completed < 4 -> active = false
            if (lineCompleted == 4 && _active)
                Achieve();
            else if (lineCompleted == 4)
                _active = true;
            else
                _active = false;
       }
    }
}
