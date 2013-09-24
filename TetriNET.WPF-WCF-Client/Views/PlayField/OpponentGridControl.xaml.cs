using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for OpponentGridControl.xaml
    /// </summary>
    public partial class OpponentGridControl : UserControl, INotifyPropertyChanged
    {
        private const int CellWidth = 8;
        private const int CellHeight = 8;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush ImmunityBorderColor = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("OpponentGridCanvasClientProperty", typeof(IClient), typeof(OpponentGridControl), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private Brush _borderColor;
        public Brush BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                OnPropertyChanged();
            }
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

        private bool _hasLost;
        public bool HasLost
        {
            get { return _hasLost; }
            set
            {
                if (_hasLost != value)
                {
                    _hasLost = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public OpponentGridControl()
        {
            InitializeComponent();

            PlayerId = -1;
            PlayerName = "Not playing";
            HasLost = false;
            
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Canvas.Background = TextureManager.TextureManager.TexturesSingleton.Instance.GetSmallBackground();
            }

            for (int y = 0; y < ClientOptionsViewModel.Height; y++)
                for (int x = 0; x < ClientOptionsViewModel.Width; x++)
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
            BorderColor = TransparentColor;
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
                        Pieces color = CellHelper.GetColor(cellValue);

                        if (special == Specials.Invalid)
                            uiPart.Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetSmallPiece(color);
                        else
                            uiPart.Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetSmallSpecial(special);
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
            if (cellX < 0 || cellX >= ClientOptionsViewModel.Width || cellY < 0 || cellY >= ClientOptionsViewModel.Height)
                return null;
            return _grid[cellX + cellY * ClientOptionsViewModel.Width];
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            OpponentGridControl @this = sender as OpponentGridControl;

            if (@this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnPlayerUnregistered -= @this.OnPlayerUnregistered;
                    oldClient.OnConnectionLost -= @this.OnConnectionLost;
                    oldClient.OnPlayerLost -= @this.OnPlayerLost;
                    oldClient.OnGameStarted -= @this.OnGameStarted;
                    oldClient.OnRedrawBoard -= @this.OnRedrawBoard;
                    oldClient.OnSpecialUsed -= @this.OnSpecialUsed;
                    oldClient.OnContinuousSpecialFinished -= @this.OnContinuousSpecialFinished;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                @this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerUnregistered += @this.OnPlayerUnregistered;
                    newClient.OnConnectionLost += @this.OnConnectionLost;
                    newClient.OnPlayerLost += @this.OnPlayerLost;
                    newClient.OnGameStarted += @this.OnGameStarted;
                    newClient.OnRedrawBoard += @this.OnRedrawBoard;
                    newClient.OnSpecialUsed += @this.OnSpecialUsed;
                    newClient.OnContinuousSpecialFinished += @this.OnContinuousSpecialFinished;
                }
                else
                {
                    @this.PlayerId = -1;
                    @this.PlayerName = "Not playing";
                }
            }
        }

        #region IClient events handler

        private void OnGameStarted()
        {
            HasLost = false;
            BorderColor = TransparentColor;
            ExecuteOnUIThread.Invoke(ClearGrid);
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            if (playerId == PlayerId && (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying))
                ExecuteOnUIThread.Invoke(() => DrawGrid(board));
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            if (playerId == PlayerId)
                HasLost = true;
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            PlayerId = -1;
            PlayerName = "Not playing";
            HasLost = false;
            BorderColor = TransparentColor;
        }

        private void OnPlayerUnregistered()
        {
            PlayerId = -1;
            PlayerName = "Not playing";
            HasLost = false;
            BorderColor = TransparentColor;
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            if (targetId == PlayerId && special == Specials.Immunity)
                BorderColor = ImmunityBorderColor;
        }

        private void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            if (playerId == PlayerId && special == Specials.Immunity)
                BorderColor = TransparentColor;
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
