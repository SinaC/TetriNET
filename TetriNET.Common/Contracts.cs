using System.Collections.Generic;
using System.ServiceModel;

namespace TetriNET.Common
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface IWCFTetriNET
    {
        [OperationContract(IsOneWay = true)]
        void RegisterPlayer(string playerName);

        [OperationContract(IsOneWay = true)]
        void UnregisterPlayer();

        [OperationContract(IsOneWay = true)]
        void Ping();

        [OperationContract(IsOneWay = true)]
        void PublishMessage(string msg);

        [OperationContract(IsOneWay = true)]
        void PlaceTetrimino(int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid);

        [OperationContract(IsOneWay = true)]
        void SendAttack(int targetId, Attacks attack);
    }

    public interface ITetriNET
    {
        void RegisterPlayer(ITetriNETCallback callback, string playerName);
        void UnregisterPlayer(ITetriNETCallback callback);
        void Ping(ITetriNETCallback callback);
        void PublishMessage(ITetriNETCallback callback, string msg);
        void PlaceTetrimino(ITetriNETCallback callback, int index, Tetriminos tetrimino, Orientations orientation, Position position, PlayerGrid grid);
        void SendAttack(ITetriNETCallback callback, int targetId, Attacks attack);
    }

    public interface ITetriNETCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnPingReceived();

        [OperationContract(IsOneWay = true)]
        void OnServerStopped();

        [OperationContract(IsOneWay = true)]
        void OnPlayerRegistered(bool succeeded, int playerId);

        [OperationContract(IsOneWay = true)]
        void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players);

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
