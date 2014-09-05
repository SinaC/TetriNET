using System;
using System.Threading;

namespace TetriNET.Common.Interfaces
{
    public interface IActionQueue
    {
        int ActionCount { get; }

        void Start(CancellationTokenSource cancellationTokenSource); // Cancel token to Stop
        void Wait(int milliseconds); // Wait until stopped or timeout elapsed

        void AddAction(Action action);
        void ResetActions();
    }
}
