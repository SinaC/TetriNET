using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.TextureManager
{
    public sealed class TextureManager : ITextureManager
    {
        private Dictionary<Specials, Brush> BigSpecialsBrushes { get; set; }
        private Dictionary<Pieces, Brush> BigPiecesBrushes { get; set; }
        private Dictionary<Specials, Brush> SmallSpecialsBrushes { get; set; }
        private Dictionary<Pieces, Brush> SmallPiecesBrushes { get; set; }
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
            SaveBrushToFile(BigBackground, 192, 352, Path.Combine(folderPath, "big_background.bmp"));
            SaveBrushToFile(SmallBackground, 96, 176, Path.Combine(folderPath, "small_background.bmp"));
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
            foreach (KeyValuePair<Pieces, Brush> piece in BigPiecesBrushes)
            {
                string filePath = Path.Combine(folderPath, "big_" + piece.Key + ".bmp");
                SaveBrushToFile(piece.Value, 16, 16, filePath);
            }
            foreach (KeyValuePair<Pieces, Brush> piece in SmallPiecesBrushes)
            {
                string filePath = Path.Combine(folderPath, "small_" + piece.Key + ".bmp");
                SaveBrushToFile(piece.Value, 8, 8, filePath);
            }
        }

        public void ReadFromPath(string folderPath)
        {
            BigBackground = ReadBackground(Path.Combine(folderPath, "big_background.bmp"), false);
            SmallBackground = ReadBackground(Path.Combine(folderPath, "small_background.bmp"), true);

            BigSpecialsBrushes = new Dictionary<Specials, Brush>();
            SmallSpecialsBrushes = new Dictionary<Specials, Brush>();
            BigPiecesBrushes = new Dictionary<Pieces, Brush>();
            SmallPiecesBrushes = new Dictionary<Pieces, Brush>();
            if (Directory.Exists(folderPath))
            {
                foreach (Specials special in EnumHelper.GetSpecials(available => available))
                {
                    string bigFilename = Path.Combine(folderPath, "big_" + special + ".bmp");
                    BigSpecialsBrushes.Add(special, ReadSpecialBrush(special, bigFilename, false));
                    string smallFilename = Path.Combine(folderPath, "small_" + special + ".bmp");
                    SmallSpecialsBrushes.Add(special, ReadSpecialBrush(special, smallFilename, true));
                }

                foreach (Pieces piece in EnumHelper.GetPieces(available => available))
                {
                    string bigFilename = Path.Combine(folderPath, "big_" + piece + ".bmp");
                    BigPiecesBrushes.Add(piece, ReadPieceBrush(piece, bigFilename, false));
                    string smallFilename = Path.Combine(folderPath, "small_" + piece + ".bmp");
                    SmallPiecesBrushes.Add(piece, ReadPieceBrush(piece, smallFilename, true));
                }
            }
            else
            {
                foreach (Specials special in EnumHelper.GetSpecials(available => available))
                {
                    BigSpecialsBrushes.Add(special, CreateDummySpecialBrush(special, false));
                    SmallSpecialsBrushes.Add(special, CreateDummySpecialBrush(special, true));
                }

                foreach (Pieces piece in EnumHelper.GetPieces(available => available))
                {
                    BigPiecesBrushes.Add(piece, CreateDummyPieceBrush(piece, false));
                    SmallPiecesBrushes.Add(piece, CreateDummyPieceBrush(piece, true));
                }
            }
        }

        public void ReadFromFile(string filename)
        {
            BigPiecesBrushes = new Dictionary<Pieces, Brush>();
            BigSpecialsBrushes = new Dictionary<Specials, Brush>();
            SmallPiecesBrushes = new Dictionary<Pieces, Brush>();
            SmallSpecialsBrushes = new Dictionary<Specials, Brush>();
            
            try
            {
                BitmapImage image = new BitmapImage(new Uri(filename, UriKind.RelativeOrAbsolute));

                #region Big brushes

                // Background
                BigBackground = ExtractBackground(image, 0, 24, 192, 352, false);
                // Tetriminoes
                BigPiecesBrushes.Add(Pieces.TetriminoI, ExtractPieceBrush(Pieces.TetriminoI, image, 0, 0, 16, 16, false));
                BigPiecesBrushes.Add(Pieces.TetriminoJ, ExtractPieceBrush(Pieces.TetriminoJ, image, 32, 0, 16, 16, false));
                BigPiecesBrushes.Add(Pieces.TetriminoL, ExtractPieceBrush(Pieces.TetriminoL, image, 48, 0, 16, 16, false));
                BigPiecesBrushes.Add(Pieces.TetriminoO, ExtractPieceBrush(Pieces.TetriminoO, image, 16, 0, 16, 16, false));
                BigPiecesBrushes.Add(Pieces.TetriminoS, ExtractPieceBrush(Pieces.TetriminoS, image, 0, 0, 16, 16, false));
                BigPiecesBrushes.Add(Pieces.TetriminoT, ExtractPieceBrush(Pieces.TetriminoT, image, 16, 0, 16, 16, false));
                BigPiecesBrushes.Add(Pieces.TetriminoZ, ExtractPieceBrush(Pieces.TetriminoZ, image, 64, 0, 16, 16, false));
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
                BigSpecialsBrushes.Add(Specials.Immunity, ExtractSpecialBrush(Specials.ClearColumn, image, 240, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.Darkness, ExtractSpecialBrush(Specials.Darkness, image, 256, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.Confusion, ExtractSpecialBrush(Specials.Darkness, image, 272, 0, 16, 16, false));
                BigSpecialsBrushes.Add(Specials.Mutation, ExtractSpecialBrush(Specials.Darkness, image, 288, 0, 16, 16, false));
                //BigSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, false)); // will be available when Left Gravity is implemented

                #endregion

                #region Small brushes

                // Background
                SmallBackground = ExtractBackground(image, 192, 24, 96, 176, true);
                // Tetriminoes
                SmallPiecesBrushes.Add(Pieces.TetriminoI, ExtractPieceBrush(Pieces.TetriminoI, image, 0, 16, 8, 8, true));
                SmallPiecesBrushes.Add(Pieces.TetriminoJ, ExtractPieceBrush(Pieces.TetriminoJ, image, 16, 16, 8, 8, true));
                SmallPiecesBrushes.Add(Pieces.TetriminoL, ExtractPieceBrush(Pieces.TetriminoL, image, 24, 16, 8, 8, true));
                SmallPiecesBrushes.Add(Pieces.TetriminoO, ExtractPieceBrush(Pieces.TetriminoO, image, 8, 16, 8, 8, true));
                SmallPiecesBrushes.Add(Pieces.TetriminoS, ExtractPieceBrush(Pieces.TetriminoS, image, 0, 16, 8, 8, true));
                SmallPiecesBrushes.Add(Pieces.TetriminoT, ExtractPieceBrush(Pieces.TetriminoT, image, 8, 16, 8, 8, true));
                SmallPiecesBrushes.Add(Pieces.TetriminoZ, ExtractPieceBrush(Pieces.TetriminoZ, image, 32, 16, 8, 8, true));
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
                SmallSpecialsBrushes.Add(Specials.Immunity, ExtractSpecialBrush(Specials.ClearColumn, image, 120, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.Darkness, ExtractSpecialBrush(Specials.Darkness, image, 128, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.Confusion, ExtractSpecialBrush(Specials.Confusion, image, 136, 16, 8, 8, true));
                SmallSpecialsBrushes.Add(Specials.Mutation, ExtractSpecialBrush(Specials.Mutation, image, 144, 16, 8, 8, true));
                //SmallSpecialsBrushes.Add(Specials.ZebraField, CreateDummySpecialBrush(Specials.ZebraField, true)); // will be available when Left Gravity is implemented

                #endregion
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Invalid texture file {0}. Exception: {1}", filename, ex);

                // Set default values
                BigBackground = new SolidColorBrush(Colors.Black);
                BigPiecesBrushes.Add(Pieces.TetriminoI, new SolidColorBrush(Colors.Blue));
                BigPiecesBrushes.Add(Pieces.TetriminoJ, new SolidColorBrush(Colors.Green));
                BigPiecesBrushes.Add(Pieces.TetriminoL, new SolidColorBrush(Colors.Magenta));
                BigPiecesBrushes.Add(Pieces.TetriminoO, new SolidColorBrush(Colors.Yellow));
                BigPiecesBrushes.Add(Pieces.TetriminoS, new SolidColorBrush(Colors.Blue));
                BigPiecesBrushes.Add(Pieces.TetriminoT, new SolidColorBrush(Colors.Yellow));
                BigPiecesBrushes.Add(Pieces.TetriminoZ, new SolidColorBrush(Colors.Red));
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
                SmallPiecesBrushes.Add(Pieces.TetriminoI, new SolidColorBrush(Colors.Blue));
                SmallPiecesBrushes.Add(Pieces.TetriminoJ, new SolidColorBrush(Colors.Green));
                SmallPiecesBrushes.Add(Pieces.TetriminoL, new SolidColorBrush(Colors.Magenta));
                SmallPiecesBrushes.Add(Pieces.TetriminoO, new SolidColorBrush(Colors.Yellow));
                SmallPiecesBrushes.Add(Pieces.TetriminoS, new SolidColorBrush(Colors.Blue));
                SmallPiecesBrushes.Add(Pieces.TetriminoT, new SolidColorBrush(Colors.Yellow));
                SmallPiecesBrushes.Add(Pieces.TetriminoZ, new SolidColorBrush(Colors.Red));
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
        public Brush GetBigPiece(Pieces piece)
        {
            Brush brush;
            if (BigPiecesBrushes == null || !BigPiecesBrushes.TryGetValue(piece, out brush))
                brush = CreateDummyPieceBrush(piece, false);
            return brush;
        }

        public Brush GetSmallPiece(Pieces piece)
        {
            Brush brush;
            if (SmallPiecesBrushes == null || !SmallPiecesBrushes.TryGetValue(piece, out brush))
                brush = CreateDummyPieceBrush(piece, true);
            return brush;
        }

        public Brush GetBigSpecial(Specials special)
        {
            Brush brush;
            if (BigSpecialsBrushes == null || !BigSpecialsBrushes.TryGetValue(special, out brush))
                brush = CreateDummySpecialBrush(special, false);
            return brush;
        }

        public Brush GetSmallSpecial(Specials special)
        {
            Brush brush;
            if (SmallSpecialsBrushes == null || !SmallSpecialsBrushes.TryGetValue(special, out brush))
                brush = CreateDummySpecialBrush(special, true);
            return brush;
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

        private static Brush ExtractPieceBrush(Pieces piece, BitmapImage image, int posX, int posY, int width, int height, bool isSmall)
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
                Log.WriteLine(Log.LogLevels.Error, "Error while extracting texture for {0}. Image {1}. Exception: {2}", piece, image.BaseUri, ex);
                specialBrush = CreateDummyPieceBrush(piece, isSmall);
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

        private static Brush ReadPieceBrush(Pieces piece, string filename, bool isSmall)
        {
            Brush brush;
            try
            {
                brush = LoadBrushFromFile(filename);
            }
            catch (Exception ex)
            {
                Log.WriteLine(Log.LogLevels.Error, "Error while reading texture for {0}. File {1}. Exception: {2}", piece, filename, ex);
                brush = CreateDummyPieceBrush(piece, isSmall);
            }
            return brush;
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

        private static Brush CreateDummyPieceBrush(Pieces piece, bool isSmall)
        {
            switch (piece)
            {
                case Pieces.TetriminoI:
                    return new SolidColorBrush(Colors.Blue);
                case Pieces.TetriminoJ:
                    return new SolidColorBrush(Colors.Green);
                case Pieces.TetriminoL:
                    return new SolidColorBrush(Colors.Magenta);
                case Pieces.TetriminoO:
                    return new SolidColorBrush(Colors.Yellow);
                case Pieces.TetriminoS:
                    return new SolidColorBrush(Colors.Blue);
                case Pieces.TetriminoT:
                    return new SolidColorBrush(Colors.Yellow);
                case Pieces.TetriminoZ:
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
}
