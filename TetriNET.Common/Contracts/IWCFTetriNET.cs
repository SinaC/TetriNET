using System.ServiceModel;
using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Contracts
{
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

        [OperationContract(IsOneWay = true)]
        void PlayerTeam(string team);

        // Chat
        [OperationContract(IsOneWay = true)]
        void PublishMessage(string msg);

        // In-game
        [OperationContract(IsOneWay = true)]
        void PlacePiece(int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid);

        [OperationContract(IsOneWay = true)]
        void ModifyGrid(byte[] grid);

        [OperationContract(IsOneWay = true)]
        void UseSpecial(int targetId, Specials special);

        [OperationContract(IsOneWay = true)]
        void SendLines(int count);

        [OperationContract(IsOneWay = true)]
        void GameLost();

        [OperationContract(IsOneWay = true)]
        void FinishContinuousSpecial(Specials special);

        [OperationContract(IsOneWay = true)]
        void EarnAchievement(int achievementId, string achievementTitle);

        // Server master commands
        [OperationContract(IsOneWay = true)]
        void StartGame();

        [OperationContract(IsOneWay = true)]
        void StopGame();

        [OperationContract(IsOneWay = true)]
        void PauseGame();

        [OperationContract(IsOneWay = true)]
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
