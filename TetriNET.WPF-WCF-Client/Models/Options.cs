using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.Properties;

namespace TetriNET.WPF_WCF_Client.Models
{
    public class Options
    {
        public const int Width = 12;
        public const int Height = 22;

        public GameOptions ServerOptions { get; set; } // Modified by UI and by Server on each game started

        // Automatically switch to play field when game is started and to party line when game is over
        private bool _automaticallySwitchToPlayFieldOnGameStarted;
        public bool AutomaticallySwitchToPlayFieldOnGameStarted
        {
            get
            {
                return _automaticallySwitchToPlayFieldOnGameStarted;
            }
            set
            {
                if (_automaticallySwitchToPlayFieldOnGameStarted != value)
                {
                    _automaticallySwitchToPlayFieldOnGameStarted = value;
                    Settings.Default.AutomaticallySwitchToPlayFieldOnGameStarted = _automaticallySwitchToPlayFieldOnGameStarted;
                    Settings.Default.Save();
                }
            }
        }

        // Automatically switch to party line when registering successfully
        private bool _automaticallySwitchToPartyLineOnRegistered;
        public bool AutomaticallySwitchToPartyLineOnRegistered
        {
            get { return _automaticallySwitchToPartyLineOnRegistered; }
            set
            {
                if (_automaticallySwitchToPartyLineOnRegistered != value)
                {
                    _automaticallySwitchToPartyLineOnRegistered = value;
                    Settings.Default.AutomaticallySwitchToPartyLineOnRegistered = _automaticallySwitchToPartyLineOnRegistered;
                    Settings.Default.Save();
                }
            }
        }

        private bool _displayOpponentsFieldEvenWhenNotPlaying;
        public bool DisplayOpponentsFieldEvenWhenNotPlaying
        {
            get { return _displayOpponentsFieldEvenWhenNotPlaying; }
            set
            {
                if (_displayOpponentsFieldEvenWhenNotPlaying != value)
                {
                    _displayOpponentsFieldEvenWhenNotPlaying = value;
                    Settings.Default.DisplayOpponentsFieldEvenWhenNotPlaying = _displayOpponentsFieldEvenWhenNotPlaying;
                    Settings.Default.Save();
                }
            }
        }

        #region Singleton
        public static readonly ThreadSafeSingleton<Options> OptionsSingleton = new ThreadSafeSingleton<Options>(() => new Options());

        private Options()
        {
            // Singleton
        }
        #endregion

        //public void AddDefaultForMissingOptions()
        //{
        //}
    }
}
