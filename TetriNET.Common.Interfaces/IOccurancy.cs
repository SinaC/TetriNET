namespace TetriNET.Common.Interfaces
{
    public interface IOccurancy<out T>
    {
        T Value { get; }
        int Occurancy { get; }
    }

}
