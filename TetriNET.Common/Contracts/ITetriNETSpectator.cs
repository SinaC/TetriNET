using TetriNET.Common.DataContracts;

namespace TetriNET.Common.Contracts
{
    public interface ITetriNETSpectator
    {
        // Spectator connexion/deconnexion management
        void RegisterSpectator(ITetriNETCallback callback, Versioning clientVersion, string spectatorName);
        void UnregisterSpectator(ITetriNETCallback callback);
        void HeartbeatSpectator(ITetriNETCallback callback);

        // Chat
        void PublishSpectatorMessage(ITetriNETCallback callback, string msg);
    }
}
