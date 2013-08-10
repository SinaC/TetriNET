using System.ServiceModel;

namespace TetriNET.Common
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface ITetriNET
    {
        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void PublishMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position);

        [OperationContract(IsOneWay = true)]
        void SendAttack(int targetId, Attacks attack);
    }

    public interface ITetriNETCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnPlayerRegistered(bool succeeded, int playerId);

        [OperationContract(IsOneWay = true)]
        void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino);

        [OperationContract(IsOneWay = true)]
        void OnGameFinished();

        [OperationContract(IsOneWay = true)]
        void OnServerAddLines(int lineCount);

        [OperationContract(IsOneWay = true)]
        void OnPlayerAddLines(int lineCount);

        [OperationContract(IsOneWay = true)]
        void OnPublishPlayerMessage(string playerName, string msg);

        [OperationContract(IsOneWay = true)]
        void OnPublishServerMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void OnAttackReceived(Attacks attack);

        [OperationContract(IsOneWay = true)]
        void OnAttackMessageReceived(string msg);

        [OperationContract(IsOneWay = true)]
        void OnNextTetrimino(int index, Tetriminos tetrimino);
    }
}
