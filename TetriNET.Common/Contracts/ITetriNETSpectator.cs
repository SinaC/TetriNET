namespace TetriNET.Common.Contracts
{
    public interface ITetriNETSpectator
    {
        // Spectator connexion/deconnexion management
        void RegisterSpectator(ITetriNETCallback callback, string spectatorName);
        void UnregisterSpectator(ITetriNETCallback callback);
        void HeartbeatSpectator(ITetriNETCallback callback);

        // Chat
        void PublishSpectatorMessage(ITetriNETCallback callback, string msg);
    }
}
