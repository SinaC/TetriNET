namespace TetriNET.Client
{
    public delegate void PauseGameHandler();
    public delegate void ResumeGameHandler();

    public interface IClient
    {
        event PauseGameHandler OnGamePaused;
        event ResumeGameHandler OnGameResumed;

        // Game controller
        void Drop();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();
    }
}
