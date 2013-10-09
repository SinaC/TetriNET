using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TetriNET.Common.Helpers;

namespace TetriNET.Common.DataContracts
{
    [DataContract]
    public class GameOptions
    {
        [DataMember]
        public List<PieceOccurancy> PieceOccurancies { get; set; } // in %, number of entries must match Pieces enum length

        [DataMember]
        public List<SpecialOccurancy> SpecialOccurancies { get; set; } // in %, number of entries must match Specials enum length

        [DataMember]
        public bool ClassicStyleMultiplayerRules { get; set; } // if true, lines are send to other players when collapsing multiple lines (2->1, 3->2, Tetris->4)

        [DataMember]
        public int StartingLevel { get; set; } // 0 -> 100

        [DataMember]
        public int InventorySize { get; set; } // 1 -> 15

        [DataMember]
        public int LinesToMakeForSpecials { get; set; } // 1 -> 4

        [DataMember]
        public int SpecialsAddedEachTime { get; set; } // 1 -> 4

        [DataMember]
        public int DelayBeforeSuddenDeath { get; set; } // 0 -> 15, in minutes (0 means no sudden death)

        [DataMember]
        public int SuddenDeathTick { get; set; } // 1 -> 30, in seconds

        public void ResetToDefault()
        {
            // Default options
            PieceOccurancies = new List<PieceOccurancy>
            {
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoJ,
                    Occurancy = 14
                },
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoZ,
                    Occurancy = 14
                },
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoO,
                    Occurancy = 15
                },
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoL,
                    Occurancy = 14
                },
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoS,
                    Occurancy = 14
                },
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoT,
                    Occurancy = 14
                },
                new PieceOccurancy
                {
                    Value = Pieces.TetriminoI,
                    Occurancy = 15
                },
            };
            SpecialOccurancies = new List<SpecialOccurancy>
            {
                new SpecialOccurancy
                {
                    Value = Specials.AddLines,
                    Occurancy = 13
                },
                new SpecialOccurancy
                {
                    Value = Specials.ClearLines,
                    Occurancy = 13
                },
                new SpecialOccurancy
                {
                    Value = Specials.NukeField,
                    Occurancy = 3
                },
                new SpecialOccurancy
                {
                    Value = Specials.RandomBlocksClear,
                    Occurancy = 11
                },
                new SpecialOccurancy
                {
                    Value = Specials.SwitchFields,
                    Occurancy = 3
                },
                new SpecialOccurancy
                {
                    Value = Specials.ClearSpecialBlocks,
                    Occurancy = 10
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
                    Occurancy = 10
                },
                new SpecialOccurancy
                {
                    Value = Specials.ClearColumn,
                    Occurancy = 5
                },
                new SpecialOccurancy
                {
                    Value = Specials.Immunity,
                    Occurancy = 3
                },
                new SpecialOccurancy
                {
                    Value = Specials.Darkness,
                    Occurancy = 5
                },
                new SpecialOccurancy
                {
                    Value = Specials.Confusion,
                    Occurancy = 0
                },
                new SpecialOccurancy
                {
                    Value = Specials.Mutation,
                    Occurancy = 7
                },
                new SpecialOccurancy // will be available when Left Gravity is implemented
                {
                    Value = Specials.ZebraField,
                    Occurancy = 0
                },
                new SpecialOccurancy // will be available when Left Gravity is implemented
                {
                    Value = Specials.LeftGravity,
                    Occurancy = 0
                },
            };
            ClassicStyleMultiplayerRules = true;
            InventorySize = 10;
            LinesToMakeForSpecials = 1;
            SpecialsAddedEachTime = 1;
            StartingLevel = 0;
            DelayBeforeSuddenDeath = 0;
            SuddenDeathTick = 1;

            foreach (Pieces piece in EnumHelper.GetPieces(available => available).Where(piece => PieceOccurancies.All(x => x.Value != piece)))
                PieceOccurancies.Add(new PieceOccurancy
                    {
                        Value = piece,
                        Occurancy = 0
                    });
            foreach (Specials special in EnumHelper.GetSpecials(available => available).Where(special => SpecialOccurancies.All(x => x.Value != special)))
                SpecialOccurancies.Add(new SpecialOccurancy // will be available when Left Gravity is implemented
                {
                    Value = special,
                    Occurancy = 0
                });
        }
    }

}
