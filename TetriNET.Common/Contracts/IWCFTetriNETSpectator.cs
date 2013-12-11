using System.ServiceModel;

namespace TetriNET.Common.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required, CallbackContract = typeof(ITetriNETCallback))]
    public interface IWCFTetriNETSpectator
    {
        // Spectator connexion/deconnexion management
        [OperationContract(IsOneWay = true)]
        void RegisterSpectator(string spectatorName);

        [OperationContract(IsOneWay = true)]
        void UnregisterSpectator();

        [OperationContract(IsOneWay = true)]
        void HeartbeatSpectator();

        // Chat
        [OperationContract(IsOneWay = true)]
        void PublishSpectatorMessage(string msg);
    }
}
