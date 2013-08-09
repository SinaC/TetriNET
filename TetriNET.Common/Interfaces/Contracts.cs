using System.ServiceModel;

namespace TetriNET.Common.Interfaces
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface ITetriNET
    {
        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void PublishMessage(int playerId, string msg);

        [OperationContract(IsOneWay = true)]
        void PlaceTetrimino(int playerId, Tetriminos tetrimino, Orientations orientation, Position position);

        [OperationContract(IsOneWay = true)]
        void SendAttack(int playedId, int targetId, Attacks attack);
    }

    public interface ITetriNETCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPlayerRegistered(bool succeeded, int playerId);

        [OperationContract(IsOneWay = true)]
        void OnPublishPlayerMessage(int playerId, string playerName, string msg);

        [OperationContract(IsOneWay = true)]
        void OnPublishServerMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void OnAttackReceived(Attacks attack);

        [OperationContract(IsOneWay = true)]
        void OnAttackMessageReceived(int attackId, string msg);
    }
}
