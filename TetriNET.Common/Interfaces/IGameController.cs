namespace TetriNET.Common.Interfaces
{
    public enum Commands
    {
        Invalid,

        Drop,
        Down,
        Left,
        Right,
        RotateClockwise,
        RotateCounterclockwise,

        DiscardFirstSpecial,
        UseSpecialOn1,
        UseSpecialOn2,
        UseSpecialOn3,
        UseSpecialOn4,
        UseSpecialOn5,
        UseSpecialOn6,
    }

    public interface IGameController
    {
        IClient Client { get; }

        void AddSensibility(Commands cmd, int interval);
        void RemoveSensibility(Commands cmd);

        void UnsubscribeFromClientEvents();

        void KeyDown(Commands cmd);
        void KeyUp(Commands cmd);
    }
}
