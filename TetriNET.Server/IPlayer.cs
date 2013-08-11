using System;
using TetriNET.Common;

namespace TetriNET.Server
{
    public interface IPlayer
    {
        int Id { get; }
        string Name { get; }
        ITetriNETCallback Callback { get; }
        int TetriminoIndex { get; set; }
        DateTime LastAction { get; set; }
    }
}