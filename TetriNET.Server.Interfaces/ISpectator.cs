namespace TetriNET.Server.Interfaces
{
    public delegate void SpectatorConnectionLostHandler(ISpectator spectator);

    public interface ISpectator : IEntity
    {
        event SpectatorConnectionLostHandler OnConnectionLost;

        string Name { get; }
    }
}
