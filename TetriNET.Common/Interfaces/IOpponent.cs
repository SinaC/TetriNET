namespace TetriNET.Common.Interfaces
{
    public interface IOpponent
    {
        int PlayerId { get; }
        bool IsImmune { get; }
        IBoard Board { get; }
    }
}
