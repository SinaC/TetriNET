namespace TetriNET.Common.Interfaces
{
    public delegate void ClientTetriminoPlacedHandler();
    public delegate void ClientStartGameHandler();
    public delegate void ClientFinishGameHandler();
    public delegate void ClientPauseGameHandler();
    public delegate void ClientResumeGameHandler();
    public delegate void ClientGameOverHandler();
    
    public delegate void ClientRedrawHandler();
    public delegate void ClientRedrawBoardHandler(int playerId);
    public delegate void ClientTetriminoMovingHandler();
    public delegate void ClientTetriminoMovedHandler();

    public interface IClient
    {
        bool IsGamePaused { get; }
        bool IsGameStarted { get; }

        ITetrimino CurrentTetrimino { get; }
        ITetrimino NextTetrimino { get; }

        IBoard Board { get; }
        IBoard GetBoard(int playerId);

        event ClientTetriminoPlacedHandler OnTetriminoPlaced;
        event ClientStartGameHandler OnGameStarted;
        event ClientFinishGameHandler OnGameFinished;
        event ClientPauseGameHandler OnGamePaused;
        event ClientResumeGameHandler OnGameResumed;
        event ClientGameOverHandler OnGameOver;

        // UI
        event ClientRedrawHandler OnRedraw;
        event ClientRedrawBoardHandler OnRedrawBoard;
        event ClientTetriminoMovingHandler OnTetriminoMoving;
        event ClientTetriminoMovedHandler OnTetriminoMoved;

        // Client->Server command
        void Register(string name);

        // Game controller
        void Drop();
        void MoveDown();
        void MoveLeft();
        void MoveRight();
        void RotateClockwise();
        void RotateCounterClockwise();

        //
        void Dump();
    }
}
