using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;

namespace TetriNET.WPF_WCF_Client.Models
{
    public class Options
    {
        public GameOptions ServerOptions { get; set; } // Modified by UI or by Server on each game started
        
        public bool AutomaticallySwitchToPlayFieldOnGameStarted { get; set; } // Automatically switch to play field when game is started and to party line when game is over
        public bool AutomaticallySwitchToPartyLineOnRegistered { get; set; } // Automatically switch to party line when registering successfully

        #region Singleton
        public static readonly ThreadSafeSingleton<Options> OptionsSingleton = new ThreadSafeSingleton<Options>(() => new Options());

        private Options()
        {
            // Singleton

            AutomaticallySwitchToPlayFieldOnGameStarted = true; // Default value
            AutomaticallySwitchToPartyLineOnRegistered = true; // Default value
        }
        #endregion
    }
}
