using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TetriNET.Common.GameDatas;
using TetriNET.Logger;

namespace TetriNET.WPF_WCF_Client.Controls
{
    public class Textures
    {
        private readonly Dictionary<Specials, Brush> _bigSpecialsBrushes = new Dictionary<Specials, Brush>();
        private readonly Dictionary<Tetriminos, Brush> _bigTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
        private readonly Dictionary<Specials, Brush> _smallSpecialsBrushes = new Dictionary<Specials, Brush>();
        private readonly Dictionary<Tetriminos, Brush> _smallTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
        private readonly Brush _bigBackground;
        private readonly Brush _smallBackground;

        public Dictionary<Specials, Brush> BigSpecialsBrushes { get { return _bigSpecialsBrushes; } }
        public Dictionary<Tetriminos, Brush> BigTetriminosBrushes { get { return _bigTetriminosBrushes; } }
        public Dictionary<Specials, Brush> SmallSpecialsBrushes { get { return _smallSpecialsBrushes; } }
        public Dictionary<Tetriminos, Brush> SmallTetriminosBrushes { get { return _smallTetriminosBrushes; } }
        public Brush BigBackground { get { return _bigBackground; }}
        public Brush SmallBackground { get { return _smallBackground; }}

        public Textures(string filename)
        {
            try
            {
                BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));

                #region Big brushes

                // Background
                _bigBackground = new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(0, 24, 192, 352),
                        Stretch = Stretch.None,
                    };
                // Tetriminos
                BigTetriminosBrushes.Add(Tetriminos.TetriminoI, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(0, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigTetriminosBrushes.Add(Tetriminos.TetriminoJ, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(32, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigTetriminosBrushes.Add(Tetriminos.TetriminoL, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(48, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigTetriminosBrushes.Add(Tetriminos.TetriminoO, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(16, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigTetriminosBrushes.Add(Tetriminos.TetriminoS, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(0, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigTetriminosBrushes.Add(Tetriminos.TetriminoT, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(16, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigTetriminosBrushes.Add(Tetriminos.TetriminoZ, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(64, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                // Specials
                //ACNRSBGQO
                BigSpecialsBrushes.Add(Specials.AddLines, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(80, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.ClearLines, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(96, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.NukeField, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(112, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.RandomBlocksClear, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(128, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.SwitchFields, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(144, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.ClearSpecialBlocks, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(160, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.BlockGravity, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(176, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.BlockQuake, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(192, 0, 16, 16),
                        Stretch = Stretch.None
                    });
                BigSpecialsBrushes.Add(Specials.BlockBomb, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(208, 0, 16, 16),
                        Stretch = Stretch.None
                    });

                #endregion

                #region Small brushes

                _smallBackground = new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(192, 24, 96, 176),
                        Stretch = Stretch.None,
                    };
                // Tetriminos
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoI, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(0, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoJ, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(16, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoL, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(24, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoO, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(8, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoS, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(0, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoT, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(8, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoZ, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(32, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                // Specials
                //ACNRSBGQO
                SmallSpecialsBrushes.Add(Specials.AddLines, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(40, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.ClearLines, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(48, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.NukeField, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(56, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.RandomBlocksClear, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(64, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.SwitchFields, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(72, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.ClearSpecialBlocks, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(80, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.BlockGravity, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(88, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.BlockQuake, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(96, 16, 8, 8),
                        Stretch = Stretch.None
                    });
                SmallSpecialsBrushes.Add(Specials.BlockBomb, new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(104, 16, 8, 8),
                        Stretch = Stretch.None
                    });

                #endregion
            }
            catch(Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Invalid texture file {0}. Exception: {1}", filename, ex);

                // Set default values
                _bigBackground = new SolidColorBrush(Colors.Black);
                BigTetriminosBrushes.Add(Tetriminos.TetriminoI, new SolidColorBrush(Colors.Blue));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoJ, new SolidColorBrush(Colors.Green));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoL, new SolidColorBrush(Colors.Magenta));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoO, new SolidColorBrush(Colors.Yellow));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoS, new SolidColorBrush(Colors.Blue));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoT, new SolidColorBrush(Colors.Yellow));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoZ, new SolidColorBrush(Colors.Red));
                BigSpecialsBrushes.Add(Specials.AddLines, CreateDummySpecialBrush(Specials.AddLines, false));
                BigSpecialsBrushes.Add(Specials.ClearLines, CreateDummySpecialBrush(Specials.ClearLines, false));
                BigSpecialsBrushes.Add(Specials.NukeField, CreateDummySpecialBrush(Specials.NukeField, false));
                BigSpecialsBrushes.Add(Specials.RandomBlocksClear, CreateDummySpecialBrush(Specials.RandomBlocksClear, false));
                BigSpecialsBrushes.Add(Specials.SwitchFields, CreateDummySpecialBrush(Specials.SwitchFields, false));
                BigSpecialsBrushes.Add(Specials.ClearSpecialBlocks, CreateDummySpecialBrush(Specials.ClearSpecialBlocks, false));
                BigSpecialsBrushes.Add(Specials.BlockGravity, CreateDummySpecialBrush(Specials.BlockGravity, false));
                BigSpecialsBrushes.Add(Specials.BlockQuake, CreateDummySpecialBrush(Specials.BlockQuake, false));
                BigSpecialsBrushes.Add(Specials.BlockBomb, CreateDummySpecialBrush(Specials.BlockBomb, false));
                BigSpecialsBrushes.Add(Specials.ClearColumn, CreateDummySpecialBrush(Specials.ClearColumn, false));
                BigSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, false));

                _smallBackground = new SolidColorBrush(Colors.Black);
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoI, new SolidColorBrush(Colors.Blue));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoJ, new SolidColorBrush(Colors.Green));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoL, new SolidColorBrush(Colors.Magenta));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoO, new SolidColorBrush(Colors.Yellow));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoS, new SolidColorBrush(Colors.Blue));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoT, new SolidColorBrush(Colors.Yellow));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoZ, new SolidColorBrush(Colors.Red));
                SmallSpecialsBrushes.Add(Specials.AddLines, CreateDummySpecialBrush(Specials.AddLines, true));
                SmallSpecialsBrushes.Add(Specials.ClearLines, CreateDummySpecialBrush(Specials.ClearLines, true));
                SmallSpecialsBrushes.Add(Specials.NukeField, CreateDummySpecialBrush(Specials.NukeField, true));
                SmallSpecialsBrushes.Add(Specials.RandomBlocksClear, CreateDummySpecialBrush(Specials.RandomBlocksClear, true));
                SmallSpecialsBrushes.Add(Specials.SwitchFields, CreateDummySpecialBrush(Specials.SwitchFields, true));
                SmallSpecialsBrushes.Add(Specials.ClearSpecialBlocks, CreateDummySpecialBrush(Specials.ClearSpecialBlocks, true));
                SmallSpecialsBrushes.Add(Specials.BlockGravity, CreateDummySpecialBrush(Specials.BlockGravity, true));
                SmallSpecialsBrushes.Add(Specials.BlockQuake, CreateDummySpecialBrush(Specials.BlockQuake, true));
                SmallSpecialsBrushes.Add(Specials.BlockBomb, CreateDummySpecialBrush(Specials.BlockBomb, true));
                SmallSpecialsBrushes.Add(Specials.ClearColumn, CreateDummySpecialBrush(Specials.ClearColumn, true));
                SmallSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, true));
            }
        }

        private Brush CreateDummySpecialBrush(Specials special, bool isSmall)
        {
            VisualBrush brush = new VisualBrush();
            //
            StackPanel aPanel = new StackPanel
                {
                    Background = new SolidColorBrush(Colors.DarkGray)
                };
            //
            TextBlock someText = new TextBlock
                {
                    Text = Mapper.MapSpecialToChar(special).ToString(CultureInfo.InvariantCulture),
                    FontSize = isSmall ? 8 : 12,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            aPanel.Children.Add(someText);
            //
            brush.Visual = aPanel;
            return brush;
        }
    }
}
