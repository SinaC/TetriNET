using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TetriNET.Common.GameDatas
{
    [DataContract]
    public class GameOptions
    {
        [DataMember]
        public List<TetriminoOccurancy> TetriminoOccurancies { get; set; } // in %, number of entries must match Tetriminos enum length

        [DataMember]
        public List<SpecialOccurancy> SpecialOccurancies { get; set; } // in %, number of entries must match Specials enum length

        [DataMember]
        public bool ClassicStyleMultiplayerRules { get; set; } // if true, lines are send to other players when collapsing multiple lines (2->1, 3->2, Tetris->4)

        [DataMember]
        public int StartingLevel { get; set; }

        [DataMember]
        public int InventorySize { get; set; }

        [DataMember]
        public int LinesToMakeForSpecials { get; set; }

        [DataMember]
        public int SpecialsAddedEachTime { get; set; }

        [DataMember]
        public int DelayBeforeSuddenDeath { get; set; } // in minutes, 0 means no sudden death

        [DataMember]
        public int SuddenDeathTick { get; set; } // in seconds

        public GameOptions()
        {
            // Default options
            TetriminoOccurancies = new List<TetriminoOccurancy>
            {
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoJ,
                    Occurancy = 14
                },
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoZ,
                    Occurancy = 14
                },
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoO,
                    Occurancy = 15
                },
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoL,
                    Occurancy = 14
                },
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoS,
                    Occurancy = 14
                },
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoT,
                    Occurancy = 14
                },
                new TetriminoOccurancy
                {
                    Value = Tetriminos.TetriminoI,
                    Occurancy = 15
                },
            };
            SpecialOccurancies = new List<SpecialOccurancy>
            {
                new SpecialOccurancy
                {
                    Value = Specials.AddLines,
                    Occurancy = 18
                },
                new SpecialOccurancy
                {
                    Value = Specials.ClearLines,
                    Occurancy = 17
                },
                new SpecialOccurancy
                {
                    Value = Specials.NukeField,
                    Occurancy = 3
                },
                new SpecialOccurancy
                {
                    Value = Specials.RandomBlocksClear,
                    Occurancy = 13
                },
                new SpecialOccurancy
                {
                    Value = Specials.SwitchFields,
                    Occurancy = 3
                },
                new SpecialOccurancy
                {
                    Value = Specials.ClearSpecialBlocks,
                    Occurancy = 13
                },
                new SpecialOccurancy
                {
                    Value = Specials.BlockGravity,
                    Occurancy = 6
                },
                new SpecialOccurancy
                {
                    Value = Specials.BlockQuake,
                    Occurancy = 11
                },
                new SpecialOccurancy
                {
                    Value = Specials.BlockBomb,
                    Occurancy = 13
                },
                new SpecialOccurancy
                {
                    Value = Specials.ClearColumn,
                    Occurancy = 5
                }
            };
            ClassicStyleMultiplayerRules = true;
            InventorySize = 10;
            LinesToMakeForSpecials = 1;
            SpecialsAddedEachTime = 1;
            StartingLevel = 0;
            DelayBeforeSuddenDeath = 0;
            SuddenDeathTick = 1;
        }
    }

}
