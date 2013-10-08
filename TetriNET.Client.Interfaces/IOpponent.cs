namespace TetriNET.Client.Interfaces
{
    public interface IOpponent
    {
        int PlayerId { get; }
        bool IsImmune { get; }
        IBoard Board { get; }
        string Team { get; }
    }
}
