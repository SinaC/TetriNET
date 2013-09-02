using System.ServiceModel;

namespace TetriNET.Common.Contracts
{
    //https://github.com/xale/iTetrinet/wiki/tetrinet-protocol%3A-client-to-server-messages
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface IWCFTetriNET
    {
        // Player connexion/deconnexion management
        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void UnregisterPlayer();

        [OperationContract(IsOneWay = true)]
        void Heartbeat();

        // Chat
        [OperationContract(IsOneWay = true)] // Partyline Chat Message
        void PublishMessage(string msg);

        // In-game
        [OperationContract(IsOneWay = true)]
        void PlaceTetrimino(int index, Tetriminos tetrimino, int orientation, int posX, int posY, byte[] grid);

        [OperationContract(IsOneWay = true)] // Field Update
        void ModifyGrid(byte[] grid);

        [OperationContract(IsOneWay = true)]
        void UseSpecial(int targetId, Specials special); // Send Special

        [OperationContract(IsOneWay = true)] // Send Classic-Style Add-Lines
        void SendLines(int count);

        [OperationContract(IsOneWay = true)] // Player Lost
        void GameLost();

        // Server management
        [OperationContract(IsOneWay = true)] // Start Game
        void StartGame();

        [OperationContract(IsOneWay = true)] // Stop Game
        void StopGame();

        [OperationContract(IsOneWay = true)] // Pause Game
        void PauseGame();

        [OperationContract(IsOneWay = true)] // Resume Game
        void ResumeGame();

        [OperationContract(IsOneWay = true)]
        void ChangeOptions(GameOptions options);

        [OperationContract(IsOneWay = true)]
        void KickPlayer(int playerId);

        [OperationContract(IsOneWay = true)]
        void BanPlayer(int playerId);

        [OperationContract(IsOneWay = true)]
        void ResetWinList();
    }
}
