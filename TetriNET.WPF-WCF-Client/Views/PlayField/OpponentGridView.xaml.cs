using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Options;
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for OpponentGridView.xaml
    /// </summary>
    public partial class OpponentGridView : UserControl
    {
        private const int CellWidth = 8;
        private const int CellHeight = 8;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush ImmunityBorderColor = new SolidColorBrush(Colors.Red);
        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public OpponentGridView()
        {
            InitializeComponent();

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
            ResetImmunity();
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

        private void SetImmunity()
        {
            Border.BorderBrush = ImmunityBorderColor;
            ImmunityDisplay.Visibility = Visibility.Visible;
        }

        private void ResetImmunity()
        {
            Border.BorderBrush = TransparentColor;
            ImmunityDisplay.Visibility = Visibility.Hidden;
        }

        #region IClient events handler

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    ClearGrid();
                    ResetImmunity();
                });
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                OpponentViewModel vm = DataContext as OpponentViewModel; // <-- this may cause cross-thread exception
                if (vm == null)
                    return;
                if (playerId == vm.PlayerId && (vm.Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying))
                    DrawGrid(board);
            });
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            ExecuteOnUIThread.Invoke(ResetImmunity);

        }

        private void OnPlayerUnregistered()
        {
            ExecuteOnUIThread.Invoke(ResetImmunity);
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                OpponentViewModel vm = DataContext as OpponentViewModel;
                if (vm == null)
                    return;
                if (targetId == vm.PlayerId && special == Specials.Immunity && (vm.Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying))
                    SetImmunity();
            });
        }

        private void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                OpponentViewModel vm = DataContext as OpponentViewModel;
                if (vm == null)
                    return;
                if (playerId == vm.PlayerId && special == Specials.Immunity)
                    ResetImmunity();
            });
        }

        #endregion

        #region DataContext + Client
        private void OpponentGridControl_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Crappy workaround MVVM -> code behind
            OpponentViewModel vm = DataContext as OpponentViewModel;
            if (vm != null)
            {
                vm.ClientChanged += OnClientChanged;

                if (vm.Client != null)
                    OnClientChanged(null, vm.Client);
            }
        }

        private void OnClientChanged(IClient oldClient, IClient newClient)
        {
            // Remove old handlers
            if (oldClient != null)
            {
                oldClient.OnPlayerUnregistered -= OnPlayerUnregistered;
                oldClient.OnConnectionLost -= OnConnectionLost;
                oldClient.OnGameStarted -= OnGameStarted;
                oldClient.OnRedrawBoard -= OnRedrawBoard;
                oldClient.OnSpecialUsed -= OnSpecialUsed;
                oldClient.OnContinuousSpecialFinished -= OnContinuousSpecialFinished;
            }
            // Add new handlers
            if (newClient != null)
            {
                newClient.OnPlayerUnregistered += OnPlayerUnregistered;
                newClient.OnConnectionLost += OnConnectionLost;
                newClient.OnGameStarted += OnGameStarted;
                newClient.OnRedrawBoard += OnRedrawBoard;
                newClient.OnSpecialUsed += OnSpecialUsed;
                newClient.OnContinuousSpecialFinished += OnContinuousSpecialFinished;
            }
        }
        #endregion
    }
}
