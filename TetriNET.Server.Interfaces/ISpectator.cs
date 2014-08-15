namespace TetriNET.Server.Interfaces
{
    public delegate void SpectatorConnectionLostEventHandler(ISpectator spectator);

    public interface ISpectator : IEntity
    {
        event SpectatorConnectionLostEventHandler ConnectionLost;

        string Name { get; }
    }
}
