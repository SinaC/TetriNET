namespace TetriNET.Server.Interfaces
{
    public delegate void SpectatorConnectionLostEventHandler(ISpectator spectator);

    public interface ISpectator : IEntity
    {
        event SpectatorConnectionLostEventHandler ConnectionLost;

        int Id { get; }
        string Name { get; }
    }
}
