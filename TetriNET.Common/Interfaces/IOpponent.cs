namespace TetriNET.Common.Interfaces
{
    public interface IOpponent
    {
        int PlayerId { get; }
        IBoard Board { get; }
    }
}
