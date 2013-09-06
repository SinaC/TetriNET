using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for OpponentGridCanvas.xaml
    /// </summary>
    public partial class OpponentGridCanvas : UserControl, INotifyPropertyChanged
    {
        // TODO: dynamically get width/height
        private const int ColumnsCount = 12;
        private const int RowsCount = 22;
        private const int CellWidth = 8;
        private const int CellHeight = 8;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        // BEWARE OF DPI ... WPF only accept 96 DPI image !!!!!!!  almost 4 hours lost to figure this out
        private readonly Uri _graphicsUri = new Uri(@"D:\Oldies\TetriNET\DATA\TNETBLKS.BMP", UriKind.Absolute); // TODO: dynamic or relative
        //private readonly Uri _graphicsUri = new Uri(@"D:\Oldies\TetriNET\x2mod\x2mod.bmp", UriKind.Absolute); // TODO: dynamic or relative

        private readonly object _lock = new object();

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);
        private static readonly SolidColorBrush SpecialColor = new SolidColorBrush(Colors.LightGray);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("OpponentGridCanvasClientProperty", typeof(IClient), typeof(OpponentGridCanvas), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private int _playerId;
        public int PlayerId {
            get { return _playerId; }
            set
            {
                if (_playerId != value)
                {
                    _playerId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _playerName;
        public string PlayerName {
            get { return _playerName; }
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly Dictionary<Tetriminos, ImageBrush> _tetriminosBrushes = new Dictionary<Tetriminos, ImageBrush>();
        private readonly Dictionary<Specials, ImageBrush> _specialsBrushes = new Dictionary<Specials, ImageBrush>();
        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public OpponentGridCanvas()
        {
            ImageBrush backgroundBrush;
            InitializeComponent();

            PlayerId = -1;
            PlayerName = "Not playing"; // default value

            BuildTextures(_graphicsUri, out backgroundBrush, _tetriminosBrushes, _specialsBrushes);

            for (int y = 0; y < RowsCount; y++)
                for (int x = 0; x < ColumnsCount; x++)
                {
                    int canvasLeft = x * (CellWidth + MarginWidth);
                    int canvasTop = y * (CellHeight + MarginHeight);
                    // Rectangle
                    Rectangle rect = new Rectangle
                    {
                        Width = CellWidth,
                        Height = CellHeight,
                        Fill = TransparentColor,
                    };
                    _grid.Add(rect);
                    Canvas.Children.Add(rect);
                    Canvas.SetLeft(rect, canvasLeft);
                    Canvas.SetTop(rect, canvasTop);
                }
            //Canvas.Width = ColumnsCount * (CellWidth + MarginWidth);
            //Canvas.Height = RowsCount * (CellHeight + MarginHeight);
            Canvas.Background = backgroundBrush;
        }

        private void DrawGrid(IBoard board)
        {
            if (board == null)
                return;
            for (int y = 1; y <= board.Height; y++)
                for (int x = 1; x <= board.Width; x++)
                {
                    int cellY = board.Height - y;
                    int cellX = x - 1;
                    byte cellValue = board[x, y];

                    Rectangle uiPart = GetControl(cellX, cellY);
                    if (cellValue == CellHelper.EmptyCell)
                        uiPart.Fill = TransparentColor;
                    else
                    {
                        Specials special = CellHelper.GetSpecial(cellValue);
                        Tetriminos color = CellHelper.GetColor(cellValue);

                        if (special == Specials.Invalid)
                            uiPart.Fill = _tetriminosBrushes[color];
                        else
                            uiPart.Fill = _specialsBrushes[special];
                    }
                }
        }

        private void ClearGrid()
        {
            foreach (Rectangle uiPart in _grid)
                uiPart.Fill = TransparentColor;
        }

        private Rectangle GetControl(int cellX, int cellY)
        {
            return _grid[cellX + cellY * ColumnsCount];
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            OpponentGridCanvas _this = sender as OpponentGridCanvas;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnRedrawBoard -= _this.OnRedrawBoard;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnRedrawBoard += _this.OnRedrawBoard;
                }
            }
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                Visibility = Visibility.Visible;
                ClearGrid();
            });
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            if (playerId == PlayerId)
                ExecuteOnUIThread.Invoke(() => DrawGrid(board));
        }

        private void BuildTextures(Uri graphicsUri, out ImageBrush background, IDictionary<Tetriminos, ImageBrush> tetriminosBrushes, IDictionary<Specials, ImageBrush> specialsBrushes)
        {
            BitmapImage image = new BitmapImage(graphicsUri);
            // Background
            background = new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(192, 24, 96, 176),
                Stretch = Stretch.None,
            };
            // Tetriminos
            tetriminosBrushes.Add(Tetriminos.TetriminoI, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 16, 8, 8),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoJ, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(16, 16, 8, 8),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoL, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(24, 16, 8, 8),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoO, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(8, 16, 8, 8),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoS, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 16, 8, 8),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoT, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(8, 16, 8, 8),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoZ, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(32, 16, 8, 8),
                Stretch = Stretch.None
            });
            // Specials
            //ACNRSBGQO
            _specialsBrushes.Add(Specials.AddLines, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(40, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.ClearLines, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(48, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.NukeField, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(56, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.RandomBlocksClear, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(64, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.SwitchFields, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(72, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.ClearSpecialBlocks, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(80, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.BlockGravity, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(88, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.BlockQuake, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(96, 16, 8, 8),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.BlockBomb, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(104, 16, 8, 8),
                Stretch = Stretch.None
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
