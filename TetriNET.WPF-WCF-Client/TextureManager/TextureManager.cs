using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Logger;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.TextureManager
{
    public sealed class TextureManager : ITextureManager
    {
        private Dictionary<Specials, Brush> BigSpecialsBrushes { get; set; }
        private Dictionary<Tetriminos, Brush> BigTetriminosBrushes { get; set; }
        private Dictionary<Specials, Brush> SmallSpecialsBrushes { get; set; }
        private Dictionary<Tetriminos, Brush> SmallTetriminosBrushes { get; set; }
        private Brush BigBackground { get; set; }
        private Brush SmallBackground { get; set; }

        #region Singleton
        public static ThreadSafeSingleton<TextureManager> TexturesSingleton = new ThreadSafeSingleton<TextureManager>(() => new TextureManager());

        private TextureManager()
        {
            // Singleton
        }
        #endregion

        public void SaveToPath(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            SaveBrushToFile(BigBackground, 192, 352, Path.Combine(folderPath, "bigbackground.bmp"));
            SaveBrushToFile(SmallBackground, 96, 176, Path.Combine(folderPath, "smallbackground.bmp"));
            foreach (KeyValuePair<Specials, Brush> special in BigSpecialsBrushes)
            {
                string filePath = Path.Combine(folderPath, "big_" + special.Key + ".bmp");
                SaveBrushToFile(special.Value, 16, 16, filePath);
            }
            foreach (KeyValuePair<Specials, Brush> special in SmallSpecialsBrushes)
            {
                string filePath = Path.Combine(folderPath, "small_" + special.Key + ".bmp");
                SaveBrushToFile(special.Value, 8, 8, filePath);
            }
            foreach (KeyValuePair<Tetriminos, Brush> tetrimino in BigTetriminosBrushes)
            {
                string filePath = Path.Combine(folderPath, "big_" + tetrimino.Key + ".bmp");
                SaveBrushToFile(tetrimino.Value, 16, 16, filePath);
            }
            foreach (KeyValuePair<Tetriminos, Brush> tetrimino in BigTetriminosBrushes)
            {
                string filePath = Path.Combine(folderPath, "small_" + tetrimino.Key + ".bmp");
                SaveBrushToFile(tetrimino.Value, 8, 8, filePath);
            }
        }

        public void ReadFromPath(string folderPath)
        {
            BigBackground = ReadBackground(Path.Combine(folderPath, "bigbackground.bmp"), false);
            SmallBackground = ReadBackground(Path.Combine(folderPath, "smallbackground.bmp"), true);

            BigSpecialsBrushes = new Dictionary<Specials, Brush>();
            SmallSpecialsBrushes = new Dictionary<Specials, Brush>();
            BigTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
            SmallTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
            if (Directory.Exists(folderPath))
            {
                foreach (Specials special in Enum.GetValues(typeof(Specials)).Cast<Specials>().Where(x => x != Specials.Invalid))
                {
                    string bigFilename = Path.Combine(folderPath, "big_" + special + ".bmp");
                    BigSpecialsBrushes.Add(special, ReadSpecialBrush(special, bigFilename, false));
                    string smallFilename = Path.Combine(folderPath, "small_" + special + ".bmp");
                    SmallSpecialsBrushes.Add(special, ReadSpecialBrush(special, smallFilename, true));
                }

                foreach (Tetriminos tetrimino in Enum.GetValues(typeof(Tetriminos)).Cast<Tetriminos>().Where(x => x != Tetriminos.Invalid && x != Tetriminos.TetriminoLast && !x.ToString().Contains("Reserved")))
                {
                    string bigFilename = Path.Combine(folderPath, "big_" + tetrimino + ".bmp");
                    BigTetriminosBrushes.Add(tetrimino, ReadTetriminoBrush(tetrimino, bigFilename, false));
                    string smallFilename = Path.Combine(folderPath, "small_" + tetrimino + ".bmp");
                    SmallTetriminosBrushes.Add(tetrimino, ReadTetriminoBrush(tetrimino, smallFilename, true));
                }
            }
            else
            {
                foreach (Specials special in Enum.GetValues(typeof(Specials)).Cast<Specials>().Where(x => x != Specials.Invalid))
                {
                    BigSpecialsBrushes.Add(special, CreateDummySpecialBrush(special, false));
                    SmallSpecialsBrushes.Add(special, CreateDummySpecialBrush(special, true));
                }

                foreach (Tetriminos tetrimino in Enum.GetValues(typeof(Tetriminos)).Cast<Tetriminos>().Where(x => x != Tetriminos.Invalid && x != Tetriminos.TetriminoLast && !x.ToString().Contains("Reserved")))
                {
                    BigTetriminosBrushes.Add(tetrimino, CreateDummyTetriminoBrush(tetrimino, false));
                    SmallTetriminosBrushes.Add(tetrimino, CreateDummyTetriminoBrush(tetrimino, true));
                }
            }
        }

        public void ReadFromFile(string filename)
        {
            BigTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
            BigSpecialsBrushes = new Dictionary<Specials, Brush>();
            SmallTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
            SmallSpecialsBrushes = new Dictionary<Specials, Brush>();
            
            try
            {
                BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));

                #region Big brushes

                // Background
                BigBackground = ExtractBackground(image, 0, 24, 192, 352, false);
                // Tetriminos
                BigTetriminosBrushes.Add(Tetriminos.TetriminoI, ExtractTetriminoBrush(Tetriminos.TetriminoI, image, 0, 0, 16, 16, false));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoJ, ExtractTetriminoBrush(Tetriminos.TetriminoJ, image, 32, 0, 16, 16, false));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoL, ExtractTetriminoBrush(Tetriminos.TetriminoL, image, 48, 0, 16, 16, false));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoO, ExtractTetriminoBrush(Tetriminos.TetriminoO, image, 16, 0, 16, 16, false));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoS, ExtractTetriminoBrush(Tetriminos.TetriminoS, image, 0, 0, 16, 16, false));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoT, ExtractTetriminoBrush(Tetriminos.TetriminoT, image, 16, 0, 16, 16, false));
                BigTetriminosBrushes.Add(Tetriminos.TetriminoZ, ExtractTetriminoBrush(Tetriminos.TetriminoZ, image, 64, 0, 16, 16, false));
                // Specials
                BigSpecialsBrushes.Add(Specials.AddLines, ExtractSpecialBrush(Specials.AddLines, image, 80, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.ClearLines, ExtractSpecialBrush(Specials.ClearLines, image, 96, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.NukeField, ExtractSpecialBrush(Specials.NukeField, image, 112, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.RandomBlocksClear, ExtractSpecialBrush(Specials.RandomBlocksClear, image, 128, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.SwitchFields, ExtractSpecialBrush(Specials.SwitchFields, image, 144, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.ClearSpecialBlocks, ExtractSpecialBrush(Specials.ClearSpecialBlocks, image, 160, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.BlockGravity, ExtractSpecialBrush(Specials.BlockGravity, image, 176, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.BlockQuake, ExtractSpecialBrush(Specials.BlockQuake, image, 192, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.BlockBomb, ExtractSpecialBrush(Specials.BlockBomb, image, 208, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.ClearColumn, ExtractSpecialBrush(Specials.ClearColumn, image, 224, 0, 16, 16, false));
                //  in tetrinet2 bitmap, Immunity is between ClearColumn & Darkness
                BigSpecialsBrushes.Add(Specials.Darkness, ExtractSpecialBrush(Specials.Darkness, image, 256, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.Confusion, ExtractSpecialBrush(Specials.Darkness, image, 272, 0, 16, 16, false));
                //BigSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, false)); // will be available when Left Gravity is implemented

                #endregion

                #region Small brushes

                SmallBackground = ExtractBackground(image, 192, 24, 96, 176, true);
                // Tetriminos
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoI, ExtractTetriminoBrush(Tetriminos.TetriminoI, image, 0, 16, 8, 8, true));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoJ, ExtractTetriminoBrush(Tetriminos.TetriminoJ, image, 16, 16, 8, 8, true));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoL, ExtractTetriminoBrush(Tetriminos.TetriminoL, image, 24, 16, 8, 8, true));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoO, ExtractTetriminoBrush(Tetriminos.TetriminoO, image, 8, 16, 8, 8, true));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoS, ExtractTetriminoBrush(Tetriminos.TetriminoS, image, 0, 16, 8, 8, true));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoT, ExtractTetriminoBrush(Tetriminos.TetriminoT, image, 8, 16, 8, 8, true));
                SmallTetriminosBrushes.Add(Tetriminos.TetriminoZ, ExtractTetriminoBrush(Tetriminos.TetriminoZ, image, 32, 16, 8, 8, true));
                // Specials
                SmallSpecialsBrushes.Add(Specials.AddLines, ExtractSpecialBrush(Specials.AddLines, image, 40, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.ClearLines, ExtractSpecialBrush(Specials.ClearLines, image, 48, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.NukeField, ExtractSpecialBrush(Specials.NukeField, image, 56, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.RandomBlocksClear, ExtractSpecialBrush(Specials.RandomBlocksClear, image, 64, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.SwitchFields, ExtractSpecialBrush(Specials.SwitchFields, image, 72, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.ClearSpecialBlocks, ExtractSpecialBrush(Specials.ClearSpecialBlocks, image, 80, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.BlockGravity, ExtractSpecialBrush(Specials.BlockGravity, image, 88, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.BlockQuake, ExtractSpecialBrush(Specials.BlockQuake, image, 96, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.BlockBomb, ExtractSpecialBrush(Specials.BlockBomb, image, 104, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.ClearColumn, ExtractSpecialBrush(Specials.ClearColumn, image, 112, 16, 8, 8, true));
                //  in tetrinet2 bitmap, Immunity is between ClearColumn & Darkness
                SmallSpecialsBrushes.Add(Specials.Darkness, ExtractSpecialBrush(Specials.Darkness, image, 128, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.Confusion, ExtractSpecialBrush(Specials.Confusion, image, 136, 16, 8, 8, true));
                //SmallSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, true)); // will be available when Left Gravity is implemented
                #endregion
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Invalid texture file {0}. Exception: {1}", filename, ex);

                // Set default values
                BigBackground = new SolidColorBrush(Colors.Black);
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
                //BigSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, false)); // will be available when Left Gravity is implemented

                SmallBackground = new SolidColorBrush(Colors.Black);
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
                SmallSpecialsBrushes.Add(Specials.Darkness, CreateDummySpecialBrush(Specials.Darkness, true));
                SmallSpecialsBrushes.Add(Specials.Confusion, CreateDummySpecialBrush(Specials.Confusion, true));
                //SmallSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, true)); // will be available when Left Gravity is implemented
            }
        }

        #region ITextureManager
        public Brush GetBigTetrimino(Tetriminos tetrimino)
        {
            return BigTetriminosBrushes[tetrimino];
        }

        public Brush GetSmallTetrimino(Tetriminos tetrimino)
        {
            return SmallTetriminosBrushes[tetrimino];
        }

        public Brush GetBigSpecial(Specials special)
        {
            return BigSpecialsBrushes[special];
        }

        public Brush GetSmallSpecial(Specials special)
        {
            return SmallSpecialsBrushes[special];
        }

        public Brush GetBigBackground()
        {
            return BigBackground;
        }

        public Brush GetSmallBackground()
        {
            return SmallBackground;
        }
        #endregion

        private static Brush ExtractBackground(BitmapImage image, int posX, int posY, int width, int height, bool isSmall)
        {
            Brush background;
            try
            {
                background = new ImageBrush(image)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(posX, posY, width, height),
                    Stretch = Stretch.None
                };
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while extracting background texture. Image {0}. Exception: {1}", image.BaseUri, ex);
                background = new SolidColorBrush(Colors.Black);
            }
            return background;
        }

        private static Brush ExtractTetriminoBrush(Tetriminos tetrimino, BitmapImage image, int posX, int posY, int width, int height, bool isSmall)
        {
            Brush specialBrush;
            try
            {
                specialBrush = new ImageBrush(image)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Viewbox = new Rect(posX, posY, width, height),
                    Stretch = Stretch.None
                };
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while extracting texture for {0}. Image {1}. Exception: {2}", tetrimino, image.BaseUri, ex);
                specialBrush = CreateDummyTetriminoBrush(tetrimino, isSmall);
            }
            return specialBrush;
        }

        private static Brush ExtractSpecialBrush(Specials special, BitmapImage image, int posX, int posY, int width, int height, bool isSmall)
        {
            Brush specialBrush;
            try
            {
                specialBrush = new ImageBrush(image)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = new Rect(posX, posY, width, height),
                        Stretch = Stretch.None
                    };
            }
            catch(Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while extracting texture for {0}. Image {1}. Exception: {2}", special, image.BaseUri, ex);
                specialBrush = CreateDummySpecialBrush(special, isSmall);
            }
            return specialBrush;
        }

        private static Brush ReadBackground(string filename, bool isSmall)
        {
            Brush background;
            try
            {
                background = LoadBrushFromFile(filename);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while reading background file {0}. Exception: {1}", filename, ex);
                background = new SolidColorBrush(Colors.Black);
            }
            return background;
        }

        private static Brush ReadTetriminoBrush(Tetriminos tetrimino, string filename, bool isSmall)
        {
            Brush tetriminoBrush;
            try
            {
                tetriminoBrush = LoadBrushFromFile(filename);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while reading texture for {0}. File {1}. Exception: {2}", tetrimino, filename, ex);
                tetriminoBrush = CreateDummyTetriminoBrush(tetrimino, isSmall);
            }
            return tetriminoBrush;
        }

        private static Brush ReadSpecialBrush(Specials special, string filename, bool isSmall)
        {
            Brush specialBrush;
            try
            {
                specialBrush = LoadBrushFromFile(filename);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while reading texture for {0}. File {1}. Exception: {2}", special, filename, ex);
                specialBrush = CreateDummySpecialBrush(special, isSmall);
            }
            return specialBrush;
        }

        private static Brush CreateDummyTetriminoBrush(Tetriminos tetrimino, bool isSmall)
        {
            switch (tetrimino)
            {
                case Tetriminos.TetriminoI:
                    return new SolidColorBrush(Colors.Blue);
                case Tetriminos.TetriminoJ:
                    return new SolidColorBrush(Colors.Green);
                case Tetriminos.TetriminoL:
                    return new SolidColorBrush(Colors.Magenta);
                case Tetriminos.TetriminoO:
                    return new SolidColorBrush(Colors.Yellow);
                case Tetriminos.TetriminoS:
                    return new SolidColorBrush(Colors.Blue);
                case Tetriminos.TetriminoT:
                    return new SolidColorBrush(Colors.Yellow);
                case Tetriminos.TetriminoZ:
                    return new SolidColorBrush(Colors.Red);
                default:
                    return new SolidColorBrush(Colors.White);
            }
        }

        private static Brush CreateDummySpecialBrush(Specials special, bool isSmall)
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

        private static BitmapSource BitmapSourceFromBrush(Brush drawingBrush, int width, int height, int dpi = 96)
        {
            // RenderTargetBitmap = builds a bitmap rendering of a visual
            RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, dpi, dpi, PixelFormats.Default);

            // Drawing visual allows us to compose graphic drawing parts into a visual to render
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext context = drawingVisual.RenderOpen())
            {
                // Declaring drawing a rectangle using the input brush to fill up the visual
                context.DrawRectangle(drawingBrush, null, new Rect(0, 0, width, height));
            }

            // Actually rendering the bitmap
            rtb.Render(drawingVisual);
            return rtb;
        }

        private static void SaveBrushToFile(Brush brush, int width, int height, string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                BitmapSource bmp = BitmapSourceFromBrush(brush, width, height);

                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fileStream);
            }
        }

        private static Brush LoadBrushFromFile(string filename)
        {
            //using (var fileStream = new FileStream(filepath, FileMode.Open))
            //{
            //    BitmapDecoder decoder = new BmpBitmapDecoder(fileStream, BitmapCreateOptions.None, BitmapCacheOption.Default);
            //    return new ImageBrush(decoder.Frames[0]);
            //}
            BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));
            return new ImageBrush(image);
        }
    }

    //public sealed class Textures
    //{
    //    public Dictionary<Specials, Brush> BigSpecialsBrushes { get; private set; }
    //    public Dictionary<Tetriminos, Brush> BigTetriminosBrushes { get; private set; }
    //    public Dictionary<Specials, Brush> SmallSpecialsBrushes { get; private set; }
    //    public Dictionary<Tetriminos, Brush> SmallTetriminosBrushes { get; private set; }
    //    public Brush BigBackground { get; private set; }
    //    public Brush SmallBackground { get; private set; }

    //    #region Singleton
    //    public static ThreadSafeSingleton<Textures> TexturesSingleton  = new ThreadSafeSingleton<Textures>(() => new Textures());

    //    private Textures()
    //    {
    //        // Singleton
    //    }
    //    #endregion

    //    public void ReadFromFile(string filename)
    //    {
    //        try
    //        {
    //            BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));

    //            #region Big brushes
    //            BigTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
    //            BigSpecialsBrushes = new Dictionary<Specials, Brush>();

    //            // Background
    //            BigBackground = new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(0, 24, 192, 352),
    //                    Stretch = Stretch.None,
    //                };
    //            // Tetriminos
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoI, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(0, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoJ, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(32, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoL, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(48, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoO, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(16, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoS, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(0, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoT, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(16, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoZ, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(64, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            // Specials
    //            //ACNRSBGQO
    //            BigSpecialsBrushes.Add(Specials.AddLines, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(80, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.ClearLines, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(96, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.NukeField, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(112, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.RandomBlocksClear, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(128, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.SwitchFields, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(144, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.ClearSpecialBlocks, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(160, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.BlockGravity, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(176, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.BlockQuake, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(192, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });
    //            BigSpecialsBrushes.Add(Specials.BlockBomb, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(208, 0, 16, 16),
    //                    Stretch = Stretch.None
    //                });

    //            #endregion

    //            #region Small brushes
    //            SmallTetriminosBrushes = new Dictionary<Tetriminos, Brush>();
    //            SmallSpecialsBrushes = new Dictionary<Specials, Brush>();

    //            SmallBackground = new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(192, 24, 96, 176),
    //                    Stretch = Stretch.None,
    //                };
    //            // Tetriminos
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoI, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(0, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoJ, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(16, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoL, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(24, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoO, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(8, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoS, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(0, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoT, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(8, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoZ, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(32, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            // Specials
    //            //ACNRSBGQO
    //            SmallSpecialsBrushes.Add(Specials.AddLines, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(40, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.ClearLines, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(48, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.NukeField, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(56, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.RandomBlocksClear, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(64, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.SwitchFields, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(72, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.ClearSpecialBlocks, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(80, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.BlockGravity, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(88, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.BlockQuake, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(96, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });
    //            SmallSpecialsBrushes.Add(Specials.BlockBomb, new ImageBrush(image)
    //                {
    //                    ViewboxUnits = BrushMappingMode.Absolute,
    //                    Viewbox = new Rect(104, 16, 8, 8),
    //                    Stretch = Stretch.None
    //                });

    //            #endregion
    //        }
    //        catch(Exception ex)
    //        {
    //            Log.WriteLine(Log.LogLevels.Error, "Invalid texture file {0}. Exception: {1}", filename, ex);

    //            // Set default values
    //            BigBackground = new SolidColorBrush(Colors.Black);
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoI, new SolidColorBrush(Colors.Blue));
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoJ, new SolidColorBrush(Colors.Green));
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoL, new SolidColorBrush(Colors.Magenta));
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoO, new SolidColorBrush(Colors.Yellow));
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoS, new SolidColorBrush(Colors.Blue));
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoT, new SolidColorBrush(Colors.Yellow));
    //            BigTetriminosBrushes.Add(Tetriminos.TetriminoZ, new SolidColorBrush(Colors.Red));
    //            BigSpecialsBrushes.Add(Specials.AddLines, CreateDummySpecialBrush(Specials.AddLines, false));
    //            BigSpecialsBrushes.Add(Specials.ClearLines, CreateDummySpecialBrush(Specials.ClearLines, false));
    //            BigSpecialsBrushes.Add(Specials.NukeField, CreateDummySpecialBrush(Specials.NukeField, false));
    //            BigSpecialsBrushes.Add(Specials.RandomBlocksClear, CreateDummySpecialBrush(Specials.RandomBlocksClear, false));
    //            BigSpecialsBrushes.Add(Specials.SwitchFields, CreateDummySpecialBrush(Specials.SwitchFields, false));
    //            BigSpecialsBrushes.Add(Specials.ClearSpecialBlocks, CreateDummySpecialBrush(Specials.ClearSpecialBlocks, false));
    //            BigSpecialsBrushes.Add(Specials.BlockGravity, CreateDummySpecialBrush(Specials.BlockGravity, false));
    //            BigSpecialsBrushes.Add(Specials.BlockQuake, CreateDummySpecialBrush(Specials.BlockQuake, false));
    //            BigSpecialsBrushes.Add(Specials.BlockBomb, CreateDummySpecialBrush(Specials.BlockBomb, false));
    //            BigSpecialsBrushes.Add(Specials.ClearColumn, CreateDummySpecialBrush(Specials.ClearColumn, false));
    //            BigSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, false));

    //            SmallBackground = new SolidColorBrush(Colors.Black);
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoI, new SolidColorBrush(Colors.Blue));
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoJ, new SolidColorBrush(Colors.Green));
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoL, new SolidColorBrush(Colors.Magenta));
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoO, new SolidColorBrush(Colors.Yellow));
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoS, new SolidColorBrush(Colors.Blue));
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoT, new SolidColorBrush(Colors.Yellow));
    //            SmallTetriminosBrushes.Add(Tetriminos.TetriminoZ, new SolidColorBrush(Colors.Red));
    //            SmallSpecialsBrushes.Add(Specials.AddLines, CreateDummySpecialBrush(Specials.AddLines, true));
    //            SmallSpecialsBrushes.Add(Specials.ClearLines, CreateDummySpecialBrush(Specials.ClearLines, true));
    //            SmallSpecialsBrushes.Add(Specials.NukeField, CreateDummySpecialBrush(Specials.NukeField, true));
    //            SmallSpecialsBrushes.Add(Specials.RandomBlocksClear, CreateDummySpecialBrush(Specials.RandomBlocksClear, true));
    //            SmallSpecialsBrushes.Add(Specials.SwitchFields, CreateDummySpecialBrush(Specials.SwitchFields, true));
    //            SmallSpecialsBrushes.Add(Specials.ClearSpecialBlocks, CreateDummySpecialBrush(Specials.ClearSpecialBlocks, true));
    //            SmallSpecialsBrushes.Add(Specials.BlockGravity, CreateDummySpecialBrush(Specials.BlockGravity, true));
    //            SmallSpecialsBrushes.Add(Specials.BlockQuake, CreateDummySpecialBrush(Specials.BlockQuake, true));
    //            SmallSpecialsBrushes.Add(Specials.BlockBomb, CreateDummySpecialBrush(Specials.BlockBomb, true));
    //            SmallSpecialsBrushes.Add(Specials.ClearColumn, CreateDummySpecialBrush(Specials.ClearColumn, true));
    //            SmallSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, true));
    //        }
    //    }

    //    private Brush CreateDummySpecialBrush(Specials special, bool isSmall)
    //    {
    //        VisualBrush brush = new VisualBrush();
    //        //
    //        StackPanel aPanel = new StackPanel
    //            {
    //                Background = new SolidColorBrush(Colors.DarkGray)
    //            };
    //        //
    //        TextBlock someText = new TextBlock
    //            {
    //                Text = Mapper.MapSpecialToChar(special).ToString(CultureInfo.InvariantCulture),
    //                FontSize = isSmall ? 8 : 12,
    //                HorizontalAlignment = HorizontalAlignment.Center,
    //                VerticalAlignment = VerticalAlignment.Center
    //            };
    //        aPanel.Children.Add(someText);
    //        //
    //        brush.Visual = aPanel;
    //        return brush;
    //    }
    //}
}
