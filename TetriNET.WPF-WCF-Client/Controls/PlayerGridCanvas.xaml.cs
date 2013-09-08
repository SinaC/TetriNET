using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for PlayerGrid.xaml
    /// </summary>
    public partial class PlayerGridCanvas : UserControl, INotifyPropertyChanged
    {
        // TODO: dynamically get width/height
        private const int ColumnsCount = 12;
        private const int RowsCount = 22;
        private const int CellWidth = 16;
        private const int CellHeight = 16;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("PlayerGridCanvasClientProperty", typeof(IClient), typeof(PlayerGridCanvas), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient) GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private Visibility _playerIdVisibility;
        public Visibility PlayerIdVisibility
        {
            get { return _playerIdVisibility; }
            set
            {
                if (_playerIdVisibility != value)
                {
                    _playerIdVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _playerId;
        public int PlayerId
        {
            get { return _playerId+1; }
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
        public string PlayerName
        {
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

        public PlayerGridCanvas()
        {
            InitializeComponent();

            PlayerId = -1;
            PlayerName = "Not registered";
            PlayerIdVisibility = Visibility.Hidden;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                ImageBrush backgroundBrush;
                BuildTextures(new Uri(ConfigurationManager.AppSettings["texture"]), out backgroundBrush, _tetriminosBrushes, _specialsBrushes);
                Canvas.Background = backgroundBrush;
            }

            for(int y = 0; y < RowsCount; y++)
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
        }

        private void DrawCurrentTetrimino()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;
            ITetrimino currentTetrimino = Client.CurrentTetrimino;
            if (currentTetrimino == null)
                return;
            Tetriminos cellTetrimino = Client.CurrentTetrimino.Value;
            for (int i = 1; i <= Client.CurrentTetrimino.TotalCells; i++)
            {
                int x, y;
                Client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x - 1;

                Rectangle uiPart = GetControl(cellX, cellY);
                uiPart.Fill = _tetriminosBrushes[cellTetrimino];
            }
        }

        private void DrawGrid()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
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
            PlayerGridCanvas _this = sender as PlayerGridCanvas;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnRoundStarted -= _this.OnRoundStarted;
                    oldClient.OnTetriminoMoved -= _this.OnTetriminoMoved;
                    oldClient.OnRedraw -= _this.OnRedraw;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    _this.PlayerName = newClient.Name;
                    _this.PlayerId = newClient.PlayerId;
                    _this.PlayerIdVisibility = Visibility.Visible;

                    newClient.OnConnectionLost += _this.OnConnectionLost;
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnRoundStarted += _this.OnRoundStarted;
                    newClient.OnTetriminoMoved += _this.OnTetriminoMoved;
                    newClient.OnRedraw += _this.OnRedraw;
                }
                else
                {
                    _this.PlayerName = "Not registered";
                    _this.PlayerId = -1;
                    _this.PlayerIdVisibility = Visibility.Hidden;
                }
            }
        }

        private void OnRedraw()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                DrawGrid();
                DrawCurrentTetrimino();
            });
        }

        private void OnTetriminoMoved()
        {
            OnRedraw();
        }

        private void OnRoundStarted()
        {
            OnRedraw();
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                ClearGrid();
                DrawCurrentTetrimino();
            });
        }

        private void OnConnectionLost()
        {
            PlayerId = -1;
            PlayerName = "Not registered";
            PlayerIdVisibility = Visibility.Hidden;
        }

        private void BuildTextures(Uri graphicsUri, out ImageBrush background, IDictionary<Tetriminos, ImageBrush> tetriminosBrushes, IDictionary<Specials, ImageBrush> specialsBrushes)
        {
            BitmapImage image = new BitmapImage(graphicsUri);
            // Background
            background = new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 24, 192, 352),
                Stretch = Stretch.None,
            };
            // Tetriminos
            tetriminosBrushes.Add(Tetriminos.TetriminoI, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 16, 16),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoJ, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(32, 0, 16, 16),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoL, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(48, 0, 16, 16),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoO, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(16, 0, 16, 16),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoS, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 16, 16),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoT, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(16, 0, 16, 16),
                Stretch = Stretch.None
            });
            tetriminosBrushes.Add(Tetriminos.TetriminoZ, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(64, 0, 16, 16),
                Stretch = Stretch.None
            });
            // Specials
            //ACNRSBGQO
            specialsBrushes.Add(Specials.AddLines, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(80, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.ClearLines, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(96, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.NukeField, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(112, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.RandomBlocksClear, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(128, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.SwitchFields, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(144, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.ClearSpecialBlocks, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(160, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.BlockGravity, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(176, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.BlockQuake, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(192, 0, 16, 16),
                Stretch = Stretch.None
            });
            specialsBrushes.Add(Specials.BlockBomb, new ImageBrush(image)
            {
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(208, 0, 16, 16),
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
