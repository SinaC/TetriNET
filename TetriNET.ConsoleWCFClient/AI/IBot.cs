using TetriNET.Common.Interfaces;
using TetriNET.Strategy;

namespace TetriNET.ConsoleWCFClient.AI
{
    public interface IBot
    {
        bool Activated { get; set; }
        int SleepTime { get; set; }

        string Name { get; }

        IClient Client { get; }
        ISpecialStrategy SpecialStrategy { get; }
        IMoveStrategy MoveStrategy { get; }
    }
}
