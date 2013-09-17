﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;
using TetriNET.Strategy;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for PlayerGridControl.xaml
    /// </summary>
    public partial class PlayerGridControl : UserControl, INotifyPropertyChanged
    {
        private const int CellWidth = 16;
        private const int CellHeight = 16;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);
        private static readonly SolidColorBrush HintColor = new SolidColorBrush(Color.FromArgb( 128, 77, 115, 141)); // Some kind of gray/blue

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("PlayerGridCanvasClientProperty", typeof(IClient), typeof(PlayerGridControl), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient) GetValue(ClientProperty); }
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

        public string PlayerName
        {
            get { return Client == null || !Client.IsRegistered ? "Not registered" : Client.Name; }
        }

        private int _playerId;
        public int PlayerId
        {
            get { return _playerId; }
            set
            {
                if (_playerId != value)
                {
                    _playerId = value;
                    OnPropertyChanged();
                    OnPropertyChanged("DisplayPlayerId");
                    OnPropertyChanged("IsPlayerIdVisible");
                    OnPropertyChanged("PlayerName");
                }
            }
        }

        private bool _isDarknessActive;
        private bool _isHintActivated;
        private IMoveStrategy _moveStrategy;
        private ITetrimino _tetriminoHint;
        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public PlayerGridControl()
        {
            InitializeComponent();

            _isHintActivated = false;

            PlayerId = -1;

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Canvas.Background = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigBackground();
            }

            for(int y = 0; y < Models.Options.Height; y++)
                for (int x = 0; x < Models.Options.Width; x++)
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

        public void ToggleHint()
        {
            _isHintActivated = !_isHintActivated;
            if (_isHintActivated)
                _moveStrategy = _moveStrategy ?? new PierreDellacherieOnePiece();
        }

        private void DrawTetrimino(IBoard board, ITetrimino tetrimono, Brush brush)
        {
            for (int i = 1; i <= tetrimono.TotalCells; i++)
            {
                int x, y;
                tetrimono.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x - 1;

                Rectangle uiPart = GetControl(cellX, cellY);
                if (uiPart != null)
                    uiPart.Fill = brush;
            }
        }

        private void DrawHint()
        {
            if(_isHintActivated)
            {
                if (_tetriminoHint == null)
                {
                    // Clone board, current and next
                    IBoard board = Client.Board.Clone();
                    _tetriminoHint = Client.CurrentTetrimino.Clone();
                    ITetrimino next = Client.NextTetrimino.Clone();

                    // Get hint
                    int bestRotationDelta, bestTranslationDelta;
                    bool rotationBeforeTranslation;
                    _moveStrategy.GetBestMove(board, _tetriminoHint, next, out bestRotationDelta, out bestTranslationDelta, out rotationBeforeTranslation);

                    // Perform move
                    if (rotationBeforeTranslation)
                    {
                        _tetriminoHint.Rotate(bestRotationDelta);
                        _tetriminoHint.Translate(bestTranslationDelta, 0);
                    }
                    else
                    {
                        _tetriminoHint.Translate(bestTranslationDelta, 0);
                        _tetriminoHint.Rotate(bestRotationDelta);
                    }
                    board.Drop(_tetriminoHint);
                }
                // Draw tetrimino
                DrawTetrimino(Client.Board, _tetriminoHint, HintColor); // FF, C0, CB
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
            Brush brush = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigTetrimino(cellTetrimino);
            DrawTetrimino(board, currentTetrimino, brush);
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
                        if (uiPart != null)
                        {
                            if (cellValue == CellHelper.EmptyCell)
                                uiPart.Fill = TransparentColor;
                            else
                            {
                                Specials special = CellHelper.GetSpecial(cellValue);
                                Tetriminos color = CellHelper.GetColor(cellValue);

                                if (special == Specials.Invalid)
                                    uiPart.Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigTetrimino(color);
                                else
                                    uiPart.Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigSpecial(special);
                            }
                        }
                    }
        }

        private void ClearGrid()
        {
            foreach (Rectangle uiPart in _grid)
                uiPart.Fill = TransparentColor;
        }

        private void DrawEverything()
        {
            if (!_isDarknessActive)
            {
                DrawGrid();
                DrawHint();
                DrawCurrentTetrimino();
            }
            else
            {
                ClearGrid();
                DrawCurrentTetrimino();
            }
        }

        private Rectangle GetControl(int cellX, int cellY)
        {
            if (cellX < 0 || cellX >= Models.Options.Width || cellY < 0 || cellY >= Models.Options.Height)
                return null;
            return _grid[cellX + cellY * Models.Options.Width];
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PlayerGridControl _this = sender as PlayerGridControl;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnPlayerRegistered -= _this.OnPlayerRegistered;
                    oldClient.OnPlayerUnregistered -= _this.OnPlayerUnregistered;
                    oldClient.OnConnectionLost -= _this.OnConnectionLost;
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnRoundStarted -= _this.OnRoundStarted;
                    oldClient.OnTetriminoMoved -= _this.OnTetriminoMoved;
                    oldClient.OnRedraw -= _this.OnRedraw;
                    oldClient.OnDarknessToggled -= _this.OnDarknessToggled;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnPlayerRegistered += _this.OnPlayerRegistered;
                    newClient.OnPlayerUnregistered += _this.OnPlayerUnregistered;
                    newClient.OnConnectionLost += _this.OnConnectionLost;
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnRoundStarted += _this.OnRoundStarted;
                    newClient.OnTetriminoMoved += _this.OnTetriminoMoved;
                    newClient.OnRedraw += _this.OnRedraw;
                    newClient.OnDarknessToggled += _this.OnDarknessToggled;
                }
            }
        }

        #region IClient events handler
        private void OnDarknessToggled(bool active)
        {
            _isDarknessActive = active;
            if (active)
            {
                ExecuteOnUIThread.Invoke(() =>
                    {
                        ClearGrid();
                        DrawCurrentTetrimino();
                    });
            }
            else
                ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnRedraw()
        {
            _tetriminoHint = null; // reset hint
            ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnTetriminoMoved()
        {
            ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnRoundStarted()
        {
            _tetriminoHint = null; // reset hint
            ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnGameStarted()
        {
            _tetriminoHint = null; // reset hint>
            ExecuteOnUIThread.Invoke(() =>
            {
                ClearGrid();
                DrawCurrentTetrimino();
            });
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            PlayerId = -1;
        }

        private void OnPlayerRegistered(bool succeeded, int playerId)
        {
            if (succeeded)
            {
                PlayerId = playerId;
            }
        }

        private void OnPlayerUnregistered()
        {
            PlayerId = -1;
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
