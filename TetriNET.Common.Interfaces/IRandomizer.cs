namespace TetriNET.Common.Interfaces
{
    public interface IRandomizer
    {
        int Next();
        int Next(int maxValue);
        int Next(int minValue, int maxValue);
    }
}
