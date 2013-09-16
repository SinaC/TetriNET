using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for OpponentGridControl.xaml
    /// </summary>
    public partial class OpponentGridControl : UserControl, INotifyPropertyChanged
    {
        // TODO: dynamically get width/height
        private const int ColumnsCount = 12;
        private const int RowsCount = 22;
        private const int CellWidth = 8;
        private const int CellHeight = 8;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("OpponentGridCanvasClientProperty", typeof(IClient), typeof(OpponentGridControl), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public bool IsPlayerIdVisible
        {
            get { return PlayerId != -1; }
        }

        public int DisplayPlayerId
        {
            get { return PlayerId + 1; }
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
                    OnPropertyChanged("DisplayPlayerId");
                    OnPropertyChanged("IsPlayerIdVisible");
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

        private readonly Textures.Textures _textures;
        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public OpponentGridControl()
        {
            InitializeComponent();

            PlayerId = -1;
            PlayerName = "Not playing";
            
            _textures = Textures.Textures.TexturesSingleton.Instance;
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Canvas.Background = _textures.GetSmallBackground();
            }

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
                            uiPart.Fill = _textures.GetSmallTetrimino(color);
                        else
                            uiPart.Fill = _textures.GetSmallSpecial(special);
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
            OpponentGridControl _this = sender as OpponentGridControl;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnPlayerUnregistered -= _this.OnPlayerUnregistered;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnRedrawBoard -= _this.OnRedrawBoard;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerUnregistered += _this.OnPlayerUnregistered;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnRedrawBoard += _this.OnRedrawBoard;
                }
                else
                {
                    _this.PlayerId = -1;
                    _this.PlayerName = "Not playing";
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(ClearGrid);
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            if (playerId == PlayerId)
                ExecuteOnUIThread.Invoke(() => DrawGrid(board));
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            PlayerId = -1;
            PlayerName = "Not playing";
        }

        private void OnPlayerUnregistered()
        {
            PlayerId = -1;
            PlayerName = "Not playing";
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
