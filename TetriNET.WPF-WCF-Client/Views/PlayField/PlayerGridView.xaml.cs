﻿using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Strategy;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Options;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for PlayerGridView.xaml
    /// </summary>
    public partial class PlayerGridView : UserControl
    {
        private const int CellWidth = 16;
        private const int CellHeight = 16;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush DarknessColor = new SolidColorBrush(Colors.Black);
        private static readonly SolidColorBrush ImmunityBorderColor = new SolidColorBrush(Colors.Green);
        private static readonly SolidColorBrush PieceAnchorColor = new SolidColorBrush(Colors.White);
        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);
        //private static readonly SolidColorBrush HintColor = new SolidColorBrush(Color.FromArgb( 128, 77, 115, 141)); // Some kind of gray/blue

        private readonly VisualBrush _hintBrush;
        private readonly VisualBrush _dropBrush;

        private bool _isDarknessActive;
        private bool _isHintActivated;
        //private bool _displayDropLocation;
        private IMoveStrategy _moveStrategy;
        private IPiece _pieceHint;
        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public PlayerViewModel ViewModel => DataContext as PlayerViewModel;

        public PlayerGridView()
        {
            InitializeComponent();

            _isHintActivated = false;
            //_displayDropLocation = false;

            if (!DesignMode.IsInDesignModeStatic)
                Canvas.Background = TextureManager.TextureManager.Instance.GetBigBackground();
            else
                Canvas.Background = new SolidColorBrush(Colors.Black);

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
            ActivateDarkness(false);
            ActivateImmunity(false);

            // Create hint brush
            _hintBrush = CreateDoubleRectangleBrush(Colors.GreenYellow);//Color.FromArgb(128, 77, 115, 141));

            // Create drop brush
            _dropBrush = CreateDoubleRectangleBrush(Colors.White);
        }

        public void ToggleHint()
        {
            _isHintActivated = !_isHintActivated;
            if (_isHintActivated)
            {
                _moveStrategy = _moveStrategy ?? new PierreDellacherieOnePiece();
                DrawHint();
            }
        }

        //public void ToggleDropLocation()
        //{
        //    _displayDropLocation = !_displayDropLocation;
        //    if (_displayDropLocation)
        //        DrawDropLocation();
        //}

        private void DrawPiece(IBoard board, IPiece piece, Brush brush)
        {
            for (int i = 1; i <= piece.TotalCells; i++)
            {
                int x, y;
                piece.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x - 1;

                Rectangle uiPart = GetControl(cellX, cellY);
                if (uiPart != null)
                    uiPart.Fill = brush;
            }
            // Draw piece center
            if (ClientOptionsViewModel.Instance.DisplayPieceAnchor && ClientOptionsViewModel.Instance.IsDeveloperModeActivated)
            {
                int cellY = board.Height - piece.PosY;
                int cellX = piece.PosX - 1;

                Rectangle uiPart = GetControl(cellX, cellY);
                if (uiPart != null)
                    uiPart.Fill = PieceAnchorColor;
            }
        }

        private void DrawHint()
        {
            if (ViewModel == null || ViewModel.Client.Board == null || ViewModel.Client.CurrentPiece == null || ViewModel.Client.NextPiece == null)
                return;
            if (_isHintActivated)
            {
                if (_pieceHint == null)
                {
                    // Clone board, current and next
                    IBoard board = ViewModel.Client.Board.Clone();
                    _pieceHint = ViewModel.Client.CurrentPiece.Clone();
                    IPiece next = ViewModel.Client.NextPiece.Clone();

                    // Get hint
                    int bestRotationDelta, bestTranslationDelta;
                    bool rotationBeforeTranslation;
                    _moveStrategy.GetBestMove(board, _pieceHint, next, out bestRotationDelta, out bestTranslationDelta, out rotationBeforeTranslation);

                    // Perform move
                    if (rotationBeforeTranslation)
                    {
                        _pieceHint.Rotate(bestRotationDelta);
                        _pieceHint.Translate(bestTranslationDelta, 0);
                    }
                    else
                    {
                        _pieceHint.Translate(bestTranslationDelta, 0);
                        _pieceHint.Rotate(bestRotationDelta);
                    }
                    board.Drop(_pieceHint);
                }
                // Draw piece
                DrawPiece(ViewModel.Client.Board, _pieceHint, _hintBrush);
            }
        }

        private void DrawDropLocation()
        {
            if (ViewModel == null || ViewModel.Client.Board == null || ViewModel.Client.CurrentPiece == null)
                return;
            //if (_displayDropLocation)
            if (ClientOptionsViewModel.Instance.DisplayDropLocation)
            {
                IBoard board = ViewModel.Client.Board.Clone();
                IPiece piece = ViewModel.Client.CurrentPiece.Clone();

                board.Drop(piece);

                DrawPiece(ViewModel.Client.Board, piece, _dropBrush);
            }
        }

        private void DrawCurrentPiece()
        {
            if (ViewModel == null || ViewModel.Client.Board == null || ViewModel.Client.CurrentPiece == null)
                return;
            Pieces cellPiece = ViewModel.Client.CurrentPiece.Value;
            Brush brush = TextureManager.TextureManager.Instance.GetBigPiece(cellPiece);
            DrawPiece(ViewModel.Client.Board, ViewModel.Client.CurrentPiece, brush);
        }

        private void DrawGrid()
        {
            if (ViewModel == null || ViewModel.Client.Board == null)
                return;
            for (int y = 1; y <= ViewModel.Client.Board.Height; y++)
                for (int x = 1; x <= ViewModel.Client.Board.Width; x++)
                {
                    int cellY = ViewModel.Client.Board.Height - y;
                    int cellX = x - 1;
                    byte cellValue = ViewModel.Client.Board[x, y];

                    Rectangle uiPart = GetControl(cellX, cellY);
                    if (uiPart != null)
                    {
                        if (cellValue == CellHelper.EmptyCell)
                            uiPart.Fill = TransparentColor;
                        else
                        {
                            Specials special = CellHelper.GetSpecial(cellValue);
                            Pieces color = CellHelper.GetColor(cellValue);

                            if (special == Specials.Invalid)
                                uiPart.Fill = TextureManager.TextureManager.Instance.GetBigPiece(color);
                            else
                                uiPart.Fill = TextureManager.TextureManager.Instance.GetBigSpecial(special);
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
                DrawDropLocation();
                DrawCurrentPiece();
            }
            else
            {
                ClearGrid();
                DrawCurrentPiece();
            }
        }

        private Rectangle GetControl(int cellX, int cellY)
        {
            if (cellX < 0 || cellX >= ClientOptionsViewModel.Width || cellY < 0 || cellY >= ClientOptionsViewModel.Height)
                return null;
            return _grid[cellX + cellY * ClientOptionsViewModel.Width];
        }

        private void ActivateDarkness(bool active)
        {
            _isDarknessActive = active;
            Canvas.Background = active ? DarknessColor : TextureManager.TextureManager.Instance.GetBigBackground();
            DrawEverything();
        }

        private void ActivateImmunity(bool active)
        {
            Border.BorderBrush = active ? ImmunityBorderColor : TransparentColor;
        }

        private void DesactivateContinuousSpecials()
        {
            ActivateDarkness(false);
            ActivateImmunity(false);
        }

        #region IClient events handler

        private void OnContinuousEffectToggled(Specials special, bool active, double duration)
        {
            if (special == Specials.Immunity)
                ExecuteOnUIThread.Invoke(() => ActivateImmunity(active));
            else if (special == Specials.Darkness)
                ExecuteOnUIThread.Invoke(() => ActivateDarkness(active));
        }

        private void OnRedraw()
        {
            _pieceHint = null; // reset hint
            ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnPieceMoved()
        {
            ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnRoundStarted()
        {
            _pieceHint = null; // reset hint
            ExecuteOnUIThread.Invoke(DrawEverything);
        }

        private void OnGameStarted()
        {
            //_displayDropLocation = ClientOptionsViewModel.Instance.DisplayDropLocation;
            _pieceHint = null; // reset hint
            ExecuteOnUIThread.Invoke(DesactivateContinuousSpecials);
            ExecuteOnUIThread.Invoke(() => PauseText.Visibility = Visibility.Hidden);
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ExecuteOnUIThread.Invoke(DesactivateContinuousSpecials);
        }

        private void OnPlayerUnregistered()
        {
            ExecuteOnUIThread.Invoke(DesactivateContinuousSpecials);
            ExecuteOnUIThread.Invoke(() => PauseText.Visibility = Visibility.Hidden);
        }

        private void OnGameOver()
        {
            ExecuteOnUIThread.Invoke(DesactivateContinuousSpecials);
            ExecuteOnUIThread.Invoke(() => PauseText.Visibility = Visibility.Hidden);
        }

        private void OnGameFinished(GameStatistics statistics)
        {
            ExecuteOnUIThread.Invoke(DesactivateContinuousSpecials);
            ExecuteOnUIThread.Invoke(() => PauseText.Visibility = Visibility.Hidden);
        }

        private void OnGamePaused()
        {
            ExecuteOnUIThread.Invoke(() => PauseText.Visibility = Visibility.Visible);
        }

        private void OnGameResumed()
        {
            ExecuteOnUIThread.Invoke(() => PauseText.Visibility = Visibility.Hidden);
        }

        #endregion

        #region DataContext + Client
        private void PlayerGridControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Crappy workaround MVVM -> code behind
            if (ViewModel != null)
            {
                ViewModel.ClientChanged += OnClientChanged;

                if (ViewModel.Client != null)
                    OnClientChanged(null, ViewModel.Client);
            }
        }

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            // Remove old handlers
            if (oldClient != null)
            {
                oldClient.GameOver -= OnGameOver;
                oldClient.GameFinished -= OnGameFinished;
                oldClient.GamePaused -= OnGamePaused;
                oldClient.GameResumed -= OnGameResumed;
                oldClient.PlayerUnregistered -= OnPlayerUnregistered;
                oldClient.ConnectionLost -= OnConnectionLost;
                oldClient.GameStarted -= OnGameStarted;
                oldClient.RoundStarted -= OnRoundStarted;
                oldClient.PieceMoved -= OnPieceMoved;
                oldClient.Redraw -= OnRedraw;
                oldClient.ContinuousEffectToggled -= OnContinuousEffectToggled;

            }
            // Add new handlers
            if (newClient != null)
            {
                newClient.GameOver += OnGameOver;
                newClient.GameFinished += OnGameFinished;
                newClient.GamePaused += OnGamePaused;
                newClient.GameResumed += OnGameResumed;
                newClient.PlayerUnregistered += OnPlayerUnregistered;
                newClient.ConnectionLost += OnConnectionLost;
                newClient.GameStarted += OnGameStarted; // will not be called on first game played because OnClientChanged is called on DataContextChanged (which is set when view receive focus) 
                newClient.RoundStarted += OnRoundStarted;
                newClient.PieceMoved += OnPieceMoved;
                newClient.Redraw += OnRedraw;
                newClient.ContinuousEffectToggled += OnContinuousEffectToggled;
            }
        }
        #endregion

        private static VisualBrush CreateDoubleRectangleBrush(Color color)
        {
            Canvas canvas = new Canvas
            {
                Width = 16,
                Height = 16,
            };
            Rectangle outer = new Rectangle
            {
                Width = 16,
                Height = 16,
                Fill = TransparentColor,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1,

            };
            Rectangle inner = new Rectangle
            {
                Width = 12,
                Height = 12,
                Fill = TransparentColor,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = 1,
            };
            canvas.Children.Add(outer);
            Canvas.SetLeft(outer, 0);
            Canvas.SetTop(outer, 0);
            canvas.Children.Add(inner);
            Canvas.SetLeft(inner, 2);
            Canvas.SetTop(inner, 2);

            return new VisualBrush
            {
                Visual = canvas
            };
        }
    }
}
