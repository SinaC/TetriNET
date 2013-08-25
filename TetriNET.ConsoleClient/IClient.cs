namespace TetriNET.Client
{
    public delegate void PauseGameHandler();
    public delegate void ResumeGameHandler();
    public delegate void GameOverHandler();
    public delegate void RedrawHandler();

    public interface IClient
    {
        bool IsGamePaused { get; }
        bool IsGameStarted { get; }

        ITetrimino CurrentTetrimino { get; }
        ITetrimino NextTetrimino { get; }

        int Width { get; }
        int Height { get; }
        byte[] Grid { get; }

        event PauseGameHandler OnGamePaused;
        event ResumeGameHandler OnGameResumed;
        event GameOverHandler OnGameOver;
        event RedrawHandler OnRedraw;

        // Game controller
        void Drop();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();
    }
}
