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
using TetriNET.WPF_WCF_Client.ViewModels.PlayField;

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

        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public OpponentGridControl()
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

        #region IClient events handler

        private void OnGameStarted()
        {
            BorderColor = TransparentColor;
            ExecuteOnUIThread.Invoke(ClearGrid);
        }

        private void OnRedrawBoard(int playerId, IBoard board)
        {
            OpponentViewModel vm = DataContext as OpponentViewModel;
            if (vm == null)
                return;
            if (playerId == vm.PlayerId && (vm.Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying))
                ExecuteOnUIThread.Invoke(() => DrawGrid(board));
        }

        private void OnConnectionLost(ConnectionLostReasons reason)
        {
            BorderColor = TransparentColor;
        }

        private void OnPlayerUnregistered()
        {
            BorderColor = TransparentColor;
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            OpponentViewModel vm = DataContext as OpponentViewModel;
            if (vm == null)
                return;
            if (targetId == vm.PlayerId && special == Specials.Immunity)
                BorderColor = ImmunityBorderColor;
        }

        private void OnContinuousSpecialFinished(int playerId, Specials special)
        {
            OpponentViewModel vm = DataContext as OpponentViewModel;
            if (vm == null)
                return;
            if (playerId == vm.PlayerId && special == Specials.Immunity)
                BorderColor = TransparentColor;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

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
    }
}
