using System.ServiceModel;

namespace TetriNET.Common
{
    //https://github.com/xale/iTetrinet/wiki/tetrinet-protocol%3A-client-to-server-messages
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface IWCFTetriNET
    {
        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void UnregisterPlayer();

        [OperationContract(IsOneWay = true)]
        void Heartbeat();

        [OperationContract(IsOneWay = true)] // Partyline Chat Message
        void PublishMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void PlaceTetrimino(int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid);

        [OperationContract(IsOneWay = true)] // Field Update
        void ModifyGrid(PlayerGrid grid);

        [OperationContract(IsOneWay = true)]
        void SendAttack(int targetId, Attacks attack); // Send Special

        [OperationContract(IsOneWay = true)] // Send Classic-Style Add-Lines
        void SendLines(int count);

        [OperationContract(IsOneWay = true)] // Start Game
        void StartGame();

        [OperationContract(IsOneWay = true)] // Stop Game
        void StopGame();

        [OperationContract(IsOneWay = true)] // Pause Game
        void PauseGame();

        [OperationContract(IsOneWay = true)] // Resume Game
        void ResumeGame();

        [OperationContract(IsOneWay = true)] // Player Lost
        void GameLost();

        [OperationContract(IsOneWay = true)]
        void ChangeOptions(GameOptions options);

        [OperationContract(IsOneWay = true)]
        void KickPlayer(int playerId);

        [OperationContract(IsOneWay = true)]
        void BanPlayer(int playerId);
    }

    public interface ITetriNET
    {
        void RegisterPlayer(ITetriNETCallback callback, string playerName);
        void UnregisterPlayer(ITetriNETCallback callback);
        void Heartbeat(ITetriNETCallback callback);
        void PublishMessage(ITetriNETCallback callback, string msg); // Partyline Chat Message
        void PlaceTetrimino(ITetriNETCallback callback, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid);
        void ModifyGrid(ITetriNETCallback callback, PlayerGrid grid);
        void SendAttack(ITetriNETCallback callback, int targetId, Attacks attack);
        void SendLines(ITetriNETCallback callback, int count);
        void StartGame(ITetriNETCallback callback);
        void StopGame(ITetriNETCallback callback);
        void PauseGame(ITetriNETCallback callback);
        void ResumeGame(ITetriNETCallback callback);
        void GameLost(ITetriNETCallback callback);
        void ChangeOptions(ITetriNETCallback callback, GameOptions options);
        void KickPlayer(ITetriNETCallback callback, int playerId);
        void BanPlayer(ITetriNETCallback callback, int playerId);
    }

    // https://github.com/xale/iTetrinet/wiki/tetrinet-protocol%3A-server-to-client-messages
    public interface ITetriNETCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnHeartbeatReceived();

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnPlayerRegistered(bool succeeded, int playerId, bool gameStarted); // Player Number + In-Game

        [OperationContract(IsOneWay = true)] // Player Joined
        void OnPlayerJoined(int playerId, string name);

        [OperationContract(IsOneWay = true)] // Player Left
        void OnPlayerLeft(int playerId, string name, LeaveReasons reason);

        [OperationContract(IsOneWay = true)] // Player Lost
        void OnPlayerLost(int playerId);

        [OperationContract(IsOneWay = true)] // Player Won
        void OnPlayerWon(int playerId);

        [OperationContract(IsOneWay = true)] // New Game
        void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, GameOptions options);

        [OperationContract(IsOneWay = true)] // End Game
        void OnGameFinished();

        [OperationContract(IsOneWay = true)] // Pause Game
        void OnGamePaused();

        [OperationContract(IsOneWay = true)] // Resume Game
        void OnGameResumed();

        [OperationContract(IsOneWay = true)]
        void OnServerAddLines(int lineCount);

        [OperationContract(IsOneWay = true)] // Classic-Style Add-Lines
        void OnPlayerAddLines(int attackId, int lineCount);

        [OperationContract(IsOneWay = true)] // Partyline Chat Message
        void OnPublishPlayerMessage(string playerName, string msg);

        [OperationContract(IsOneWay = true)] // Game Chat Message
        void OnPublishServerMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void OnPublishAttackMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void OnAttackReceived(int attackId, Attacks attack); // Special Used

        [OperationContract(IsOneWay = true)]
        void OnNextTetrimino(int index, Tetriminos tetrimino);

        [OperationContract(IsOneWay = true)] // Field Update
        void OnGridModified(int playerId, PlayerGrid grid);

        [OperationContract(IsOneWay = true)]
        void OnServerMasterChanged(int playerId);
    }
}
