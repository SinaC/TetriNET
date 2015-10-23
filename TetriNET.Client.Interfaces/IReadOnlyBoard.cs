using System.Collections.Generic;

namespace TetriNET.Client.Interfaces
{
    public interface IReadOnlyBoard
    {
        int Width { get; }
        int Height { get; }

        int PieceSpawnX { get; }
        int PieceSpawnY { get; }

        IReadOnlyCollection<byte> ReadOnlyCells { get; }
        byte this[int x, int y] { get; }
    }
}
