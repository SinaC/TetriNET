using System.ComponentModel;

namespace TetriNET.Client.Interfaces
{
    public enum Commands
    {
        Invalid,

        [Description("Drop")]
        Drop,
        [Description("Down")]
        Down,
        [Description("Left")]
        Left,
        [Description("Right")]
        Right,
        [Description("Rotate clockwise")]
        RotateClockwise,
        [Description("Rotate counterclockwise")]
        RotateCounterclockwise,
        [Description("Hold")]
        Hold,

        [Description("Discard special")]
        DiscardFirstSpecial,
        [Description("Use special on player 1")]
        UseSpecialOn1,
        [Description("Use special on player 2")]
        UseSpecialOn2,
        [Description("Use special on player 3")]
        UseSpecialOn3,
        [Description("Use special on player 4")]
        UseSpecialOn4,
        [Description("Use special on player 5")]
        UseSpecialOn5,
        [Description("Use special on player 6")]
        UseSpecialOn6,
        [Description("User special on yourself")]
        UseSpecialOnSelf,
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
